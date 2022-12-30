using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text.Json;
using System.Text.Json.Nodes;
using TodoAPI_MVC.Authentication.Handlers;

namespace TodoAPI_MVC_Tests.Authentication.Handlers
{
    public class AuthEventsHandlerTests
    {
        [Test]
        public async Task MessageRecievedHandler_ShouldAddToken_OnValidMessageRecieved()
        {
            const string token = "thisisjustan.exampletoken.ok";
            var handler = new AuthEventsHandler(new JsonSerializerOptions());
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("x-auth-token", token);
            var context = GetMessageRecievedContext(handler, httpContext);

            await handler.OnMessageReceived(context);

            context.Token.Should().Be(token);
            context.Result.Should().BeNull();
        }

        [Test]
        public async Task MessageRecievedHandler_ShouldSignalNoResult_OnInvalidMessageRecieved()
        {
            var handler = new AuthEventsHandler(new JsonSerializerOptions());
            var context = GetMessageRecievedContext(handler);

            await handler.OnMessageReceived(context);

            context.Token.Should().BeNullOrEmpty();
            context.Result.None.Should().BeTrue();
            context.Result.Succeeded.Should().BeFalse();
        }

        [Test]
        public async Task ParseErrorsHeaderIntoBody_ShouldCreateErrorBody_IfErrorHeaderIsPresent()
        {
            var handler = new AuthEventsHandler(new JsonSerializerOptions());
            var memoryStream = new MemoryStream(2048);
            var errorHeaders = new StringValues(new[] { "error numero uno", "error numero dos" });
            var responseMock = new Mock<HttpResponse>();
            responseMock
                .SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary { { "error", errorHeaders } });
            responseMock
                .SetupGet(m => m.Body)
                .Returns(memoryStream);
            var httpContextMock = new Mock<HttpContext>();
            responseMock
                .SetupGet(m => m.HttpContext)
                .Returns(httpContextMock.Object);
            httpContextMock
                .SetupGet(m => m.Response)
                .Returns(responseMock.Object);
            var context = GetForbiddenContext(handler, httpContextMock.Object);

            await handler.OnForbidden(context);
            memoryStream.Position = 0;
            var responseObject = await JsonSerializer.DeserializeAsync<JsonObject>(memoryStream);
            var actual = responseObject?["error"];

            actual?.GetValue<string>().Split(',').Should().HaveCount(errorHeaders.Count);
        }

        private static ForbiddenContext GetForbiddenContext(
            AuthEventsHandler handler, HttpContext? httpContext = null)
        {
            httpContext ??= new DefaultHttpContext();

            return new ForbiddenContext(
                httpContext,
                new AuthenticationScheme("d", "d", typeof(IAuthenticationHandler)),
                new JwtBearerOptions
                {
                    Events = handler
                });
        }

        [Test]
        public async Task ParseErrorsHeaderIntoBody_ShouldNotCreateErrorBody_IfErrorHeaderIsMissing()
        {
            var handler = new AuthEventsHandler(new JsonSerializerOptions());
            var memoryStream = new MemoryStream(2048);
            var responseMock = new Mock<HttpResponse>();
            responseMock
                .SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary());
            responseMock
                .SetupGet(m => m.Body)
                .Returns(memoryStream);
            var httpContextMock = new Mock<HttpContext>();
            responseMock
                .SetupGet(m => m.HttpContext)
                .Returns(httpContextMock.Object);
            httpContextMock
                .SetupGet(m => m.Response)
                .Returns(responseMock.Object);
            var context = GetForbiddenContext(handler, httpContextMock.Object);

            await handler.OnForbidden(context);

            memoryStream.Length.Should().Be(0);
        }

        private static MessageReceivedContext GetMessageRecievedContext(
            AuthEventsHandler handler, HttpContext? httpContext = null)
        {
            httpContext ??= new DefaultHttpContext();

            return new MessageReceivedContext(
                httpContext,
                new AuthenticationScheme("d", "d", typeof(IAuthenticationHandler)),
                new JwtBearerOptions
                {
                    Events = handler
                });
        }
    }
}
