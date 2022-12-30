using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public class AuthEventsHandler : JwtBearerEvents
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AuthEventsHandler(JsonSerializerOptions jsonSerializerOptions)
        {
            OnMessageReceived = MessageReceivedHandler;
            OnForbidden = ParseErrorsHeaderIntoBody;
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        private async Task ParseErrorsHeaderIntoBody(ResultContext<JwtBearerOptions> context)
        {
            var errors = context.Response.Headers["error"];
            if (!context.Response.Headers.Remove("error"))
                return;

            await context.Response.WriteAsJsonAsync(
                new { Error = string.Join(", ", errors) }, _jsonSerializerOptions);
        }

        private Task MessageReceivedHandler(MessageReceivedContext context)
        {
            if (!context.Request.Headers.TryGetValue("x-auth-token", out var headerValue))
            {
                context.NoResult();
                return Task.CompletedTask;
            }

            context.Token = headerValue;

            return Task.CompletedTask;
        }
    }
}
