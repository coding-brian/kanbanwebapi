using KanbanWebApi.Dto.Task;
using KanbanWebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace KanbanWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController(TaskService taskService) : ControllerBase
    {
        private readonly TaskService _taskService = taskService;

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            return Ok(await _taskService.GetAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateTaskDto dto)
        {
            return Ok(await _taskService.CreateAsync(dto));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            return Ok(await _taskService.DeleteAsync(id));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateTaskDto dto)
        {
            return Ok(await _taskService.UpdateAsync(dto));
        }
    }
}