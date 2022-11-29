using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database
{
    public static class Defaults
    {
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
