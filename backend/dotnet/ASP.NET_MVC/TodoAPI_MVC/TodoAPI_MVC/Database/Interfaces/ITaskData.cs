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

        static async Task<IDatabaseResult<TodoTask[]>> CreateDefaultsAsync(ITaskData taskData, int? userId, CancellationToken cancellationToken = default)
        {
            var tasks = new List<TodoTask>();
            foreach (var task in Defaults.DefaultTasks)
            {
                var result = await taskData.CreateAsync(task, userId, cancellationToken);
                if (!result.IsOk)
                    return DatabaseResults.Error<TodoTask[]>(result.ErrorData);

                tasks.Add(result.Data);
            }

            return DatabaseResults.Ok(tasks.ToArray());
        }
    }
}
