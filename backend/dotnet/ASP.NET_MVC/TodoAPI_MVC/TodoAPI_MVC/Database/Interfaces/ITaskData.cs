using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database.Interfaces
{
    public interface ITaskData
    {
        Task<IDatabaseResult<TodoTask>> CreateAsync(TodoTask task, int? userId, CancellationToken cancellationToken = default);
        Task<IDatabaseResult> DeleteAsync(int id, int? userId, CancellationToken cancellationToken = default);
        Task<IDatabaseResult<TodoTask>> GetAsync(int id, int? userId, CancellationToken cancellationToken = default);
        Task<IDatabaseResult<TodoTask[]>> GetAllOwnedAsync(int? userId, CancellationToken cancellationToken = default);
        Task<IDatabaseResult<TodoTask[]>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IDatabaseResult<TodoTask>> ToggleCompletedAsync(int id, int? userId, CancellationToken cancellationToken = default);
        Task<IDatabaseResult<TodoTask>> UpdateAsync(int id, TodoTask task, int? userId, CancellationToken cancellationToken = default);
        Task<IDatabaseResult<TodoTask[]>> CreateDefaultsAsync(int? userId, CancellationToken cancellationToken = default);
    }
}
