using Microsoft.Extensions.Configuration;
using TodoAPI_MVC.Authentication;
using TodoAPI_MVC.Models;
using TodoAPI_MVC.Services;

namespace TodoAPI_MVC_Tests.Authentication
{
    public class AuthenticationServiceTests
    {
        [Test]
        public void GetToken_ShouldReturnValidToken_WithProperConfig()
        {
            var variablesMock = new Mock<IVariables>();
            variablesMock
                .SetupGet(m => m.JwtSecret)
                .Returns("This is a test secret used for testing");
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(m => m["Jwt:Issuer"]).Returns("issuer.com");
            configMock.Setup(m => m["Jwt:Audience"]).Returns("issuer.com");
            var service = new AuthenticationService(
                configMock.Object, variablesMock.Object);

            var expected = service.GetToken(new User());

            expected.Should().NotBeNullOrEmpty();
        }
    }
}
