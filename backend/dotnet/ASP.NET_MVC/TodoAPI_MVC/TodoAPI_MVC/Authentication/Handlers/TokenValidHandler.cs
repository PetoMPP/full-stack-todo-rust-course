using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public class TokenValidHandler : VerboseAuthorizationHandler<TokenValidRequirement>
    {
        private readonly IRevokedTokens _revokedTokens;

        public TokenValidHandler(
            IRevokedTokens revokedTokens,
            ILogger<TokenValidHandler> logger) : base(logger)
        {
            _revokedTokens = revokedTokens;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, TokenValidRequirement requirement)
        {
            if (context.User.Identity is not ClaimsIdentity identity)
            {
                FailAndCryAboutIt(context, "User doesn't have identity schema!");
                return Task.CompletedTask;
            }
         
            if (!Guid.TryParse(
                identity.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value,
                out var tokenId))
            {
                FailAndCryAboutIt(context, "Invalid identity Id!");
            }

            if (!Guid.TryParse(
                identity.Claims.FirstOrDefault(c => c.Type == "SessionId")?.Value,
                out var sessionId))
            {
                FailAndCryAboutIt(context, "Invalid identity session Id!");
            }

            if (context.HasFailed)
                return Task.CompletedTask;

            if (sessionId != IAuthenticationService.SessionId)
            {
                FailAndCryAboutIt(context, "Invalid identity session Id!");
                return Task.CompletedTask;
            }

            if (_revokedTokens.Contains(tokenId))
            {
                FailAndCryAboutIt(context, "Invalid identity token!");
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
