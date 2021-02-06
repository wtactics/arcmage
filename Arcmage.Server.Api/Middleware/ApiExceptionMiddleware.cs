using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Arcmage.Server.Api.Middleware
{
    /// <summary>
    /// When an ApiException is thrown, the ApiExceptionMiddleware will include its message in the http response.
    /// When normal Exceptions are thrown, their details will be included based upon the ApiExceptionMiddleware configuration.
    /// </summary>
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiExceptionMiddlewareOptions _options;

        private static readonly ILogger Log = Serilog.Log.ForContext<ApiExceptionMiddleware>();

        public ApiExceptionMiddleware(RequestDelegate next, ApiExceptionMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An exception occurred while trying to process a {context.Request.Method} request to {context.Request.Path}");
                
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var message = $"{ex.Message}\n\n{ex.StackTrace}";

                if (!_options.ShowDetailsInHttpResponse)
                {
                    message = "An error occurred, please try again or contact the administrator.";
                }

                await context.Response.WriteAsync(message);
            }
        }
    }
}
