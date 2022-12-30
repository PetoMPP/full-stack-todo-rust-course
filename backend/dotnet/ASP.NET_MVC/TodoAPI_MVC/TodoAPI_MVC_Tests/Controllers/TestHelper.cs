using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC_Tests.Controllers
{
    internal class TestHelper
    {
        internal const string Error = "error";

        internal static ControllerContext GetControllerContext(IEnumerable<Claim>? userClaims = null)
        {
            userClaims ??= new List<Claim>();
            var identityMock = new Mock<ClaimsIdentity>();
            identityMock
                .SetupGet(m => m.Claims)
                .Returns(userClaims);
            var userMock = new Mock<ClaimsPrincipal>();
            userMock
                .SetupGet(m => m.Identity)
                .Returns(identityMock.Object);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock
                .SetupGet(m => m.User)
                .Returns(userMock.Object);

            return new ControllerContext
            {
                HttpContext = httpContextMock.Object,
            };
        }

        internal static Mock<SignInManager<User>> GetSignInManagerMock(
            UserManager<User> userManager, bool shouldAuthFail = false)
        {
            var mock = new Mock<SignInManager<User>>(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null);

            mock
                .Setup(m => m.CheckPasswordSignInAsync(
                    It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(shouldAuthFail
                    ? Microsoft.AspNetCore.Identity.SignInResult.Failed
                    : Microsoft.AspNetCore.Identity.SignInResult.Success);

            return mock;
        }

        internal static Mock<UserManager<User>> GetUserManagerMock(
            bool shouldFail = false,
            string? error = null)
        {
            var mock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                null, null, null, null, null, null, null, null);

            mock
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))!
                .ReturnsAsync((string id) => 
                    shouldFail ? null : new User { Id = int.Parse(id) });

            mock
                .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))!
                .Callback((User user, string password) =>
                    user.NormalizedUsername = user.Username.ToUpperInvariant())
                .ReturnsAsync((User user, string _) => shouldFail
                    ? IdentityResult.Failed(error is string errorString
                        ? new[] { new IdentityError { Description = errorString } }
                        : Array.Empty<IdentityError>())
                    : IdentityResult.Success);

            mock
                .Setup(m => m.FindByNameAsync(It.IsAny<string>()))!
                .ReturnsAsync((string name) => shouldFail
                    ? null
                    : new User { Id = 1, Username = name.ToLowerInvariant(), NormalizedUsername = name });

            return mock;
        }

        internal static IDatabaseResult<T> GetDbResult<T>(
            StatusCode statusCode, bool isErrorNull, T data)
        {
            var error = isErrorNull ? null : new[] { Error };
            return statusCode switch
            {
                StatusCode.Ok => DatabaseResults.Ok(data),
                StatusCode.Error => DatabaseResults.Error<T>(error),
                _ => new DatabaseResult<T>(statusCode, default, error),
            };
        }

        internal static IDatabaseResult GetDbResult(
            StatusCode statusCode, bool isErrorNull)
        {
            var error = isErrorNull ? null : new[] { Error };
            return statusCode switch
            {
                StatusCode.Ok => DatabaseResults.Ok(),
                StatusCode.Error => DatabaseResults.Error(error),
                _ => new DatabaseResult(statusCode, error),
            };
        }
    }
}
