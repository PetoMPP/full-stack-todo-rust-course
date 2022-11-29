using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database
{
    public interface IUserData
    {
        public Task<IDatabaseResult<User?>> RegisterAsync(WebApplication app, UserDto user);
        public Task<IDatabaseResult<User?>> LoginAsync(WebApplication app, UserDto user);
    }
}
