using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public class AccessHandler : VerboseAuthorizationHandler<AccessRequirement>
    {
        protected const string AccessMissingMessage = "User doesn't have access level specified!";
        protected const string AccessUnparsableMessage = "User has unrecognized access level!";
        protected const string AccessInvalidMessage = "User doesn't have required access level!";

        public AccessHandler(ILogger<AccessHandler> logger) : base(logger)
        {
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AccessRequirement requirement)
        {
            var identity = (ClaimsIdentity)context.User.Identity!;

            if (identity.Claims.FirstOrDefault(c => c.Type == "Access") is not Claim accessClaim)
            {
                FailAndCryAboutIt(context, AccessMissingMessage);
                return Task.CompletedTask;
            }

            if (!int.TryParse(accessClaim.Value, out var access))
                FailAndCryAboutIt(context, AccessUnparsableMessage);

            if (!((EndpointAccess)access).HasFlag(requirement.RequiredAccess))
            {
                FailAndCryAboutIt(context, AccessInvalidMessage);
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
