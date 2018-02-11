using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SystemChecker.Web.Helpers
{
    public class ErrorHandlingMiddleware
    {
        private static readonly JsonSerializerSettings errorSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private static Task HandleException(HttpContext context, Exception exception)
        {
            var code = GetStatusCodeForException(exception);
            var result = GetErrorResponse(exception);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }

        public static HttpStatusCode GetStatusCodeForException(Exception e)
        {
            if (e is NotFoundException)
                return HttpStatusCode.NotFound;
            else if (e is UnauthorizedAccessException)
                return HttpStatusCode.Unauthorized;
            else
                return HttpStatusCode.InternalServerError;
        }

        public static string GetErrorResponse(Exception e)
        {
            return JsonConvert.SerializeObject(new { Error = e.Message, Stack = e.StackTrace }, errorSerializerSettings);
        }
    }

    public static class ErrorHandlingMiddlewareExtension
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }

    public class NotFoundException : Exception { public NotFoundException(string message) : base(message) { } };
}
