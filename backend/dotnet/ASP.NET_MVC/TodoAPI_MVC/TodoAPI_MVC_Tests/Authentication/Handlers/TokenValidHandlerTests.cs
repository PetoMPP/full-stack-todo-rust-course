using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using TodoAPI_MVC.Authentication;
using TodoAPI_MVC.Authentication.Handlers;

namespace TodoAPI_MVC_Tests.Authentication.Handlers
{
    public class TokenValidHandlerTests : TokenValidHandler
    {
        private static readonly TokenValidRequirement TokenValidRequirement = new();

        public TokenValidHandlerTests() : base(
            Mock.Of<IAuthenticationService>(),
            Mock.Of<IRevokedTokens>(),
            Mock.Of<ILogger<TokenValidHandler>>())
        {
        }

        [Test]
        public async Task HandleRequirementAsync_ShouldSucceed_OnValidClaims()
        {
            var id = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var authServiceMock = CreateAuthenticationServiceMock(sessionId);
            SetAuthenticationService(authServiceMock.Object);
            var revokedTokensMock = CreateRevokedTokensMock();
            SetRevokedTokens(revokedTokensMock.Object);
            var userClaims = GetValidClaims(id, sessionId);
            var context = TestsHelper.GetAuthorizationHandlerContext(
                TokenValidRequirement, userClaims, true);

            await HandleRequirementAsync(context, TokenValidRequirement);

            context.HasSucceeded.Should().BeTrue();
        }

        private static Claim[] GetValidClaims(Guid id, Guid sessionId)
        {
            return new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, $"{id}"),
                new Claim("SessionId", $"{sessionId}")
            };
        }

        [Test]
        public async Task HandleRequirementAsync_ShouldFail_OnMissingClaim()
        {
            var id = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var authServiceMock = CreateAuthenticationServiceMock(sessionId);
            SetAuthenticationService(authServiceMock.Object);
            var revokedTokensMock = CreateRevokedTokensMock();
            SetRevokedTokens(revokedTokensMock.Object);
            var context = TestsHelper.GetAuthorizationHandlerContext(
                TokenValidRequirement, null, true);

            await HandleRequirementAsync(context, TokenValidRequirement);

            context.HasSucceeded.Should().BeFalse();
            context.HasFailed.Should().BeTrue();
            context.FailureReasons.Should().Contain(r => r.Message == InvalidIdentityIdMessage);
            context.FailureReasons.Should().Contain(r => r.Message == InvalidSessionIdMessage);
        }

        [Test]
        public async Task HandleRequirementAsync_ShouldFail_OnInvalidSessionId()
        {
            var id = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var userClaims = GetValidClaims(id, sessionId);
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            authenticationServiceMock
                .SetupGet(m => m.SessionId)
                .Returns(Guid.NewGuid());
            SetAuthenticationService(authenticationServiceMock.Object);
            var context = TestsHelper.GetAuthorizationHandlerContext(
                TokenValidRequirement, userClaims, true);

            await HandleRequirementAsync(context, TokenValidRequirement);

            context.HasSucceeded.Should().BeFalse();
            context.HasFailed.Should().BeTrue();
            context.FailureReasons.Should().OnlyContain(r => r.Message == InvalidSessionIdMessage);
        }

        [Test]
        public async Task HandleRequirementAsync_ShouldFail_OnInvalidToken()
        {
            var id = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var userClaims = GetValidClaims(id, sessionId);
            var authServiceMock = CreateAuthenticationServiceMock(sessionId);
            SetAuthenticationService(authServiceMock.Object);
            var revokedTokensMock = CreateRevokedTokensMock(new[] { id });
            SetRevokedTokens(revokedTokensMock.Object);
            var context = TestsHelper.GetAuthorizationHandlerContext(
                TokenValidRequirement, userClaims, true);

            await HandleRequirementAsync(context, TokenValidRequirement);

            context.HasSucceeded.Should().BeFalse();
            context.HasFailed.Should().BeTrue();
            context.FailureReasons.Should().OnlyContain(r => r.Message == InvalidTokenMessage);
        }

        private static Mock<IAuthenticationService> CreateAuthenticationServiceMock(Guid sessionId)
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            authenticationServiceMock
                .SetupGet(m => m.SessionId)
                .Returns(sessionId);

            return authenticationServiceMock;
        }

        private static Mock<IRevokedTokens> CreateRevokedTokensMock(IEnumerable<Guid>? revokedTokens = null)
        {
            revokedTokens ??= new List<Guid>();
            var revokedTokensMock = new Mock<IRevokedTokens>();
            revokedTokensMock
                .Setup(m => m.GetEnumerator())
                .Returns(revokedTokens.GetEnumerator());

            return revokedTokensMock;
        }

        private void SetAuthenticationService(IAuthenticationService service)
        {
            var field = typeof(TokenValidHandler)
                .GetField("_authenticationService", BindingFlags.NonPublic | BindingFlags.Instance);

            field?.SetValue(this, service);
        }

        private void SetRevokedTokens(IRevokedTokens tokens)
        {
            var field = typeof(TokenValidHandler)
                .GetField("_revokedTokens", BindingFlags.NonPublic | BindingFlags.Instance);

            field?.SetValue(this, tokens);
        }
    }
}
