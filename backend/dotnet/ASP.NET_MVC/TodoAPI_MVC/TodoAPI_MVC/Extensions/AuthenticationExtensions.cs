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
                        ValidateIssuerSigningKey = true,
                        LifetimeValidator = ValidateLifetime
                    };
                });

            builder.Services.AddSingleton<IRevokedTokens, RevokedTokens>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddClaims();
        }

        private static bool ValidateLifetime(
            DateTime? notBefore,
            DateTime? expires,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            if (!validationParameters.ValidateLifetime)
                return true;

            var checkTime = DateTime.UtcNow;

            return notBefore is DateTime nbf && nbf <= checkTime &&
                expires is DateTime exp && exp >= checkTime;
        }

        private static IServiceCollection AddClaims(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, AccessHandler>();
            services.AddSingleton<IAuthorizationHandler, TokenValidHandler>();
            services.AddAuthorization(AddPolicies);

            return services;
        }

        private static void AddPolicies(AuthorizationOptions o)
        {
            foreach (var access in Enum.GetValues<EndpointAccess>())
            {

                var name = $"{access}";
                var allowedValues = $"{(int)access}";
                o.AddPolicy(name, p => CreatePolicy(p, access));
            }
        }

        private static void CreatePolicy(AuthorizationPolicyBuilder builder, EndpointAccess access)
        {
            builder.Requirements.Add(new TokenValidRequirement());
            builder.Requirements.Add(new AccessRequirement(access));
        }
    }
}
