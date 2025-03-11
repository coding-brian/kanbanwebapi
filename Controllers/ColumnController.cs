using KanbanWebApi.Dto;
using KanbanWebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace KanbanWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColumnController(ColumnService columnService) : ControllerBase
    {
        private readonly ColumnService _columnService = columnService;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] IList<CreateColumnDto> dto)
        {
            return Ok(await _columnService.CreateAsync(dto));
        }
    }
}