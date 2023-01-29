using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoAPI_MVC_Tests.Authentication.Handlers
{
    internal static class TestsHelper
    {
        internal static AuthorizationHandlerContext GetAuthorizationHandlerContext(
            IAuthorizationRequirement requirement,
            IEnumerable<Claim>? userClaims = null,
            bool isAuthenticated = false,
            object? resource = null)
        {
            userClaims ??= new List<Claim>();
            var identityMock = new Mock<ClaimsIdentity>();
            identityMock
                .SetupGet(m => m.Claims)
                .Returns(userClaims);
            identityMock
                .SetupGet(m => m.IsAuthenticated)
                .Returns(isAuthenticated);
            var userMock = new Mock<ClaimsPrincipal>();
            userMock
                .SetupGet(m => m.Identity)
                .Returns(identityMock.Object);

            return new(new[] { requirement }, userMock.Object, resource);
        }
    }
}
