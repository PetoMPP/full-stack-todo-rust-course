using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoAPI_MVC.Models;
using TodoAPI_MVC.Services;

namespace TodoAPI_MVC.Authentication
{
    public interface IAuthenticationService
    {
        string GetToken(User user);
        Guid SessionId { get; }
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _config;
        private readonly IVariables _variables;

        public Guid SessionId => Guid.NewGuid();

        public AuthenticationService(IConfiguration config, IVariables variables)
        {
            _config = config;
            _variables = variables;
        }

        public string GetToken(User user)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_variables.JwtSecret));
            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", $"{user.Id}"),
                    new Claim("SessionId", $"{SessionId}"),
                    new Claim("Access", $"{(int)user.Access}"),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, $"{Guid.NewGuid()}")
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = credentials
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }
    }
}
