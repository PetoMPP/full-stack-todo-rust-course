using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public class AccessHandler : VerboseAuthorizationHandler<AccessRequirement>
    {
        public AccessHandler(
            ILogger<AccessHandler> logger) : base(logger)
        {
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AccessRequirement requirement)
        {
            if (context.User.Identity is not ClaimsIdentity identity ||
                !identity.Claims.Any(c => c.Type == "Access"))
            {
                FailAndCryAboutIt(context, "User doesn't have access level specified!");
                return Task.CompletedTask;
            }

            if (!int.TryParse(context.User.Claims.First(c => c.Type == "Access").Value, out var access))
                FailAndCryAboutIt(context, "User has unrecognized access level!");

            if (!((EndpointAccess)access).HasFlag(requirement.RequiredAccess))
            {
                FailAndCryAboutIt(context, "User doesn't have required access level!");
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
