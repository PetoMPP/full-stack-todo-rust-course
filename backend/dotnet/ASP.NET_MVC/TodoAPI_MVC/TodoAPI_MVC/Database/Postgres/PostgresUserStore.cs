using Microsoft.AspNetCore.Identity;
using Npgsql;
using System.Linq.Expressions;
using System.Security.Claims;
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

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                await _dataSource.DeleteRows(TableName, _const((User u) => u.Id == user.Id), cancellationToken);
                return IdentityResult.Success;
            }
            catch (PostgresException error)
            {
                return IdentityResult.Failed(new IdentityError { Description = error.MessageText });
            }
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

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                var constraint = _const((User u) => u.Id == user.Id);
                var rowsChanged = await _dataSource.UpdateRows(TableName, user, constraint, cancellationToken);
                if (rowsChanged > 0)
                    return IdentityResult.Success;

                return IdentityResult.Failed(new IdentityError { Description = "Unable to find user!" });
            }
            catch (PostgresException error)
            {
                return IdentityResult.Failed(new IdentityError { Description = error.MessageText });
            }
        }

        public Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            if (claim.Type != newClaim.Type)
                throw new ArgumentException("Incompatible Claim types!", nameof(newClaim));

            user.Access = claim.Type switch
            {
                "Access" => Enum.Parse<EndpointAccess>(newClaim.Value),
                _ => throw new InvalidOperationException("Unrecognized Claim type!"),
            };

            return Task.CompletedTask;
        }

        public Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            switch (claim.Type)
            {
                case "Access":
                    var access = Enum.Parse<EndpointAccess>(claim.Value);
                    var constraint = _const((User u) => u.Access == access);
                    return await _dataSource.ReadRows<User>(TableName, constraint, cancellationToken);

                default:
                    return new List<User>();
            }
        }
    }
}