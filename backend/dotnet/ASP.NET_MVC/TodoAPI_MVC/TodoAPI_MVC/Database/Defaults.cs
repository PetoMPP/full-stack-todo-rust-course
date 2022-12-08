using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database
{
    public static class Defaults
    {
        public static readonly UserDto DefaultAdmin = new(
            Environment.GetEnvironmentVariable(VariableNames.ApiAdminUser)
                ?? throw new InvalidOperationException($"{VariableNames.ApiAdminUser} is unset!"),
            Environment.GetEnvironmentVariable(VariableNames.ApiAdminPassword)
                ?? throw new InvalidOperationException($"{VariableNames.ApiAdminPassword} is unset!"));

        public static readonly TodoTask[] DefaultTasks = new[]
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
                Title = "See my details for by clicking me",
                Priority = Priority.B,
                Description = "My description can be changed"
            },
        };
    }
}
