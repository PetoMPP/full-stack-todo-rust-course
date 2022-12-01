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
        Task<IDatabaseResult<TodoTask?>> UpdateAsync(int id, TodoTask task, int? userId);

        static async Task<IDatabaseResult<TodoTask[]>> CreateDefaultsAsync(ITaskData taskData, int? userId)
        {
            var tasks = new List<TodoTask>();
            foreach (var task in Defaults.DefaultTasks)
            {
                var result = await taskData.CreateAsync(task, userId);
                if (!result.IsOk)
                    return DatabaseResults.Error<TodoTask[]>(result.ErrorData);

                tasks.Add((TodoTask)result.Data!);
            }

            return DatabaseResults.Ok(tasks.ToArray());
        }
    }
}
