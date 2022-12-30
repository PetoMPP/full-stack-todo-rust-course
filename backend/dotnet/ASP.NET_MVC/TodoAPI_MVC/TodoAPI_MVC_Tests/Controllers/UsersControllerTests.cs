using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TodoAPI_MVC.Authentication;
using TodoAPI_MVC.Controllers;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC_Tests.Controllers
{
    public class UsersControllerTests
    {
        private const string ValidToken = "validtoken";

        [Test]
        public async Task RegisterAsync_ShouldReturnUserWithToken_OnSuccess()
        {
            var testDto = new UserDto("user", "pw");
            var controller = GetController();

            var actual = (ObjectResult)await controller.RegisterAsync(testDto);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeAssignableTo(typeof(User));
            var actualUser = (User)actual.Value!;
            actualUser.Username.Should().Be(testDto.Username);
            actualUser.Token.Should().Be(ValidToken);
        }

        [Test]
        public async Task RegisterAsync_ShouldReturnErrorString_OnFailure()
        {
            var error = "it is not allowed";
            var userManagerMock = TestHelper.GetUserManagerMock(true, error);
            var testDto = new UserDto("user", "pw");
            var controller = GetController(userManagerMock: userManagerMock);

            var actual = (ObjectResult)await controller.RegisterAsync(testDto);

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeAssignableTo(typeof(string));
            var actualError = (string)actual.Value!;
            actualError.Should().Be(error);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnUserWithToken_OnSuccess()
        {
            var testDto = new UserDto("user", "pw");
            var controller = GetController();

            var actual = (ObjectResult)await controller.LoginAsync(testDto);

            actual.StatusCode.Should().BeInRange(200, 299);
            actual.Value.Should().BeAssignableTo(typeof(User));
            var actualUser = (User)actual.Value!;
            actualUser.Username.Should().Be(testDto.Username);
            actualUser.Token.Should().Be(ValidToken);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnErrorString_OnInvalidUsernameFailure()
        {
            var userManagerMock = TestHelper.GetUserManagerMock(true);
            var testDto = new UserDto("user", "pw");
            var controller = GetController(userManagerMock: userManagerMock);

            var actual = (ObjectResult)await controller.LoginAsync(testDto);

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeAssignableTo(typeof(string));
            var actualError = (string)actual.Value!;
            actualError.Should().Be(UsersController.LoginError);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnErrorString_OnInvalidPasswordFailure()
        {
            var userManagerMock = TestHelper.GetUserManagerMock();
            var signInManagerMock = TestHelper.GetSignInManagerMock(userManagerMock.Object, true);
            var testDto = new UserDto("user", "pw");
            var controller = GetController(
                userManagerMock: userManagerMock,
                signInManagerMock: signInManagerMock);

            var actual = (ObjectResult)await controller.LoginAsync(testDto);

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeAssignableTo(typeof(string));
            var actualError = (string)actual.Value!;
            actualError.Should().Be(UsersController.LoginError);
        }

        [Test]
        public async Task LogoutAsync_ShouldReturnOkStatusCode_OnSuccess()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, $"{Guid.NewGuid()}"),
                new Claim(JwtRegisteredClaimNames.Exp, $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}")
            };
            var controller = GetController(claims: claims);

            var actual = (StatusCodeResult)await controller.LogoutAsync();

            actual.StatusCode.Should().BeInRange(200, 299);
        }

        [Test]
        public async Task LogoutAsync_ShouldReturnError_OnMissingJti()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Exp, $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}")
            };
            var controller = GetController(claims: claims);

            var actual = (ObjectResult)await controller.LogoutAsync();

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeAssignableTo(typeof(string));
            var actualError = (string)actual.Value!;
            actualError.Should().Be(UsersController.NotLoggedInError);
        }

        [Test]
        public async Task LogoutAsync_ShouldReturnError_OnUnparsableJti()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, "Guid.NewGuid()"),
                new Claim(JwtRegisteredClaimNames.Exp, $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}")
            };
            var controller = GetController(claims: claims);

            var actual = (ObjectResult)await controller.LogoutAsync();

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeAssignableTo(typeof(string));
            var actualError = (string)actual.Value!;
            actualError.Should().Be(UsersController.NotLoggedInError);
        }

        [Test]
        public async Task LogoutAsync_ShouldReturnError_OnMissingExp()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, $"{Guid.NewGuid()}")
            };
            var controller = GetController(claims: claims);

            var actual = (ObjectResult)await controller.LogoutAsync();

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeAssignableTo(typeof(string));
            var actualError = (string)actual.Value!;
            actualError.Should().Be(UsersController.NotLoggedInError);
        }

        [Test]
        public async Task LogoutAsync_ShouldReturnError_OnUnparsableExp()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, $"{Guid.NewGuid()}"),
                new Claim(JwtRegisteredClaimNames.Exp, "DateTimeOffset.UtcNow.ToUnixTimeSeconds()")
            };
            var controller = GetController(claims: claims);

            var actual = (ObjectResult)await controller.LogoutAsync();

            actual.StatusCode.Should().BeInRange(400, 499);
            actual.Value.Should().BeAssignableTo(typeof(string));
            var actualError = (string)actual.Value!;
            actualError.Should().Be(UsersController.NotLoggedInError);
        }

        private static UsersController GetController(
            Mock<ITaskData>? taskDataMock = null,
            Mock<UserManager<User>>? userManagerMock = null,
            Mock<SignInManager<User>>? signInManagerMock = null,
            Mock<IAuthenticationService>? authServiceMock = null,
            IEnumerable<Claim>? claims = null)
        {
            taskDataMock ??= GetTaskDataMock();
            userManagerMock ??= TestHelper.GetUserManagerMock();
            signInManagerMock ??= TestHelper.GetSignInManagerMock(userManagerMock.Object);
            authServiceMock ??= GetAuthenticationServiceMock();

            return new UsersController(
                Mock.Of<ILogger<UsersController>>(),
                Mock.Of<IRevokedTokens>(),
                taskDataMock.Object,
                authServiceMock.Object,
                userManagerMock.Object,
                signInManagerMock.Object)
            {
                ControllerContext = TestHelper.GetControllerContext(claims)
            };
        }

        private static Mock<IAuthenticationService> GetAuthenticationServiceMock()
        {
            var mock = new Mock<IAuthenticationService>();
            mock
                .Setup(m => m.GetToken(It.IsAny<User>()))
                .Returns(ValidToken);

            return mock;
        }

        private static Mock<ITaskData> GetTaskDataMock()
        {
            var mock = new Mock<ITaskData>();
            mock
                .Setup(m => m.CreateDefaultsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestHelper.GetDbResult(
                    StatusCode.Ok,
                    false,
                    new[]
                    { 
                        new TodoTask { Id = 1, Title = "1" },
                        new TodoTask { Id = 2, Title = "2" }
                    }
                )
            );

            return mock;
        }
    }
}
