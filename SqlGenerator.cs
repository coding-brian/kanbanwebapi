using Dapper;
using KanbanWebApi.Tables;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace KanbanWebApi
{
    public class SqlGenerator<T> : ISqlGenerator<T> where T : Entity
    {
        public List<PropertyInfo> Properties = typeof(T).GetProperties().ToList();

        private List<ColumnPropertyNameMapping> _columnMappings = typeof(T).GetProperties()
                                                                           .Select(x => new ColumnPropertyNameMapping { ColumnName = x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name, PropertyName = x.Name })
                                                                           .ToList();

        private readonly string _tableName = typeof(T).GetCustomAttribute<TableAttribute>().Name.ToLower();

        public string GenerateSelectSQL(Guid? id)
        {
            var sql = @$"SELECT * FROM ""{_tableName}"" /**where**/";

            var sqlBuilder = new SqlBuilder();

            sqlBuilder.Where("entity_status=true");

            if (id.HasValue)
            {
                sqlBuilder.Where("id = @id", new { id });
            }

            var template = sqlBuilder.AddTemplate(sql);

            return template.RawSql;
        }

        public string GenerateInsertSQL()
        {
            return @$" INSERT INTO ""{_tableName}"" ({string.Join(",", _columnMappings.Select(c => c.ColumnName))}) VALUES ({string.Join(",", _columnMappings.Select(x => $"@{x.PropertyName}"))});";
        }

        public string GenerateUpdateSQL()
        {
            var columns = _columnMappings.Where(c => c.ColumnName != "id").ToList();

            var sql = @$"UPDATE ""{_tableName}"" SET {string.Join(",", columns.Select(x => $"{x.ColumnName}=@{x.PropertyName}"))} /**where**/";

            var sqlBuilder = new SqlBuilder();

            sqlBuilder.Where("id = @id");

            var template = sqlBuilder.AddTemplate(sql);

            return template.RawSql;
        }
    }

    public interface ISqlGenerator<T>
    {
        string GenerateInsertSQL();

        string GenerateSelectSQL(Guid? id);

        string GenerateUpdateSQL();
    }

    public class ColumnPropertyNameMapping()
    {
        public string ColumnName { get; set; }

        public string PropertyName { get; set; }
    }
}