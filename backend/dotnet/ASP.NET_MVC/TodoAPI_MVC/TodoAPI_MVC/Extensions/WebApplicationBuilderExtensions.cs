using Microsoft.AspNetCore.Identity;
using Npgsql;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Database.Memory;
using TodoAPI_MVC.Database.Postgres;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services)
        {
            if (Environment.GetEnvironmentVariable(Consts.DatabaseModeEnvName) is not string databaseModeString ||
                !Enum.TryParse<DatabaseMode>(databaseModeString, true, out var databaseMode))
                throw new InvalidOperationException($"{Consts.DatabaseModeEnvName} is unset!");

            switch (databaseMode)
            {
                case DatabaseMode.Memory:

                    services.AddSingleton<ITaskData, MemoryTaskData>();
                    services.AddSingleton<IUserStore<User>, MemoryUserStore>();
                    return services;

                case DatabaseMode.Postgres:

                    services.AddSingleton<IPostgresDataSource>(
                        new PostgresDataSource(NpgsqlDataSource.Create(GetPostgresConnectionString())));

                    services.AddSingleton<ITaskData, PostgresTaskData>();
                    services.AddSingleton<IDatabaseUserStore<User>, PostgresUserStore>();
                    services.AddSingleton<IUserStore<User>, PostgresUserStore>();
                    return services;

                default:

                    throw new InvalidOperationException($"{databaseMode} is not valid database mode!");
            }
        }

        private static string GetPostgresConnectionString()
        {
            if (Environment.GetEnvironmentVariable(Consts.DatabaseUserEnvName) is not string user)
                throw new InvalidOperationException($"{Consts.DatabaseUserEnvName} is unset!");

            if (Environment.GetEnvironmentVariable(Consts.DatabasePasswordEnvName) is not string password)
                throw new InvalidOperationException($"{Consts.DatabasePasswordEnvName} is unset!");

            return $"Host=localhost:5432;Username={user};Password={password}";
        }
    }
}
