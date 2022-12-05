using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoAPI_MVC.Authentication;
using TodoAPI_MVC.Authentication.Handlers;

namespace TodoAPI_MVC.Extensions
{
    public static class AuthenticationExtensions
    {
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

            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddClaims();
        }

        private static IServiceCollection AddClaims(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, AccessHandler>();
            services.AddAuthorization(AddAccessPolicies);

            return services;
        }

        private static void AddAccessPolicies(AuthorizationOptions o)
        {
            foreach (var access in Enum.GetValues<EndpointAccess>()[1..])
            {

                var name = $"{access}";
                var allowedValues = $"{(int)access}";
                o.AddPolicy(name, p => p.Requirements.Add(new AccessRequirement(access)));
            }
        }
    }
}
