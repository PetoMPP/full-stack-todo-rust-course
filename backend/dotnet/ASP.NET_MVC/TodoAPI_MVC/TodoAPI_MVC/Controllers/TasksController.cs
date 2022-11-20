using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoAPI_MVC.Database;
using TodoAPI_MVC.Extensions;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TasksController : ApiControllerBase
    {
        public TasksController(
            IConfiguration config,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IDatabase database)
            : base(config, userManager, signInManager, database)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoTask newTask)
        {
            return (await _database.TaskData.CreateAsync(newTask, await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            return (await _database.TaskData.GetAsync(id, await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return (await _database.TaskData.GetAllAsync(await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Update(int id, TodoTask updatedTask)
        {
            return (await _database.TaskData.UpdateAsync(id, updatedTask, await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpPatch("{id:int}/toggle-completed")]
        public async Task<IActionResult> ToggleCompleted(int id)
        {
            return (await _database.TaskData.ToggleCompletedAsync(id, await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return (await _database.TaskData.DeleteAsync(id, await GetCurrentUserId())).ToIActionResult(this);
        }
    }
}
