using Microsoft.AspNetCore.Identity;
using Npgsql;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Database.Memory;
using TodoAPI_MVC.Database.Postgres;
using TodoAPI_MVC.Database.Service;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services)
        {
            if (Environment.GetEnvironmentVariable(VariableNames.DatabaseMode) is not string databaseModeString ||
                !Enum.TryParse<DatabaseMode>(databaseModeString, true, out var databaseMode))
            {
                throw new InvalidOperationException($"{VariableNames.DatabaseMode} is unset!");
            }

            return databaseMode switch
            {
                DatabaseMode.Memory => AddMemoryDbContext(services),
                DatabaseMode.Postgres => AddPostgresDbContext(services),
                _ => throw new InvalidOperationException(
                    $"{databaseMode} is not valid database mode!"),
            };
        }

        public static IServiceCollection AddDbServiceOptions(
            this IServiceCollection services,
            DbServiceOptions options)
        {
            services.AddSingleton(options);
            return services;
        }

        private static IServiceCollection AddPostgresDbContext(IServiceCollection services)
        {
            services.AddSingleton<IDbService, DbService>();
            services.AddSingleton(NpgsqlDataSource.Create(GetPostgresConnectionString()));
            services.AddSingleton<IPostgresDataSource, PostgresDataSource>();

            services.AddSingleton<ITaskData, PostgresTaskData>();
            services.AddSingleton<IDatabaseUserStore<User>, PostgresUserStore>();
            services.AddSingleton<IUserStore<User>, PostgresUserStore>();
            return services;
        }

        private static IServiceCollection AddMemoryDbContext(IServiceCollection services)
        {
            services.AddSingleton<ITaskData, MemoryTaskData>();
            services.AddSingleton<IUserStore<User>, MemoryUserStore>();
            return services;
        }

        private static string GetPostgresConnectionString()
        {
            if (Environment.GetEnvironmentVariable(VariableNames.DatabaseUser) is not string user)
                throw new InvalidOperationException($"{VariableNames.DatabaseUser} is unset!");

            if (Environment.GetEnvironmentVariable(VariableNames.DatabasePassword) is not string password)
                throw new InvalidOperationException($"{VariableNames.DatabasePassword} is unset!");

            return $"Host=localhost:5432;Username={user};Password={password}";
        }
    }
}
