using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Extensions;
using TodoAPI_MVC.Json;
using TodoAPI_MVC.Middleware;
using TodoAPI_MVC.Models;
using TodoAPI_MVC.Services;

namespace TodoAPI_MVC
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var variables = new Variables();
            ApplyArgs(args, variables);

#if DEBUG

            variables.DatabaseMode = "postgres";
            variables.DatabaseHost = "localhost";
            variables.DatabaseUser = "postgres";
            variables.DatabasePassword = "12345";
            variables.ApiAdminUser = "admin";
            variables.ApiAdminPassword = "Adm1n!";

#endif

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = SnakeCaseNamingPolicy.SnakeCase
            };

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
                p.WithMethods("*").WithHeaders("*").WithOrigins("*")));

            builder.AddJwtAuthentication(jsonSerializerOptions, variables);
            builder.Services.AddDbServiceOptions(
                new(new Database.Service.SnakeCaseNamingPolicy()));

            builder.Services.AddDatabaseContext(variables);
            builder.Services.AddSingleton<IVariables>(variables);
            builder.Services.AddSingleton<IDefaults, Defaults>();
            builder.Services.AddScoped<DatabaseInitializor>();

            builder.Services.AddIdentityCore<User>()
                .AddUserManager<UserManager<User>>()
                .AddSignInManager<SignInManager<User>>();

            builder.Services.AddControllers()
                .AddJsonOptions(o => OverrideJsonOptions(o, jsonSerializerOptions));

        #if DEBUG
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        #endif

            var app = builder.Build();

        #if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI();
        #endif

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseResponseWrapper();

            var databaseInitializor = app.Services
                .CreateScope().ServiceProvider
                .GetRequiredService<DatabaseInitializor>();

            await databaseInitializor.StartAsync(CancellationToken.None);

            app.Run();
        }

        private static void OverrideJsonOptions(
            JsonOptions jsonOptions, JsonSerializerOptions jsonSerializerOptions)
        {
            foreach (var propertyInfo in typeof(JsonSerializerOptions).GetProperties().Where(p => p.CanWrite))
            {
                var value = propertyInfo.GetValue(jsonSerializerOptions);
                propertyInfo.SetValue(jsonOptions.JsonSerializerOptions, value);
            }
        }

        private static void ApplyArgs(string[] args, IVariables variables)
        {
            var jwtSecretIndex = Array.FindIndex(
                args, 0, args.Length, a => a == Arguments.JwtSecret);

            if (jwtSecretIndex >= 0 && args.TryGetValueAt(jwtSecretIndex + 1, out var jwtSecret))
                variables.JwtSecret = jwtSecret;
        }
    }
}