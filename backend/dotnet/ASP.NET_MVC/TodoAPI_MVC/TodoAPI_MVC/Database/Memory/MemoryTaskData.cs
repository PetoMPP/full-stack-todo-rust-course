using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Extensions;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database.Memory
{
    internal class MemoryTaskData : ITaskData
    {
        private readonly List<TodoTask> _tasks = new();
        private readonly Dictionary<int, int> _taskOwners = new();

        public Task<IDatabaseResult<TodoTask>> CreateAsync(
            TodoTask task, int? userId, CancellationToken _ = default)
        {
            var validationError = task.Validate();
            if (validationError is string error)
                return Task.FromResult(DatabaseResults.Error<TodoTask>(error));

            if (userId is not int id)
                return Task.FromResult(DatabaseResults.Error<TodoTask>("Invalid user!"));

            task.Id = _tasks.GetNextValue(t => t.Id);
            _tasks.Add(task);
            _taskOwners[task.Id] = id;

            return Task.FromResult(DatabaseResults.Ok(task));
        }

        public Task<IDatabaseResult<TodoTask>> UpdateAsync(
            int id, TodoTask task, int? userId, CancellationToken _ = default)
        {
            var validationError = task.Validate();
            if (validationError is string error)
                return Task.FromResult(DatabaseResults.Error<TodoTask>(error));

            var taskIndex = _tasks.FindIndex(t => t.Id == id);

            if (taskIndex < 0)
                return Task.FromResult(DatabaseResults.Error<TodoTask>("Task not found!"));

            var taskToUpdate = _tasks[taskIndex];
            var ownershipError = EnsureOwnership(taskToUpdate, userId);

            if (ownershipError is string oError)
                return Task.FromResult(DatabaseResults.Error<TodoTask>(oError));

            _tasks[taskIndex] = task;

            return Task.FromResult(DatabaseResults.Ok(taskToUpdate));
        }

        public Task<IDatabaseResult> DeleteAsync(
            int id, int? userId, CancellationToken _ = default)
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

        public Task<IDatabaseResult<TodoTask[]>> GetAllOwnedAsync(
            int? userId, CancellationToken _ = default)
        {
            if (userId is not int id)
                return Task.FromResult(DatabaseResults.Error<TodoTask[]>("Invalid user!"));

            var ownedTasksIds = _taskOwners.Where(kv => kv.Value == userId).Select(kv => kv.Key);
            return Task.FromResult(DatabaseResults.Ok(
                _tasks.Where(t => ownedTasksIds.Any(i => i == t.Id)).ToArray()));
        }

        public Task<IDatabaseResult<TodoTask>> ToggleCompletedAsync(
            int id, int? userId, CancellationToken _ = default)
        {
            var taskIndex = _tasks.FindIndex(0, t => t.Id == id);
            if (taskIndex < 0)
                return Task.FromResult(DatabaseResults.Error<TodoTask>("Task not found!"));

            var task = _tasks[taskIndex];
            var ownershipError = EnsureOwnership(task, userId);

            if (ownershipError is string oError)
                return Task.FromResult(DatabaseResults.Error<TodoTask>(oError));

            task.CompletedAt = task.CompletedAt is null
                ? DateTime.Now
                : null;

            _tasks[taskIndex] = task;

            return Task.FromResult(DatabaseResults.Ok(task));
        }

        public Task<IDatabaseResult<TodoTask>> GetAsync(
            int id, int? userId, CancellationToken _ = default)
        {
            var taskIndex = _tasks.FindIndex(t => t.Id == id);
            if (taskIndex < 0)
                return Task.FromResult(DatabaseResults.Error<TodoTask>("Task not found!"));

            var task = _tasks[taskIndex];
            var ownershipError = EnsureOwnership(task, userId);

            if (ownershipError is string oError)
                return Task.FromResult(DatabaseResults.Error<TodoTask>(oError));

            return Task.FromResult(DatabaseResults.Ok(_tasks[taskIndex]));
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

        public Task<IDatabaseResult<TodoTask[]>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDatabaseResult<TodoTask[]>> CreateDefaultsAsync(
            int? userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDatabaseResult<TodoTask>> CreateAsync(
            TodoTask task, int? userId, bool asCompleted, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
