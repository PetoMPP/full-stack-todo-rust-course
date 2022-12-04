using Microsoft.AspNetCore.Identity;
using TodoAPI_MVC.Extensions;
using TodoAPI_MVC.Json;
using TodoAPI_MVC.Middleware;
using TodoAPI_MVC.Models;
using TodoAPI_MVC.Services;

namespace TodoAPI_MVC
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ApplyArgs(args);

        #if DEBUG
            Environment.SetEnvironmentVariable(Consts.DatabaseModeEnvName, "postgres");
            Environment.SetEnvironmentVariable(Consts.DatabaseUserEnvName, "postgres");
            Environment.SetEnvironmentVariable(Consts.DatabasePasswordEnvName, "12345");
            Environment.SetEnvironmentVariable(Consts.ApiAdminUserEnvName, "admin");
            Environment.SetEnvironmentVariable(Consts.ApiAdminPasswordEnvName, "Adm1n!");
        #endif

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
            {
                p.WithMethods("*").WithHeaders("*").WithOrigins("*");
            }));

            builder.AddJwtAuthentication();
            builder.Services.AddDbServiceOptions(
                new(new Database.Service.SnakeCaseNamingPolicy()));

            builder.Services.AddDatabaseContext();
            builder.Services.AddScoped<DatabaseInitializor>();

            builder.Services.AddIdentityCore<User>()
                .AddUserManager<UserManager<User>>()
                .AddSignInManager<SignInManager<User>>();

            builder.Services.AddControllers().AddJsonOptions(o =>
                o.JsonSerializerOptions.PropertyNamingPolicy = SnakeCaseNamingPolicy.SnakeCase);

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

            var databaseInitializor = app.Services.CreateScope().ServiceProvider.GetRequiredService<DatabaseInitializor>();
            await databaseInitializor.StartAsync(CancellationToken.None);

            app.Run();
        }

        private static void ApplyArgs(string[] args)
        {
            var jwtSecretIndex = Array.FindIndex(args, 0, args.Length, a => a == Consts.JwtSecretArgName);
            if (jwtSecretIndex >= 0 && args.TryGetValueAt(jwtSecretIndex + 1, out var jwtSecret))
                Environment.SetEnvironmentVariable(Consts.JwtSecretEnvName, jwtSecret);
        }
    }
}