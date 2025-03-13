using KanbanWebApi.Dto.Column;
using KanbanWebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace KanbanWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColumnController(ColumnService columnService) : ControllerBase
    {
        private readonly ColumnService _columnService = columnService;

        [HttpPut("{id:guid}/tasks")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] IList<UpdateColumnTaskPriorityDto> dtos)
        {
            return Ok(await _columnService.UpdateAsync(id, dtos));
        }
    }
}