using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Extensions;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TasksController : ApiControllerBase
    {
        private readonly ITaskData _taskData;

        public TasksController(
            ITaskData taskData,
            IConfiguration config,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
            : base(config, userManager, signInManager)
        {
            _taskData = taskData;
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoTask newTask)
        {
            return (await _taskData.CreateAsync(newTask, await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            return (await _taskData.GetAsync(id, await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return (await _taskData.GetAllAsync(await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Update(int id, TodoTask updatedTask)
        {
            return (await _taskData.UpdateAsync(id, updatedTask, await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpPatch("{id:int}/toggle-completed")]
        public async Task<IActionResult> ToggleCompleted(int id)
        {
            return (await _taskData.ToggleCompletedAsync(id, await GetCurrentUserId())).ToIActionResult(this);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return (await _taskData.DeleteAsync(id, await GetCurrentUserId())).ToIActionResult(this);
        }
    }
}
