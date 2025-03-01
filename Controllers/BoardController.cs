using KanbanWebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace KanbanWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController(BoardService boardService) : ControllerBase
    {
        private readonly BoardService _boardService = boardService;

        public async Task<IActionResult> GetAsync()
        {
            return Ok(await _boardService.GetListAsync());
        }
    }
}