using AutoMapper;
using Dapper;
using KanbanWebApi.Dto;
using KanbanWebApi.Dto.SubTask;
using KanbanWebApi.Repository;
using KanbanWebApi.Tables;
using Serilog;
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

            var selectColumns = typeof(BoardDto).GetProperties().Select(x => x.Name);

            var columns = typeof(Board).GetProperties().Where(x => selectColumns.Contains(x.Name)).Select(x => x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name);

            var template = sqlBuilder.AddTemplate(@"
                                                    SELECT b.id,b.name,b.member_id,
                                                           c.id,c.board_id,c.name,c.is_active,
                                                           t.id,t.title,t.column_id,t.board_id,
                                                           st.id,st.title,st.is_completed,st.task_id
                                                    FROM board b
                                                    /**leftjoin**/
                                                    /**where**/");

            sqlBuilder.Where("b.id = @id", new { Id = id });
            sqlBuilder.Where("b.entity_status = true");

            sqlBuilder.LeftJoin(@"(SELECT * FROM  ""column"" WHERE entity_status = true) c on b.id = c.board_id");
            sqlBuilder.LeftJoin(@"( SELECT * FROM  task WHERE entity_status = true) t on t.column_id = c.id");
            sqlBuilder.LeftJoin(@"( SELECT * FROM  sub_task  WHERE entity_status = true) st on st.task_id = t.id");

            var boardDic = new Dictionary<Guid, BoardDto>();

            return (await _connection.QueryAsync<Board, Column, Tables.Task, SubTask, BoardDto>(template.RawSql, (board, column, task, subTask) =>
            {
                var dto = _mapper.Map<BoardDto>(board);

                if (!boardDic.TryGetValue(dto.Id, out var existingBoard))
                {
                    existingBoard = dto;

                    boardDic.Add(dto.Id, dto);
                }

                if (column != null)
                {
                    var existingColumn = existingBoard.Columns.FirstOrDefault(c => c.Id == column.Id);

                    if (existingColumn == null)
                    {
                        existingColumn = _mapper.Map<ColumnDto>(column);
                        existingBoard.Columns.Add(existingColumn);
                    }

                    if (task != null)
                    {
                        var existingTask = existingColumn.Tasks.FirstOrDefault(t => t.Id == task.Id);

                        if (existingTask == null)
                        {
                            existingTask = _mapper.Map<TaskDto>(task);
                            existingColumn.Tasks.Add(existingTask);
                        }

                        if (subTask != null)
                        {
                            var existingSubTask = existingTask.SubTasks.FirstOrDefault(x => x.Id == subTask.Id);

                            if (existingSubTask == null)
                            {
                                existingSubTask = _mapper.Map<SubTaskDto>(subTask);
                                existingTask.SubTasks.Add(existingSubTask);
                            }
                        }
                    }
                }

                return existingBoard;
            }, template.Parameters)).Distinct().FirstOrDefault();
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

                    await _columnRepository.InsertAsync(_columnSqlGenerator.GenerateInsertSQL<CreateBoardDto>(null), columns);
                }

                await _boardRepository.InsertAsync(_boardSqlGenerator.GenerateInsertSQL(board), board);

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error");

                transaction.Rollback();

                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var sqlBuilder = new SqlBuilder();

                var template = sqlBuilder.AddTemplate($@"UPDATE board SET entity_status = false  /**where**/");

                sqlBuilder.Where("id = @Id", new { Id = id });

                transaction.Commit();

                await _boardRepository.DeleteAsync(template.RawSql, template.Parameters, transaction);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error");

                transaction.Rollback();

                return false;
            }
        }

        public async Task<bool> UpdateAsync(UpdateBoardDto dto)
        {
            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var updateBoard = _mapper.Map<Board>(dto);

                var boardSqlBuilder = new SqlBuilder();

                var selectColumns = typeof(UpdateBoardDto).GetProperties().Select(x => x.Name);

                var properties = typeof(Board).GetProperties().Where(x => x.Name != "Id").Where(x => selectColumns.Contains(x.Name));

                var template = boardSqlBuilder.AddTemplate($@"UPDATE board  /**set**/  /**where**/");

                foreach (var propertyName in properties.Select(x => x.Name))
                {
                    boardSqlBuilder.Set($"{propertyName}=@{propertyName}");
                }

                boardSqlBuilder.Where("id = @id", new { dto.Id });

                await _boardRepository.UpdateAsync(template.RawSql, dto, transaction);

                if (dto.Columns != null && dto.Columns.Count > 0)
                {
                    var columns = _mapper.Map<List<Column>>(dto.Columns);

                    foreach (var column in columns)
                    {
                        column.ModificationTime = DateTime.Now;
                    }

                    var columnSqlBuilder = new SqlBuilder();
                    var columnTemplate = columnSqlBuilder.AddTemplate($@"UPDATE ""column""  /**set**/  /**where**/");

                    columnSqlBuilder.Where("id = @id");

                    columnSqlBuilder.Set("name = @Name");

                    columnSqlBuilder.Set("modification_time = @ModificationTime");

                    await _columnRepository.UpdateAsync(columnTemplate.RawSql, columns, transaction);
                }

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error");

                transaction.Rollback();

                return false;
            }
        }
    }
}