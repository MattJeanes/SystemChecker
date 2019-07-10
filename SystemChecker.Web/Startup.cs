using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moneybarn.Common.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using SystemChecker.Model;
using SystemChecker.Web.Helpers;
using SystemChecker.Web.Hubs;

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

        public void ConfigureServices(IServiceCollection services)
        {
            if (Configuration.GetValue<bool>("UseHttps"))
            {
                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
                });
            }
            services
                .AddMvc(options =>
                {
                    options.InputFormatters.Add(new TextPlainInputFormatter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddSignalR();
            services.AddSystemChecker(Configuration);

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (!_env.IsDevelopment())
            {
                app.UseSecurityHeaders();
            }
            if (Configuration.GetValue<bool>("UseHttps"))
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthenticationMiddleware();
            app.UseErrorHandlingMiddleware();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

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
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (_env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:42452");
                }
            });
        }
    }
}