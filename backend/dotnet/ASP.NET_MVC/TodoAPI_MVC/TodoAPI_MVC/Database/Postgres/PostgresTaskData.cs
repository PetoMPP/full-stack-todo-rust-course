using System.Linq.Expressions;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database.Postgres
{
    public class PostgresTaskData : ITaskData
    {
        private const string TableName = "tasks";
        private readonly IPostgresDataSource _dataSource;
        private readonly IDbService _dbService;
        private readonly IDefaults _defaults;
        private readonly Func<LambdaExpression, DbConstraint> _const;

        public PostgresTaskData(
            IPostgresDataSource dataSource,
            IDbService dbService,
            IDefaults defaults)
        {
            _dataSource = dataSource;
            _dbService = dbService;
            _defaults = defaults;
            _const = (e) => new DbConstraint(_dbService, e);
        }

        public async Task<IDatabaseResult<TodoTask>> CreateAsync(
            TodoTask task, int? userId, bool asCompleted, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationError = task.Validate();
                if (validationError is string error)
                    return DatabaseResults.Error<TodoTask>(error);

                task.UserId = userId ?? throw new ArgumentNullException(nameof(userId));
                task.CompletedAt = asCompleted ? DateTime.Now : null;
                var createdTask = (await _dataSource.InsertRowsReturning(
                    TableName, new[] { task }, cancellationToken)).FirstOrDefault();

                return DatabaseResults.Ok(createdTask);
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask>(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask[]>> CreateDefaultsAsync(
            int? userId, CancellationToken cancellationToken = default)
        {
            var tasks = new List<TodoTask>();
            foreach (var task in _defaults.DefaultTasks)
            {
                var result = await CreateAsync(task, userId, false, cancellationToken);
                if (result.Code != StatusCode.Ok)
                    return DatabaseResults.Error<TodoTask[]>(result.ErrorData);

                tasks.Add(result.Data);
            }

            return DatabaseResults.Ok(tasks.ToArray());
        }

        public async Task<IDatabaseResult> DeleteAsync(
            int id, int? userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var constraint = _const((TodoTask t) => t.Id == id && t.UserId == userId);
                if (await _dataSource.DeleteRows(TableName, constraint, cancellationToken) == 0)
                    return DatabaseResults.Error("Task not found!");

                return DatabaseResults.Ok();
            }
            catch (Exception error)
            {
                return DatabaseResults.Error(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask[]>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var tasks = await _dataSource.ReadRows<TodoTask>(
                    TableName, cancellationToken: cancellationToken);

                return DatabaseResults.Ok(tasks.ToArray());
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask[]>(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask[]>> GetAllOwnedAsync(
            int? userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var constraint = _const((TodoTask t) => t.UserId == userId);
                var tasks = await _dataSource.ReadRows<TodoTask>(
                    TableName, constraint, cancellationToken);

                return DatabaseResults.Ok(tasks.ToArray());
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask[]>(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask>> GetAsync(
            int id, int? userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var constraint = _const((TodoTask t) => t.Id == id && t.UserId == userId);
                var tasks = await _dataSource.ReadRows<TodoTask>(
                    TableName, constraint, cancellationToken);

                if (!tasks.Any())
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                return DatabaseResults.Ok(tasks[0]);
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask>(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask>> ToggleCompletedAsync(
            int id, int? userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var commandString = $"""
                    UPDATE tasks 
                        SET completed_at = 
                        CASE
                            WHEN completed_at is NULL THEN CURRENT_TIMESTAMP
                            ELSE NULL
                        END
                    WHERE {_const((TodoTask t) => t.Id == id && t.UserId == userId).ToSqlString()}
                    RETURNING *
                    """;

                var tasks = await _dataSource.ExecuteQuery<TodoTask>(
                    commandString, cancellationToken);

                if (!tasks.Any())
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                return DatabaseResults.Ok(tasks[0]);
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask>(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask>> UpdateAsync(
            int id, TodoTask task, int? userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (task.Id != id)
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                if (userId is not int validUserId)
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                var validationError = task.Validate();
                if (validationError is string error)
                    return DatabaseResults.Error<TodoTask>(error);

                task.UserId = validUserId;

                var constraint = _const((TodoTask t) => t.Id == id && t.UserId == validUserId);

                var tasks = await _dataSource.UpdateRowsReturning(
                    TableName, task, constraint, cancellationToken);

                if (!tasks.Any())
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                return DatabaseResults.Ok(tasks[0]);
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask>(error.Message);
            }
        }
    }
}