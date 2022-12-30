using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected readonly UserManager<User> _userManager;
        protected readonly SignInManager<User> _signInManager;

        protected ApiControllerBase(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        protected async Task<int?> GetCurrentUserId()
        {
            return (await GetCurrentUser())?.Id;
        }

        protected async Task<User?> GetCurrentUser()
        {
            var identity = (ClaimsIdentity)HttpContext.User.Identity!;

            if (identity.Claims.FirstOrDefault(c => c.Type == "Id")?.Value is not string userId)
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        protected IActionResult ActionResult<T>(IDatabaseResult<T> dbResult)
        {
            return dbResult.Code switch
            {
                Models.StatusCode.Ok => base.Ok(dbResult.Data),
                Models.StatusCode.Error => base.BadRequest(GetErrorString(dbResult.ErrorData)),
                _ => base.StatusCode(500, GetErrorString(dbResult.ErrorData))
            };
        }

        protected IActionResult ActionResult(IDatabaseResult dbResult)
        {
            return dbResult.Code switch
            {
                Models.StatusCode.Ok => Ok(),
                Models.StatusCode.Error => BadRequest(GetErrorString(dbResult.ErrorData)),
                _ => StatusCode(500, GetErrorString(dbResult.ErrorData))
            };
        }

        private static string GetErrorString(string[]? errorData)
        {
            return string.Join(Environment.NewLine, errorData ?? Array.Empty<string>());
        }
    }
}
