using AutoMapper;
using Dapper;
using KanbanWebApi.Dto.Column;
using KanbanWebApi.Dto.Task;
using KanbanWebApi.Repository;
using KanbanWebApi.Tables;
using Serilog;
using System.Data;

namespace KanbanWebApi.Service
{
    public class ColumnService(IMapper mapper,
                               IDbConnection connection,
                               IRepository<Column> repository,
                               IRepository<Board> boardRepository,
                               IRepository<Tables.Task> taskRepository,
                               ISqlGenerator<Column> sqlGenerator)
    {
        private readonly IMapper _mapper = mapper;

        private readonly IDbConnection _connection = connection;

        private readonly IRepository<Column> _repository = repository;
        private readonly IRepository<Board> _boardRepository = boardRepository;
        private readonly IRepository<Tables.Task> _taskRepository = taskRepository;

        private readonly ISqlGenerator<Column> _sqlGenerator = sqlGenerator;

        public async Task<ColumnDto> GetAsync(Guid id)
        {
            var sqlBuidler = new SqlBuilder();

            var template = sqlBuidler.AddTemplate($@"SELECT * FROM ""column""  c  /**leftjoin**/ WHERE c.id= @id and c.entity_status=true");

            sqlBuidler.LeftJoin(@$"(SELECT * FROM ""task"" WHERE entity_status=true) t on c.id = t.column_id");

            var columnDic = new Dictionary<Guid, ColumnDto>();

            return (await _connection.QueryAsync<Column, Tables.Task, ColumnDto>(template.RawSql, (column, task) =>
            {
                var dto = _mapper.Map<ColumnDto>(column);

                if (!columnDic.TryGetValue(dto.Id, out var existingColumn))
                {
                    existingColumn = dto;
                    columnDic.Add(dto.Id, dto);
                }

                if (task != null)
                {
                    var existingTask = existingColumn.Tasks.FirstOrDefault(item => item.Id == task.Id);

                    if (existingTask == null) existingColumn.Tasks.Add(_mapper.Map<TaskDto>(task));
                }

                return existingColumn;
            }, new { id })).Distinct().FirstOrDefault();
        }

        public async Task<ColumnDto> UpdateAsync(Guid id, IList<UpdateColumnTaskPriorityDto> dtos)
        {
            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var sql = @$"SELECT * FROM ""task""  WHERE id = ANY(@ids) and entity_status=true";

                var tasks = await _taskRepository.GetListAsync(sql, new { ids = dtos.Select(dto => dto.Id).ToList() });

                var updateSql = @"UPDATE ""task"" SET priority=@priority,column_id=@columnId WHERE id=@id";

                foreach (var task in tasks)
                {
                    var dto = dtos.FirstOrDefault(item => item.Id == task.Id);
                    task.Priority = dto.Priority;
                    task.ColumnId = id;
                }

                await _taskRepository.UpdateAsync(updateSql, tasks);

                transaction.Commit();

                return await GetAsync(id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error");
                transaction.Rollback();
                return null;
            }
        }
    }
}