using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoAPI_MVC.Authentication;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Controllers
{
    [Authorize(Policy = nameof(EndpointAccess.TasksOwned))]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TasksController : ApiControllerBase
    {
        private readonly ITaskData _taskData;

        public TasksController(
            ITaskData taskData,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
            : base(userManager, signInManager)
        {
            _taskData = taskData;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            TodoTask newTask, CancellationToken cancellationToken)
        {
            return ActionResult(
                await _taskData.CreateAsync(newTask, await GetCurrentUserId(), cancellationToken));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            return ActionResult(
                await _taskData.GetAsync(id, await GetCurrentUserId(), cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOwned(CancellationToken cancellationToken)
        {
            return ActionResult(
                await _taskData.GetAllOwnedAsync(await GetCurrentUserId(), cancellationToken));
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Update(
            int id, TodoTask updatedTask, CancellationToken cancellationToken)
        {
            return ActionResult(
                await _taskData.UpdateAsync(id, updatedTask, await GetCurrentUserId(), cancellationToken));
        }

        [HttpPatch("{id:int}/toggle-completed")]
        public async Task<IActionResult> ToggleCompleted(int id, CancellationToken cancellationToken)
        {
            return ActionResult(
                await _taskData.ToggleCompletedAsync(id, await GetCurrentUserId(), cancellationToken));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            return ActionResult(
                await _taskData.DeleteAsync(id, await GetCurrentUserId(), cancellationToken));
        }

        [HttpGet("all")]
        [Authorize(Policy = nameof(EndpointAccess.TasksAll))]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            return ActionResult(await _taskData.GetAllAsync(cancellationToken));
        }
    }
}
