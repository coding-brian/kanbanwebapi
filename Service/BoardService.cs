using KanbanWebApi.Repository;
using KanbanWebApi.Tables;

namespace KanbanWebApi.Service
{
    public class BoardService(IRepository<Board> boardRepository)
    {
        private readonly IRepository<Board> _boardRepository = boardRepository;

        public async Task<List<Board>> GetListAsync()
        {
            var sql = @"SELECT id, name, entity_status, creation_time, create_by, modification_time, modify_by, deletion_time, delete_by
                        FROM board";

            return await _boardRepository.GetListAsync(sql, null);
        }
    }
}