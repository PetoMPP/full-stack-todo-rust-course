using Microsoft.AspNetCore.Authorization;
using TodoAPI_MVC.Authentication;

namespace TodoAPI_MVC.Atributtes
{
    public class AuthorizeAccessAttribute : AuthorizeAttribute
    {
        public AuthorizeAccessAttribute(EndpointAccess access)
        {
            Policy = $"{access}";
        }
    }
}
