using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace TodoAPI_MVC.Handlers
{
    public class AuthEventsHandler : JwtBearerEvents
    {
        private AuthEventsHandler() => OnMessageReceived = MessageReceivedHandler;

        public static AuthEventsHandler Instance { get; } = new AuthEventsHandler();

        private Task MessageReceivedHandler(MessageReceivedContext context)
        {
            if (!context.Request.Headers.TryGetValue("x-auth-token", out var headerValue))
                context.NoResult();

            context.Token = headerValue;

            return Task.CompletedTask;
        }
    }
}
