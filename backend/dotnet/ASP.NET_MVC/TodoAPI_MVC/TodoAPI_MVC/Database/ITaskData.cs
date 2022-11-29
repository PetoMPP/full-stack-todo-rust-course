using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database
{
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
}
