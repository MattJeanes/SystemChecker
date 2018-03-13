using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Web.Helpers
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISecurityHelper _securityHelper;
        private readonly IRepository<ApiKey> _apiKeys;
        public AuthenticationMiddleware(RequestDelegate next, ISecurityHelper securityHelper, IRepository<ApiKey> apiKeys)
        {
            _next = next;
            _securityHelper = securityHelper;
            _apiKeys = apiKeys;
        }

        public async Task Invoke(HttpContext context)
        {
            if (
                !context.Request.Path.StartsWithSegments(new PathString("/api")) // Allow calls not to the api through
                || context.Request.Path.StartsWithSegments(new PathString("/api/login")) // Also the login api
                || context.Request.Path.StartsWithSegments(new PathString("/api/init")) // And the init api
                )
            {
                await _next(context);
                return;
            }

            var apiKey = context.Request.Headers["ApiKey"].FirstOrDefault();
            if (apiKey != null)
            {
                var key = await _apiKeys.GetAll().Include(x => x.User).FirstOrDefaultAsync(x => x.Key == apiKey);
                if (key == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Invalid API key" }, Formatting.Indented));
                    return;
                }
                else
                {
                    SetUsername(context, key.User.Username);
                }
            }
            else
            {
                try
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault()?.Remove(0, "Bearer ".Length);
                    if (authHeader == null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Authorization header missing, use ApiKey or Authorization JWT" }, Formatting.Indented));
                        return;
                    }
                    var token = await _securityHelper.ValidateToken(authHeader);
                    var username = _securityHelper.GetUsername(token);
                    SetUsername(context, username);
                    var renewAfter = token.ValidFrom.AddMinutes(token.ValidTo.Subtract(token.ValidFrom).TotalMinutes / 2);
                    if (DateTime.Now > renewAfter)
                    {
                        var newToken = await _securityHelper.GetToken(username);
                        context.Response.Headers["X-Token"] = newToken;
                    }
                }
                catch (Exception e)
                {
                    context.Response.Headers["X-Token-Invalid"] = "true";
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = e.Message }, Formatting.Indented));
                    return;
                }
            }

            await _next(context);
        }

        private void SetUsername(HttpContext context, string username)
        {
            context.Items["Username"] = username;
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
