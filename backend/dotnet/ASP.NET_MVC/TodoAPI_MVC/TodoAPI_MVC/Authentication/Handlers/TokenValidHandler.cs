using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public class TokenValidHandler : VerboseAuthorizationHandler<TokenValidRequirement>
    {
        protected const string SchemaMissingMessage = "User doesn't have identity schema!";
        protected const string InvalidIdentityIdMessage = "Invalid identity Id!";
        protected const string InvalidSessionIdMessage = "Invalid identity session Id!";
        protected const string InvalidTokenMessage = "Invalid identity token!";

        private readonly IAuthenticationService _authenticationService;
        private readonly IRevokedTokens _revokedTokens;

        public TokenValidHandler(
            IAuthenticationService authenticationService,
            IRevokedTokens revokedTokens,
            ILogger<TokenValidHandler> logger) : base(logger)
        {
            _authenticationService = authenticationService;
            _revokedTokens = revokedTokens;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, TokenValidRequirement requirement)
        {
            var identity = (ClaimsIdentity)context.User.Identity!;

            if (!Guid.TryParse(
                identity.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value,
                out var tokenId))
            {
                FailAndCryAboutIt(context, InvalidIdentityIdMessage);
            }

            if (!Guid.TryParse(
                identity.Claims.FirstOrDefault(c => c.Type == "SessionId")?.Value,
                out var sessionId))
            {
                FailAndCryAboutIt(context, InvalidSessionIdMessage);
            }

            if (context.HasFailed)
                return Task.CompletedTask;

            if (sessionId != _authenticationService.SessionId)
            {
                FailAndCryAboutIt(context, InvalidSessionIdMessage);
                return Task.CompletedTask;
            }

            if (_revokedTokens.Contains(tokenId))
            {
                FailAndCryAboutIt(context, InvalidTokenMessage);
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
