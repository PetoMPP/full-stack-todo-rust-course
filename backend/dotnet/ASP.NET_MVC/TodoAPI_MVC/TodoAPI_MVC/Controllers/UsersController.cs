using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoAPI_MVC.Atributtes;
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
        private readonly ILogger _logger;
        private readonly IRevokedTokens _revokedTokens;
        private readonly ITaskData _taskData;
        private readonly IAuthenticationService _authService;

        public UsersController(
            ILogger<UsersController> logger,
            IRevokedTokens revokedTokens,
            ITaskData taskData,
            IAuthenticationService authService,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
            : base(userManager, signInManager)
        {
            _logger = logger;
            _revokedTokens = revokedTokens;
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
                await _taskData.CreateDefaultsAsync(newUser.Id);
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

        [HttpPost("logout")]
        [AuthorizeAccess(EndpointAccess.None)]
        public IActionResult Logout()
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest("User is not logged in!");

            try
            {
                var tokenGuid = Guid.Parse(
                    identity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);

                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(
                    long.Parse(identity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp).Value))
                    .DateTime;

                _revokedTokens.Add(tokenGuid, expirationDate);

                return Ok();
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Error during claims parsing!");
                return BadRequest("User is not logged in!");
            }
        }
    }
}
