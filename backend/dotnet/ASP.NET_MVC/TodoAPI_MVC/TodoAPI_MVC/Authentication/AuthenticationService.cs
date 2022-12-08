using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Authentication
{
    public interface IAuthenticationService
    {
        string GetToken(User user);
        static Guid SessionId { get; } = Guid.NewGuid();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _config;

        public AuthenticationService(IConfiguration config)
        {
            _config = config;
        }

        public string GetToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable(VariableNames.JwtSecret)
                ?? throw new InvalidOperationException($"{VariableNames.JwtSecret} is unset!");

            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", $"{user.Id}"),
                    new Claim("SessionId", $"{IAuthenticationService.SessionId}"),
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
