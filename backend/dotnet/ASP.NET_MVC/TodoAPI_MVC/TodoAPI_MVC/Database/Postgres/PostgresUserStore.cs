using Microsoft.AspNetCore.Identity;
using Npgsql;
using System.Linq.Expressions;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database.Postgres
{
    public class PostgresUserStore : IDatabaseUserStore<User>
    {
        private const string TableName = "users";
        private readonly IPostgresDataSource _dataSource;
        private readonly IDbService _dbService;
        private readonly Func<LambdaExpression, DbConstraint> _const;

        private record struct PasswordHash(string Password);

        public PostgresUserStore(
            IPostgresDataSource dataSource,
            IDbService dbService)
        {
            _dataSource = dataSource;
            _dbService = dbService;
            _const = (e) => new DbConstraint(_dbService, e);
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                await _dataSource.InsertRows(TableName, new[] {user}, cancellationToken);
                return IdentityResult.Success;
            }
            catch (PostgresException error)
            {
                return IdentityResult.Failed(new IdentityError { Description = error.MessageText });
            }
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var constraint = _const((User u) => u.Id == int.Parse(userId));
            var user = (await _dataSource.ReadRows<User>(TableName, constraint, cancellationToken))
                .FirstOrDefault();

            return user!;
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var constraint = _const((User u) => u.NormalizedUsername == normalizedUserName);
            var user = (await _dataSource.ReadRows<User>(TableName, constraint, cancellationToken))
                .FirstOrDefault();

            return user!;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            var constraint = _const((User u) => u.Id == user.Id);
            var hashes = await _dataSource.ReadRows<PasswordHash>(TableName, constraint, cancellationToken);
            return hashes.FirstOrDefault().Password;
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult($"{user.Id}");
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Username);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUsername = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.Password = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}