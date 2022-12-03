using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected readonly IConfiguration _config;
        protected readonly UserManager<User> _userManager;
        protected readonly SignInManager<User> _signInManager;

        protected ApiControllerBase(
            IConfiguration config,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        protected async Task<int?> GetCurrentUserId()
        {
            return (await GetCurrentUser())?.Id;
        }

        protected async Task<User?> GetCurrentUser()
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return null;

            var userId = identity.Claims.First(c => c.Type == "Id")?.Value;

            return await _userManager.FindByIdAsync(userId);
        }
    }
}
