using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Arcmage.Game.Api.Utils
{
    public static class HttpContextExtensions
    {
        public static Guid GetUserGuid(this HttpContext httpContext)
        {
            var identity = httpContext.User.Identity as ClaimsIdentity;
            var guid = identity?.Name;
            if (guid != null) return Guid.Parse(guid);
            return Guid.Empty;
        }
    }
}
