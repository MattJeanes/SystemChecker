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


            var builder = new DbContextOptionsBuilder<CheckerContext>();
            builder.UseSqlServer(Configuration.GetConnectionString("SystemChecker"));
            services.AddScoped<ICheckerUow>(_ => new CheckerUow(new RepositoryProvider(new RepositoryFactories()), builder.Options));
            services.AddSingleton<IMapper>(_ => new Mapper(new MapperConfiguration(x => x.AddProfile<MappingProfile>())));
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<ISchedulerManager, SchedulerManager>();
            services.AddSingleton<IJobFactory, JobFactory>();

            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Jobs
            services.AddTransient<ScheduleUpdater>();
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
