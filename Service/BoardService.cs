using AutoMapper;
using KanbanWebApi.Dto;
using KanbanWebApi.Repository;
using KanbanWebApi.Tables;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace KanbanWebApi.Service
{
    public class BoardService(IRepository<Board> boardRepository, IMapper mapper)
    {
        private readonly IRepository<Board> _boardRepository = boardRepository;

        private readonly IMapper _mapper = mapper;

        public async Task<List<Board>> GetListAsync()
        {
            var sql = @"SELECT id, name, entity_status, creation_time, create_by, modification_time, modify_by, deletion_time, delete_by
                        FROM board";

            return await _boardRepository.GetListAsync(sql, null);
        }

        public async Task<int> CreateAsync(CreateBoardDto dto)
        {
            var board = _mapper.Map<Board>(dto);

            board.Id = Guid.NewGuid();
            board.EntityStatus = true;
            board.CreationTime = DateTime.Now;

            var properties = typeof(Board).GetProperties().Where(x => x.GetValue(board) != null);

            var columns = properties.Select(x => x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name);

            var sql = @$" INSERT INTO board({string.Join(",", columns)}) VALUES ({string.Join(",", properties.Select(x => $"@{x.Name}"))});";

            return await _boardRepository.InsertAsync(sql, board);
        }
    }
}