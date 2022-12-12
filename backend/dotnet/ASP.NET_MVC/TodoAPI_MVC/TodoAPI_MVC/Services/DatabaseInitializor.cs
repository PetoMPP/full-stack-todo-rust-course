using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TodoAPI_MVC.Authentication;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Services
{
    public class DatabaseInitializor : IHostedService
    {
        private readonly IDefaults _defaults;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1));

        public DatabaseInitializor(
            IDefaults defaults,
            UserManager<User> userManager,
            ILogger<DatabaseInitializor> logger,
            IHostApplicationLifetime applicationLifetime)
        {
            _defaults = defaults;
            _userManager = userManager;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();
            return Task.CompletedTask;
        }

        private async void OnStarted()
        {
            try
            {
                do
                {
                    await EnsureAdminAccount();
                }
                while (await _timer.WaitForNextTickAsync());
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Unable to create admin account!");
            }
        }

        private async Task EnsureAdminAccount()
        {
            var adminRights = Enum.GetValues<EndpointAccess>()
                .Aggregate((curr, next) => curr |= next);

            var adminClaim = new Claim("Access", $"{(int)adminRights}");
            var admins = await _userManager.GetUsersForClaimAsync(adminClaim);

            if (admins.Any())
                return;

            var newAdmin = _defaults.DefaultAdmin;
            var user = new User { Username = newAdmin.Username };

            if (!await CreateDefaultAdmin(user, newAdmin.Password))
                throw new InvalidOperationException("Unable to create admin account!");

            user = await _userManager.FindByNameAsync(user.Username);

            var claimsResult = await _userManager.ReplaceClaimAsync(user, new Claim("Access", "1"), adminClaim);

            if (!claimsResult.Succeeded)
                throw new InvalidOperationException("Unable to create admin account!");
        }

        private async Task<bool> CreateDefaultAdmin(User user, string password)
        {
            var creationResult = await _userManager.CreateAsync(user, password);
            if (creationResult.Succeeded)
                return true;

            var existingUser = await _userManager.FindByNameAsync(user.Username);

            var deletionResult = await _userManager.DeleteAsync(existingUser);
            if (!deletionResult.Succeeded)
                return false;

            return (await _userManager.CreateAsync(user, password)).Succeeded;
        }
    }
}
