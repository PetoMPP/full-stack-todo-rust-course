using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database
{
    public interface IDatabase
    {
        public ITaskData TaskData { get; }
        public IDatabaseUserStore<User> UserStore { get; }
    }

    public interface ITaskData
    {
        Task<IDatabaseResult<TodoTask?>> CreateAsync(TodoTask task, int? userId);
        Task<IDatabaseResult> DeleteAsync(int id, int? userId);
        Task<IDatabaseResult<TodoTask?>> GetAsync(int id, int? userId);
        Task<IDatabaseResult<TodoTask[]?>> GetAllAsync(int? userId);
        Task<IDatabaseResult<TodoTask?>> ToggleCompletedAsync(int id, int? userId);
        Task<IDatabaseResult<TodoTask?>> UpdateAsync(int? taskId, TodoTask task, int? userId);
        Task<IDatabaseResult<TodoTask[]>> CreateDefaultsAsync(int? userId);
    }

    public interface IUserData
    {
        public Task<IDatabaseResult<User?>> RegisterAsync(WebApplication app, UserDto user);
        public Task<IDatabaseResult<User?>> LoginAsync(WebApplication app, UserDto user);
    }
}
