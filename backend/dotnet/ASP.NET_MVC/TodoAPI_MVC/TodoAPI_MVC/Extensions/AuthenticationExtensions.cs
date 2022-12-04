using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoAPI_MVC.Handlers;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Extensions
{
    public static class AuthenticationExtensions
    {
        public static string GetToken(this IConfiguration config, User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable(Consts.JwtSecretEnvName) ?? "password";
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", $"{user.Id}"),
                    new Claim("Access", $"{(int)user.Access}"),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Email, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = credentials
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }

        public static void AddJwtAuthentication(this WebApplicationBuilder builder)
        {
            var jwtKey = Environment.GetEnvironmentVariable(Consts.JwtSecretEnvName) ?? "password";
            builder.Services
                .AddAuthentication(options =>
                 {
                     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                 })
                .AddJwtBearer(options =>
                {
                    options.Events = AuthEventsHandler.Instance;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey
                            (Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });

            builder.Services.AddAuthorization();
        }
    }
}
