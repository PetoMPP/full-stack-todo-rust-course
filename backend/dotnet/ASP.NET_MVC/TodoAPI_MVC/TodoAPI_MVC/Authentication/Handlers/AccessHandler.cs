using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public class AccessHandler : AuthorizationHandler<AccessRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AccessRequirement requirement)
        {
            if (context.User.Identity is not ClaimsIdentity identity ||
                !identity.Claims.Any(c => c.Type == "Access"))
            {
                context.Fail(new(this, "User doesn't have access level specified!"));
                return Task.CompletedTask;
            }

            if (!int.TryParse(context.User.Claims.First(c => c.Type == "Access").Value, out var access))
                context.Fail(new(this, "User has unrecognized access level!"));

            if (!((EndpointAccess)access).HasFlag(requirement.RequiredAccess))
            {
                context.Fail(new(this, "User doesn't have required access level!"));
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
