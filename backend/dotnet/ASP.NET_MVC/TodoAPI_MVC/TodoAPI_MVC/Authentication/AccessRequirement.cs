using Microsoft.AspNetCore.Authorization;

namespace TodoAPI_MVC.Authentication
{
    public class AccessRequirement : IAuthorizationRequirement
    {
        public EndpointAccess RequiredAccess { get; }

        public AccessRequirement(EndpointAccess requiredAccess)
        {
            RequiredAccess = requiredAccess;
        }
    }
}