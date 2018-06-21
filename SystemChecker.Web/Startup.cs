using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using SystemChecker.Model;
using SystemChecker.Web.Helpers;
using SystemChecker.Web.Hubs;
using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace SystemChecker.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            _env = env;
            Configuration = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (Configuration.GetValue<bool>("UseHttps"))
            {
                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
                });
            }
            // Add framework services.
            services
                .AddMvc(options =>
                {
                    options.InputFormatters.Add(new TextPlainInputFormatter());
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddSignalR();
            services.AddSystemChecker(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseSecurityHeaders();
            if (Configuration.GetValue<bool>("UseHttps"))
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthenticationMiddleware();
            app.UseErrorHandlingMiddleware();

            if (_env.IsDevelopment())
            {
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ConfigFile = "webpack.config.ts",
                    EnvParam = new
                    {
                        aot = false // can't use AOT with HMR currently https://github.com/angular/angular-cli/issues/6347
                    }
                });
            }

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<DashboardHub>("/hub/dashboard");
                routes.MapHub<DetailsHub>("/hub/details");
                routes.MapHub<CheckHub>("/hub/check");
            });

            var dashboardHub = app.ApplicationServices.GetRequiredService<IHubContext<DashboardHub>>();
            var detailsHub = app.ApplicationServices.GetRequiredService<IHubContext<DetailsHub>>();
            var checkHub = app.ApplicationServices.GetRequiredService<IHubContext<CheckHub>>();

            var connectionMultiplexer = app.ApplicationServices.GetRequiredService<IConnectionMultiplexer>();
            var pubsub = connectionMultiplexer.GetSubscriber();
            pubsub.Subscribe("check", async (channel, value) =>
            {
                var checkID = (int)value;
                await dashboardHub.Clients.All.SendAsync("check", checkID);
                await detailsHub.Clients.All.SendAsync("check", checkID);
                await checkHub.Clients.All.SendAsync("check", checkID);
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