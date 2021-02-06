
namespace Arcmage.Server.Api.Middleware
{
    /// <summary>
    /// ApiExceptionMiddlewareOptions setting for the ApiExceptionMiddleware
    /// </summary>
    public class ApiExceptionMiddlewareOptions
    {
        /// <summary>
        /// Setting to show internal exception info (the stacktrace, in the http-result).
        /// Should not be used in production.
        /// </summary>
        public bool ShowDetailsInHttpResponse { get; set; }
    }
}
