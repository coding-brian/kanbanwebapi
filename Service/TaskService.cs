using AutoMapper;
using Dapper;
using KanbanWebApi.Dto;
using KanbanWebApi.Dto.Task;
using KanbanWebApi.Repository;
using KanbanWebApi.Tables;
using Serilog;
using System.Data;

namespace KanbanWebApi.Service
{
    public class TaskService(IMapper mapper,
                             IDbConnection connection,
                             IRepository<Tables.Task> repository,
                             IRepository<SubTask> subTaskRepository,
                             ISqlGenerator<Tables.Task> sqlGenerator,
                             ISqlGenerator<SubTask> subTaskSqlGenerator)
    {
        private readonly IMapper _mapper = mapper;

        private readonly IDbConnection _connection = connection;

        private readonly IRepository<Tables.Task> _repository = repository;

        private readonly IRepository<SubTask> _subTaskRepository = subTaskRepository;

        private readonly ISqlGenerator<Tables.Task> _sqlGenerator = sqlGenerator;

        private readonly ISqlGenerator<SubTask> _subTaskSqlGenerator = subTaskSqlGenerator;

        public async Task<TaskDto> GetAsync(Guid id)
        {
            var sqlBuilder = new SqlBuilder();

            var template = sqlBuilder.AddTemplate(@"SELECT * FROM Task /**where**/");

            sqlBuilder.Where("id=@id", new { id });

            return _mapper.Map<TaskDto>(await _repository.GetAsync(template.RawSql, template.Parameters));
        }

        public async Task<bool> CreateAsync(CreateTaskDto dto)
        {
            if (dto == null) return false;

            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var task = _mapper.Map<Tables.Task>(dto);
                task.Id = Guid.NewGuid();
                task.EntityStatus = true;
                task.CreationTime = DateTime.Now;

                if (dto.SubTasks != null && dto.SubTasks.Count > 0)
                {
                    var subTasks = new List<SubTask>();
                    foreach (var subTaskDto in dto.SubTasks)
                    {
                        var subTask = _mapper.Map<SubTask>(subTaskDto);
                        subTask.Id = Guid.NewGuid();
                        subTask.TaskId = task.Id;
                        subTask.EntityStatus = true;
                        subTask.CreationTime = DateTime.Now;

                        subTasks.Add(subTask);
                    }
                    await _subTaskRepository.InsertAsync(_subTaskSqlGenerator.GenerateInsertSQL<CreateTaskDto>(null), subTasks, transaction);
                }

                await _repository.InsertAsync(_sqlGenerator.GenerateInsertSQL(task), task, transaction);

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error");

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

                var template = sqlBuilder.AddTemplate($@"UPDATE task SET entity_status = false    /**where**/");

                sqlBuilder.Where("id = @Id", new { Id = id });

                await _repository.UpdateAsync(template.RawSql, template.Parameters, transaction);

                var subTaskSqlBuilder = new SqlBuilder();

                var subTaskTemplate = subTaskSqlBuilder.AddTemplate("UPDATE sub_task SET entity_status=false /**where**/");

                subTaskSqlBuilder.Where("task_id = @Id", new { Id = id });

                await _repository.UpdateAsync(subTaskTemplate.RawSql, subTaskTemplate.Parameters, transaction);

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