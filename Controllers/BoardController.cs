using KanbanWebApi.Dto;
using KanbanWebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace KanbanWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController(BoardService boardService) : ControllerBase
    {
        private readonly BoardService _boardService = boardService;

        [HttpGet]
        public async Task<IActionResult> GetListAsync()
        {
            return Ok(await _boardService.GetListAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            return Ok(await _boardService.GetAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateBoardDto dto)
        {
            return Ok(await _boardService.CreateAsync(dto));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateBoardDto dto)
        {
            return Ok(await _boardService.UpdateAsync(dto));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            return Ok(await _boardService.DeleteAsync(id));
        }
    }
}