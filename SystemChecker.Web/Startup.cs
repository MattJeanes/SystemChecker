using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using ***REMOVED***;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Helpers;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Quartz;
using Quartz.Impl;
using SystemChecker.Model;
using Quartz.Spi;
using SystemChecker.Model.Jobs;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;
using AutoMapper.EquivalencyExpression;
using SystemChecker.Model.Hubs;
using SystemChecker.Model.Notifiers;

namespace SystemChecker.Web
{
    public class Startup
    {
        private IHostingEnvironment _env;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            _env = env;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddSignalR();

            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            var builder = new DbContextOptionsBuilder<CheckerContext>();
            builder.UseSqlServer(Configuration.GetConnectionString("SystemChecker"));
            services.AddTransient<ICheckerUow>(_ => new CheckerUow(new RepositoryProvider(new RepositoryFactories()), builder.Options));
            services.AddSingleton<IMapper>(_ => new Mapper(new MapperConfiguration(cfg => {
                cfg.AddProfile<MappingProfile>();
                cfg.AddCollectionMappers();
            })));
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<ISchedulerManager, SchedulerManager>();
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<IEncryptionHelper, EncryptionHelper>();
            services.AddSingleton<ICheckLogger, CheckLogger>();

            // Helpers
            services.AddTransient<ISettingsHelper, SettingsHelper>();
            services.AddTransient<ICheckerHelper, CheckerHelper>();
            services.AddTransient<ISlackHelper, SlackHelper>();

            // Jobs
            services.AddTransient<DatabaseChecker>();
            services.AddTransient<WebRequestChecker>();
            services.AddTransient<PingChecker>();
            services.AddTransient<CleanupJob>();

            // Notifiers
            services.AddTransient<SlackNotifier>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseErrorHandlingMiddleware();

            if (env.IsDevelopment())
            {
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ConfigFile = "webpack.dev.js"
                });
            }

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<DashboardHub>("hub/dashboard");
                routes.MapHub<DetailsHub>("hub/details");
                routes.MapHub<CheckHub>("hub/check");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
