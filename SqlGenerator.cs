using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace KanbanWebApi
{
    public class SqlGenerator<T> : ISqlGenerator<T>
    {
        private readonly PropertyInfo[] _properties = typeof(T).GetProperties();

        private readonly List<string> _columns = typeof(T).GetProperties().Select(x => x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name).ToList();

        private readonly string _tableName = typeof(T).Name.ToLower();

        public string GenerateSelectSQL()
        {
            var sql = @$"SELECT {string.Join(",", _columns)} FROM {_tableName}";

            return sql;
        }

        public string GenerateInsertSQL(T source)
        {
            var properties = typeof(T).GetProperties().ToList();

            if (source != null)
            {
                properties = properties.Where(x => x.GetValue(source) != null).ToList();
            }

            var columns = properties.Select(x => x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name);

            return @$" INSERT INTO ""{_tableName}"" ({string.Join(",", columns)}) VALUES ({string.Join(",", properties.Select(x => $"@{x.Name}"))});";
        }
    }

    public interface ISqlGenerator<T>
    {
        string GenerateInsertSQL(T source);

        string GenerateSelectSQL();
    }
}