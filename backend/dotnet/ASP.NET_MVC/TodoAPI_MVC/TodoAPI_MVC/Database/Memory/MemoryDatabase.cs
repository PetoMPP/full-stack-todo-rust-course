using Microsoft.AspNetCore.Identity;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database.Memory
{
    public class MemoryDatabase : IDatabase
    {
        public ITaskData TaskData { get; }
        public IDatabaseUserStore<User> UserStore { get; set; }

        public MemoryDatabase()
        {
            TaskData = new MemoryTaskData();
            UserStore = new MemoryUserStore();
        }
    }
}
