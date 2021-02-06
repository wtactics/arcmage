using System;
using Microsoft.AspNetCore.Builder;

namespace Arcmage.Server.Api.Middleware
{
    public static class ApiExceptionMiddlewareExtension
    {
        public static IApplicationBuilder UseApiExceptionMiddleware(this IApplicationBuilder app, Action<ApiExceptionMiddlewareOptions> configureOptions)
        {
            var options = new ApiExceptionMiddlewareOptions();
            configureOptions(options);
            return app.UseMiddleware<ApiExceptionMiddleware>(options);
        }
    }
}
