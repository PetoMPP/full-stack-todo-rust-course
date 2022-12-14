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
        public const string LoginError = "Invalid username or password!";
        public const string NotLoggedInError = "User is not logged in!";
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
                return Unauthorized(LoginError);

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(LoginError);

            user.Token = _authService.GetToken(user);
            return Ok(user);
        }

        [HttpPost("logout")]
        [AuthorizeAccess(EndpointAccess.None)]
        public Task<IActionResult> LogoutAsync()
        {
            var identity = (ClaimsIdentity)HttpContext.User.Identity!;

            if (identity.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Jti)?.Value is not string tokenGuidRaw)
            {
                _logger.LogError("Token id not found!");
                return Task.FromResult<IActionResult>(BadRequest(NotLoggedInError));
            }

            if (!Guid.TryParse(tokenGuidRaw, out var tokenGuid))
            {
                _logger.LogError("Token id is not valid Guid string!");
                return Task.FromResult<IActionResult>(BadRequest(NotLoggedInError));
            }

            if (identity.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Exp)?.Value is not string expRaw)
            {
                _logger.LogError("Token has no expiration set!");
                return Task.FromResult<IActionResult>(BadRequest(NotLoggedInError));
            }

            if (!long.TryParse(expRaw, out var unixExp))
            {
                _logger.LogError("Token has invalid expiration format!");
                return Task.FromResult<IActionResult>(BadRequest(NotLoggedInError));
            }

            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(unixExp).DateTime;

            _revokedTokens.Add(tokenGuid, expirationDate);

            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
