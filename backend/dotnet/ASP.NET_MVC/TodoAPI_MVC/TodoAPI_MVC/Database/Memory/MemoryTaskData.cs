using TodoAPI_MVC.Extensions;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database.Memory
{
    internal class MemoryTaskData : ITaskData
    {
        private readonly List<TodoTask> _tasks = new();
        private readonly Dictionary<int, int> _taskOwners = new();

        public Task<IDatabaseResult<TodoTask?>> CreateAsync(TodoTask task, int? userId)
        {
            var validationError = task.Validate();
            if (validationError is string error)
                return Task.FromResult(DatabaseResults.Error<TodoTask?>(error));

            if (userId is not int id)
                return Task.FromResult(DatabaseResults.Error<TodoTask?>("Invalid user!"));

            task.Id = _tasks.GetNextValue(t => t.Id);
            _tasks.Add(task);
            _taskOwners[task.Id] = id;

            return Task.FromResult(DatabaseResults.Ok<TodoTask?>(task));
        }

        public Task<IDatabaseResult<TodoTask?>> UpdateAsync(int? taskId, TodoTask task, int? userId)
        {
            var validationError = task.Validate();
            if (validationError is string error)
                return Task.FromResult(DatabaseResults.Error<TodoTask?>(error));

            var taskIndex = _tasks.FindIndex(t => t.Id == taskId);

            if (taskIndex < 0)
                return Task.FromResult(DatabaseResults.Error<TodoTask?>("Task not found!"));

            var taskToUpdate = _tasks[taskIndex];
            var ownershipError = EnsureOwnership(taskToUpdate, userId);

            if (ownershipError is string oError)
                return Task.FromResult(DatabaseResults.Error<TodoTask?>(oError));

            _tasks[taskIndex] = task;

            return Task.FromResult(DatabaseResults.Ok<TodoTask?>(taskToUpdate));
        }

        public Task<IDatabaseResult> DeleteAsync(int id, int? userId)
        {
            if (!_tasks.Any(t => t.Id == id))
                return Task.FromResult(DatabaseResults.Error("Task not found!"));

            var task = _tasks.First(t => t.Id == id);
            var ownershipError = EnsureOwnership(task, userId);

            if (ownershipError is string oError)
                return Task.FromResult(DatabaseResults.Error(oError));

            _tasks.Remove(task);
            return Task.FromResult(DatabaseResults.Ok());
        }

        public Task<IDatabaseResult<TodoTask[]?>> GetAllAsync(int? userId)
        {
            if (userId is not int id)
                return Task.FromResult(DatabaseResults.Error<TodoTask[]?>("Invalid user!"));

            var ownedTasksIds = _taskOwners.Where(kv => kv.Value == userId).Select(kv => kv.Key);
            return Task.FromResult(DatabaseResults.Ok<TodoTask[]?>(
                _tasks.Where(t => ownedTasksIds.Any(i => i == t.Id)).ToArray()));
        }

        public Task<IDatabaseResult<TodoTask?>> ToggleCompletedAsync(int id, int? userId)
        {
            var taskIndex = _tasks.FindIndex(0, t => t.Id == id);
            if (taskIndex < 0)
                return Task.FromResult(DatabaseResults.Error<TodoTask?>("Task not found!"));

            var task = _tasks[taskIndex];
            var ownershipError = EnsureOwnership(task, userId);

            if (ownershipError is string oError)
                return Task.FromResult(DatabaseResults.Error<TodoTask?>(oError));

            task.CompletedAt = task.CompletedAt is null
                ? DateTime.Now
                : null;

            _tasks[taskIndex] = task;

            return Task.FromResult(DatabaseResults.Ok<TodoTask?>(task));
        }

        public Task<IDatabaseResult<TodoTask?>> GetAsync(int id, int? userId)
        {
            var taskIndex = _tasks.FindIndex(t => t.Id == id);
            if (taskIndex < 0)
                return Task.FromResult(DatabaseResults.Error<TodoTask?>("Task not found!"));

            return Task.FromResult(DatabaseResults.Ok<TodoTask?>(_tasks[taskIndex]));
        }

        public async Task<IDatabaseResult<TodoTask[]>> CreateDefaultsAsync(int? userId)
        {
            var tasks = new List<TodoTask>();
            foreach (var task in Defaults.DefaultTasks)
            {
                var result = await CreateAsync(task, userId);
                if (!result.IsOk)
                    return DatabaseResults.Error<TodoTask[]>(result.ErrorData);

                tasks.Add((TodoTask)result.Data!);
            }

            return DatabaseResults.Ok(tasks.ToArray());
        }

        private string? EnsureOwnership(TodoTask taskToUpdate, int? userId)
        {
            if (userId is not int id)
                return "Invalid user!";

            if (!_taskOwners.TryGetValue(taskToUpdate.Id, out var taskOwnerId))
                return "Task not found!";

            if (taskOwnerId != id)
                return "Task is not owned by the user!";

            return null;
        }
    }
}
