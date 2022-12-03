﻿using System.Linq.Expressions;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database.Postgres
{
    public class PostgresTaskData : ITaskData
    {
        private const string TableName = "tasks";
        private readonly IPostgresDataSource _dataSource;
        private readonly IDbService _dbService;
        private readonly Func<LambdaExpression, DbConstraint> _const;

        public PostgresTaskData(
            IPostgresDataSource dataSource,
            IDbService dbService)
        {
            _dataSource = dataSource;
            _dbService = dbService;
            _const = (e) => new DbConstraint(_dbService, e);
        }

        public async Task<IDatabaseResult<TodoTask>> CreateAsync(TodoTask task, int? userId)
        {
            try
            {
                task.UserId = userId ?? throw new ArgumentNullException(nameof(userId));
                var createdTask = (await _dataSource.InsertRowsReturning(TableName, new[] { task }))
                    .FirstOrDefault();
                return DatabaseResults.Ok(createdTask);
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask>(error.Message);
            }
        }

        public async Task<IDatabaseResult> DeleteAsync(int id, int? userId)
        {
            try
            {
                var constraint = _const((TodoTask t) => t.Id == id && t.UserId == userId);
                if (await _dataSource.DeleteRows(TableName, constraint) == 0)
                    return DatabaseResults.Error("Task not found!");

                return DatabaseResults.Ok();
            }
            catch (Exception error)
            {
                return DatabaseResults.Error(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask[]>> GetAllAsync(int? userId)
        {
            try
            {
                var constraint = _const((TodoTask t) => t.UserId == userId);
                var tasks = await _dataSource.ReadRows<TodoTask>(TableName, constraint);
                return DatabaseResults.Ok(tasks.ToArray());
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask[]>(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask>> GetAsync(int id, int? userId)
        {
            try
            {
                var constraint = _const((TodoTask t) => t.Id == id && t.UserId == userId);
                var tasks = await _dataSource.ReadRows<TodoTask>(TableName, constraint);
                if (!tasks.Any())
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                return DatabaseResults.Ok(tasks.First());
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask>(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask>> ToggleCompletedAsync(int id, int? userId)
        {
            try
            {
                var commandString = """
                    UPDATE tasks 
                        SET completed_at = 
                        CASE
                            WHEN completed_at is NULL THEN CURRENT_TIMESTAMP
                            ELSE NULL
                        END
                    WHERE id = 1
                    RETURNING *
                    """;

                var tasks = await _dataSource.ExecuteQuery<TodoTask>(commandString);
                if (!tasks.Any())
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                return DatabaseResults.Ok(tasks.First());
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask>(error.Message);
            }
        }

        public async Task<IDatabaseResult<TodoTask>> UpdateAsync(int id, TodoTask task, int? userId)
        {
            try
            {
                if (task.Id != id)
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                if (userId is null)
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                task.UserId = (int)userId;

                var constraint = _const((TodoTask t) => t.Id == id && t.UserId == userId);
                var tasks = await _dataSource.UpdateRowsReturning(TableName, task, constraint);
                if (!tasks.Any())
                    return DatabaseResults.Error<TodoTask>("Task not found!");

                return DatabaseResults.Ok(tasks.First());
            }
            catch (Exception error)
            {
                return DatabaseResults.Error<TodoTask>(error.Message);
            }
        }
    }
}