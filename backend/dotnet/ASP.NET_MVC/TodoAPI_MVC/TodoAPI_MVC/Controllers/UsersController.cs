using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoAPI_MVC.Authentication;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ApiControllerBase
    {
        private readonly ITaskData _taskData;
        private readonly IAuthenticationService _authService;

        public UsersController(
            ITaskData taskData,
            IAuthenticationService authService,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
            : base(userManager, signInManager)
        {
            _taskData = taskData;
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(UserDto dto)
        {
            var user = new User { Username = dto.Username };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                var newUser = await _userManager.FindByNameAsync(user.NormalizedUsername);
                await ITaskData.CreateDefaultsAsync(_taskData, newUser.Id);
                newUser.Token = _authService.GetToken(newUser);
                return Ok(newUser);
            }

            var error = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            return Unauthorized(error);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user is null)
                return Unauthorized("Invalid username or password!");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid username or password!");

            user.Token = _authService.GetToken(user);
            return Ok(user);
        }
    }
}
