using Microsoft.AspNetCore.Authorization;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public class AccessHandler : AuthorizationHandler<AccessRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AccessRequirement requirement)
        {
            if (!context.User.Claims.Any(c => c.Type == "Access"))
            {
                context.Fail(new(this, "User doesn't have access level specified!"));
                return Task.CompletedTask;
            }

            var userAccess = (EndpointAccess)int.Parse(
                context.User.Claims.First(c => c.Type == "Access").Value);

            if (!userAccess.HasFlag(requirement.RequiredAccess))
            {
                context.Fail(new(this, "User doesn't have required access level!"));
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
