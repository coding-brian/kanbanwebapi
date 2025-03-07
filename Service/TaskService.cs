using AutoMapper;
using Dapper;
using KanbanWebApi.Dto.SubTask;
using KanbanWebApi.Dto.Task;
using KanbanWebApi.Repository;
using KanbanWebApi.Tables;
using Serilog;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

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

            var template = sqlBuilder.AddTemplate(@"SELECT  t.id,t.column_id,t.board_id,t.title,t.description,
                                                            st.id,st.task_id,st.title,st.is_completed
                                                    FROM Task t
                                                    /**leftjoin**/
                                                    /**where**/");

            sqlBuilder.Where("t.id=@id", new { id });

            sqlBuilder.LeftJoin("sub_task st on t.id = st.task_id");

            var taskDic = new Dictionary<Guid, TaskDto>();

            return (await _connection.QueryAsync<Tables.Task, SubTask, TaskDto>(template.RawSql, (task, subTask) =>
            {
                var dto = _mapper.Map<TaskDto>(task);

                if (!taskDic.TryGetValue(task.Id, out var existingTask))
                {
                    existingTask = dto;

                    taskDic.Add(task.Id, dto);
                }

                if (subTask != null)
                {
                    var existingSubTask = existingTask.SubTasks.FirstOrDefault(x => x.Id == subTask.Id);
                    if (existingSubTask == null)
                    {
                        existingTask.SubTasks.Add(_mapper.Map<SubTaskDto>(subTask));
                    }
                }

                return existingTask;
            }, template.Parameters)).Distinct().FirstOrDefault();
        }

        public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
        {
            if (dto == null) return null;

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
                        subTask.IsCompleted = false;
                        subTask.TaskId = task.Id;
                        subTask.EntityStatus = true;
                        subTask.CreationTime = DateTime.Now;

                        subTasks.Add(subTask);
                    }
                    await _subTaskRepository.InsertAsync(_subTaskSqlGenerator.GenerateInsertSQL<CreateTaskDto>(null), subTasks, transaction);
                }

                await _repository.InsertAsync(_sqlGenerator.GenerateInsertSQL(task), task, transaction);

                transaction.Commit();

                return await GetAsync(task.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error");

                return null;
            }
        }

        public async Task<TaskDto> UpdateAsync(UpdateTaskDto dto)
        {
            if (dto == null) return null;

            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var task = await _repository.GetAsync("SELECT * FROM Task WHERE id=@id", new { id = dto.Id });

                _mapper.Map(dto, task);

                task.ModificationTime = DateTime.Now;

                var properties = typeof(Tables.Task).GetProperties().Where(x => x.Name != "Id");

                var sqlBuilder = new SqlBuilder();

                var template = sqlBuilder.AddTemplate($@"UPDATE task /**set**/  /**where**/");

                sqlBuilder.Where("id=@id", new { dto.Id });

                var names = properties.Select(x => new { columnName = x.GetCustomAttribute<ColumnAttribute>()?.Name, parameteName = x.Name });

                sqlBuilder.Set($"{string.Join(",", names.Select(x => $@"{x.columnName}=@{x.parameteName}"))}");

                await _repository.UpdateAsync(template.RawSql, task, transaction);

                if (dto.SubTasks.Count > 0)
                {
                    var subTasks = await _subTaskRepository.GetListAsync("SELECT * FROM sub_task WHERE id = ANY(@ids) ", new { ids = dto.SubTasks.Select(item => item.Id).ToArray() });

                    var updateSubTaskDtos = dto.SubTasks.Where(x => x.Id != null).ToList();

                    if (updateSubTaskDtos.Count > 0)
                    {
                        var updateSubTasks = subTasks.Where(x => updateSubTaskDtos.Select(y => y.Id).Contains(x.Id));

                        var subTaskProperties = typeof(SubTask).GetProperties().Where(x => x.Name != "Id");

                        var subTaskNames = subTaskProperties.Select(x => new { columnName = x.GetCustomAttribute<ColumnAttribute>()?.Name, parameteName = x.Name });

                        var subTaskSqlBuilder = new SqlBuilder();

                        var subTaskTemplate = subTaskSqlBuilder.AddTemplate($@"UPDATE sub_task  /**set**/  /**where**/");

                        foreach (var updateSubTask in updateSubTasks)
                        {
                            var updateSubTaskDto = updateSubTaskDtos.First(x => x.Id == updateSubTask.Id);

                            _mapper.Map(updateSubTaskDto, updateSubTask);

                            updateSubTask.ModificationTime = DateTime.Now;
                        }

                        subTaskSqlBuilder.Where("id=@id");
                        subTaskSqlBuilder.Set($"{string.Join(",", subTaskNames.Select(x => $@"{x.columnName}=@{x.parameteName}"))}");

                        await _subTaskRepository.UpdateAsync(subTaskTemplate.RawSql, updateSubTasks, transaction);
                    }

                    var createSubTaskDtos = dto.SubTasks.Where(x => x.Id == null).ToList();

                    if (createSubTaskDtos.Any())
                    {
                        var createSubTaskProperties = typeof(SubTask).GetProperties();

                        var createSubTaskNames = createSubTaskProperties.Select(x => new { columnName = x.GetCustomAttribute<ColumnAttribute>()?.Name, parameteName = x.Name });

                        var createSubTaskSqlBuilder = new SqlBuilder();
                        var createSubTaskTemplate =
                            sqlBuilder.AddTemplate($@"INSERT INTO sub_task ({string.Join(",", createSubTaskNames.Select(x => x.columnName))})  VALUES ({string.Join(",", createSubTaskNames.Select(x => $"@{x.parameteName}"))})");

                        var createSubTasks = _mapper.Map<List<SubTask>>(createSubTaskDtos);

                        foreach (var createSubTask in createSubTasks)
                        {
                            createSubTask.Id = Guid.NewGuid();
                            createSubTask.CreationTime = DateTime.Now;
                            createSubTask.EntityStatus = true;
                        }

                        await _subTaskRepository.UpdateAsync(createSubTaskTemplate.RawSql, createSubTasks, transaction);
                    }
                }

                transaction.Commit();

                return await GetAsync(dto.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error");

                transaction.Rollback();

                return null;
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