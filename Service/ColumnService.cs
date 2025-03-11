using AutoMapper;
using Dapper;
using KanbanWebApi.Dto;
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
                               ISqlGenerator<Column> sqlGenerator)
    {
        private readonly IMapper _mapper = mapper;

        private readonly IDbConnection _connection = connection;

        private readonly IRepository<Column> _repository = repository;
        private readonly IRepository<Board> _boardRepository = boardRepository;

        private readonly ISqlGenerator<Column> _sqlGenerator = sqlGenerator;

        public async Task<List<ColumnDto>> CreateAsync(IList<CreateColumnDto> dtos)
        {
            if (dtos == null || dtos.Count == 0) return null;

            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var columns = new List<Column>();

                foreach (var dto in dtos)
                {
                    var column = _mapper.Map<Column>(dto);
                    column.Id = Guid.NewGuid();
                    column.IsActive = true;
                    column.EntityStatus = true;
                    column.CreationTime = DateTime.Now;

                    columns.Add(column);
                }

                await _repository.InsertAsync(_sqlGenerator.GenerateInsertSQL<CreateColumnDto>(null), columns, transaction);

                transaction.Commit();

                var sqlBuilder = new SqlBuilder();

                var template = sqlBuilder.AddTemplate(@"SELECT * FROM ""column"" /**where**/");

                sqlBuilder.Where("id=ANY(@ids)", new { ids = columns.Select(x => x.Id).ToList() });

                return _mapper.Map<List<ColumnDto>>(await _repository.GetListAsync(template.RawSql, template.Parameters));
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