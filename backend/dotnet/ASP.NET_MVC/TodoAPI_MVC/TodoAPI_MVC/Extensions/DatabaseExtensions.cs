using Microsoft.AspNetCore.Identity;
using Npgsql;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Database.Memory;
using TodoAPI_MVC.Database.Postgres;
using TodoAPI_MVC.Database.Service;
using TodoAPI_MVC.Models;
using TodoAPI_MVC.Services;

namespace TodoAPI_MVC.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseContext(
            this IServiceCollection services,
            IVariables variables)
        {
            if (!Enum.TryParse<DatabaseMode>(
                variables.DatabaseMode, true, out var databaseMode))
            {
                throw new InvalidOperationException($"{variables.DatabaseMode} has invalid value!");
            }

            return databaseMode switch
            {
                DatabaseMode.Memory => AddMemoryDbContext(services),
                DatabaseMode.Postgres => AddPostgresDbContext(services, variables),
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

        private static IServiceCollection AddPostgresDbContext(
            IServiceCollection services,
            IVariables variables)
        {
            services.AddSingleton<IDbService, DbService>();
            services.AddSingleton(NpgsqlDataSource.Create(GetPostgresConnectionString(variables)));
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

        private static string GetPostgresConnectionString(IVariables variables)
        {
            return 
                $"Host=localhost:5432;" +
                $"Username={variables.DatabaseUser};" +
                $"Password={variables.DatabasePassword}";
        }
    }
}
