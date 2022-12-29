using TodoAPI_MVC.Models;
using TodoAPI_MVC.Services;

namespace TodoAPI_MVC.Database
{
    public interface IDefaults
    {
        UserDto DefaultAdmin { get; }
        IReadOnlyCollection<TodoTask> DefaultTasks { get; }
    }

    public class Defaults : IDefaults
    {
        private readonly IVariables _variables;

        public UserDto DefaultAdmin => new(
            _variables.ApiAdminUser,
            _variables.ApiAdminPassword);

        public IReadOnlyCollection<TodoTask> DefaultTasks => new[]
        {
            new TodoTask
            {
                Id = 0,
                Title = "I am a task, you can complete me by checking the box",
                Priority = Priority.A,
                Description = "This is my description"
            },
            new TodoTask
            {
                Id = 0,
                Title = "See my details by clicking me",
                Priority = Priority.B,
                Description = "My description can be changed"
            },
        };

        public Defaults(IVariables variables)
        {
            _variables = variables;
        }
    }
}
