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
            app.UseAuthenticationMiddleware();
            app.UseErrorHandlingMiddleware();

            if (_env.IsDevelopment())
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