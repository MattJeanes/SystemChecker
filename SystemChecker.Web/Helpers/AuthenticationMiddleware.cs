using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model;

namespace SystemChecker.Web.Helpers
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        public AuthenticationMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            // Allow hub calls with an id through
            if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments(new PathString("/hub")))
            {
                await _next(context);
                return;
            }

            //var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            //var apiKey = context.Request.Headers["ApiKey"].FirstOrDefault();
            //if (apiKey != null)
            //{
            //    if (apiKey != _appSettings.ApiKey)
            //    {
            //        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            //        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Invalid API key" }, Formatting.Indented));
            //        return;
            //    }
            //}
            //else if (true) // TODO: Proper authentcation
            //{

            //}
            //else
            //{
            //    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    await context.Response.WriteAsync("Unauthorized");
            //    return;
            //}

            // TODO authentication

            await _next(context);
        }
    }

    public static class AuthenticationMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
