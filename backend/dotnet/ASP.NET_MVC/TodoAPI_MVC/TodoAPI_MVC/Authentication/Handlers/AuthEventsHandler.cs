using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public class AuthEventsHandler : JwtBearerEvents
    {
        public static AuthEventsHandler Instance { get; } = new AuthEventsHandler();

        private AuthEventsHandler()
        {
            OnMessageReceived = MessageReceivedHandler;
            OnForbidden = ParseErrorsHeaderIntoBody;
        }

        private static async Task ParseErrorsHeaderIntoBody(ResultContext<JwtBearerOptions> context)
        {
            var errors = context.Response.Headers["error"];
            if (context.Response.Headers.Remove("error"))
                await context.Response.WriteAsJsonAsync(new { error = string.Join(", ", errors) });
        }

        private Task MessageReceivedHandler(MessageReceivedContext context)
        {
            if (!context.Request.Headers.TryGetValue("x-auth-token", out var headerValue))
                context.NoResult();

            context.Token = headerValue;

            return Task.CompletedTask;
        }
    }
}
