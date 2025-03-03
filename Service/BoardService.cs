using AutoMapper;
using Dapper;
using KanbanWebApi.Dto;
using KanbanWebApi.Repository;
using KanbanWebApi.Tables;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

namespace KanbanWebApi.Service
{
    public class BoardService(IRepository<Board> boardRepository,
                              IRepository<Column> columnRepository,
                              IMapper mapper,
                              ISqlGenerator<Board> boardSqlGenerator,
                              ISqlGenerator<Column> columnSqlGenerator,
                              IDbConnection connection)
    {
        private readonly IRepository<Board> _boardRepository = boardRepository;
        private readonly IRepository<Column> _columnRepository = columnRepository;

        private readonly IMapper _mapper = mapper;

        private readonly ISqlGenerator<Board> _boardSqlGenerator = boardSqlGenerator;
        private readonly ISqlGenerator<Column> _columnSqlGenerator = columnSqlGenerator;

        private readonly IDbConnection _connection = connection;

        public async Task<List<BoardDto>> GetListAsync()
        {
            return _mapper.Map<List<BoardDto>>(await _boardRepository.GetListAsync(_boardSqlGenerator.GenerateSelectSQL(), null));
        }

        public async Task<BoardDto> GetAsync(Guid id)
        {
            var sqlBuilder = new SqlBuilder();

            var columes = typeof(BoardDto).GetProperties().Select(x => x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name);

            var template = sqlBuilder.AddTemplate($@"SELECT {string.Join(",", columes)} FROM board /** where **/");

            sqlBuilder.Where("id = @Id", new { Id = id });

            return _mapper.Map<BoardDto>(await _boardRepository.GetAsync(template.RawSql, template.Parameters));
        }

        public async Task<bool> CreateAsync(CreateBoardDto dto)
        {
            if (dto.MemberId == null) return false;

            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var board = _mapper.Map<Board>(dto);

                board.Id = Guid.NewGuid();
                board.EntityStatus = true;
                board.CreationTime = DateTime.Now;

                if (dto.Colums != null && dto.Colums.Count > 0)
                {
                    var columns = new List<Column>();

                    foreach (var columnDto in dto.Colums)
                    {
                        var column = _mapper.Map<Column>(columnDto);
                        column.Id = Guid.NewGuid();
                        column.BoardId = board.Id;
                        column.IsActive = true;
                        column.EntityStatus = true;
                        column.CreationTime = DateTime.Now;

                        columns.Add(column);
                    }

                    await _columnRepository.InsertAsync(_columnSqlGenerator.GenerateInsertSQL(null), columns);
                }

                await _boardRepository.InsertAsync(_boardSqlGenerator.GenerateInsertSQL(board), board);

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return false;
            }
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var sqlBuilder = new SqlBuilder();

            var template = sqlBuilder.AddTemplate($@"DELETE FROM board /** where **/");

            sqlBuilder.Where("id = @Id", new { Id = id });

            return await _boardRepository.DeleteAsync(template.RawSql, template.Parameters);
        }
    }
}