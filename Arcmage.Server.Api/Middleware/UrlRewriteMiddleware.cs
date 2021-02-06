using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace Arcmage.Server.Api.Middleware
{
    public static class UrlRewriteMiddleware
    {
        /// <summary>
        /// Handle arrays of complex objects in query string. Popular client frameworks like angular and kendo send arrays in the format field[index][property],
        /// while asp.net core webapi can only handle the format field[index].property. Urls are transformed in the format webapi can handle.
        /// </summary>
        /// <param name="app"></param>
        public static void ApplyUrlRewrite(this IApplicationBuilder app)
        {
            app.UseRewriter(new RewriteOptions().Add(RewriteUrl));
        }

        private static void RewriteUrl(RewriteContext context)
        {
            if (context?.HttpContext?.Request?.QueryString == null) return;

            var queryString = WebUtility.UrlDecode(context.HttpContext.Request.QueryString.Value);

            var previousParamWasNumeric = false;
            var startBracketIndex = 0;
            while ((startBracketIndex = queryString.IndexOf("[", startBracketIndex, StringComparison.InvariantCultureIgnoreCase)) >= 0)
            {
                var endBracketIndex = queryString.IndexOf("]", startBracketIndex, StringComparison.InvariantCultureIgnoreCase) + 1;
                var param = queryString.Substring(startBracketIndex, endBracketIndex - startBracketIndex);
                var value = param.Substring(1, param.Length - 2);
                var currentParamIsNumeric = value.IsNumeric();

                if (!currentParamIsNumeric && previousParamWasNumeric)
                {
                    queryString = queryString.Substring(0, startBracketIndex) + $".{value}" + queryString.Substring(endBracketIndex);
                    endBracketIndex -= 1;
                }

                previousParamWasNumeric = currentParamIsNumeric;
                startBracketIndex = endBracketIndex;
            }

            context.HttpContext.Request.QueryString = new QueryString(queryString);
        }

        private static bool IsNumeric(this string input)
        {
            return int.TryParse(input, out int _);
        }

    }
}
