using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.JsonWebTokens;

namespace VPS.ControlCenter.Api.Extensions
{


    public class CustomAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly int[] _userTypeIds;

        public CustomAuthorizeAttribute(params int[] userTypeIds)
        {
            _userTypeIds = userTypeIds;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }

            // Check if the user has the required user type IDs
            if (!_userTypeIds.Any(id => user.HasClaim(c => c.Type == "UserTypeId" && c.Value == id.ToString())))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
                return;
            }

            // Check expiry time
            var expiryClaim = user.FindFirst(c => c.Type == JwtRegisteredClaimNames.Exp);
            if (expiryClaim != null && long.TryParse(expiryClaim.Value, out long expiryTime))
            {
                var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryTime).UtcDateTime;
                if (expiryDateTime < DateTime.UtcNow)
                {
                    context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                }
            }
        }

    }
}
