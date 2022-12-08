﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace TodoAPI_MVC.Authentication.Handlers
{
    public abstract class VerboseAuthorizationHandler<T> : AuthorizationHandler<T>
        where T : IAuthorizationRequirement
    {
        protected readonly ILogger _logger;

        public VerboseAuthorizationHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected void FailAndCryAboutIt(AuthorizationHandlerContext context, string message)
        {
            context.Fail(new(this, message));
            _logger.LogWarning(null, "{}", message);

            if (context.User.Identity?.IsAuthenticated == true &&
                context.Resource is HttpContext httpContext)
            {
                var errors = httpContext.Response.Headers["error"].Append(message);
                httpContext.Response.Headers["error"] = new StringValues(errors.ToArray());
            }
        }
    }
}
