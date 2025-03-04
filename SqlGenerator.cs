using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace KanbanWebApi
{
    public class SqlGenerator<T> : ISqlGenerator<T>
    {
        private List<PropertyInfo> Properties = typeof(T).GetProperties().ToList();

        public List<string> Columns = typeof(T).GetProperties().Select(x => x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name).ToList();

        private readonly string _tableName = typeof(T).Name.ToLower();

        public string GenerateSelectSQL()
        {
            var sql = @$"SELECT {string.Join(",", Columns)} FROM {_tableName}";

            return sql;
        }

        public string GenerateInsertSQL<TSource>(TSource source)
        {
            if (source != null)
            {
                Properties = Properties.Where(x => x.GetValue(source) != null).ToList();
            }

            Columns = Properties.Select(x => x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name).ToList();

            return @$" INSERT INTO ""{_tableName}"" ({string.Join(",", Columns)}) VALUES ({string.Join(",", Properties.Select(x => $"@{x.Name}"))});";
        }

        public string GenerateUpdateSQL<TSource>(TSource source)
        {
            var sourceNames = typeof(TSource).GetProperties().Select(x => x.Name);

            Columns = Properties.Where(x => sourceNames.Contains(x.Name)).Select(x => x.GetCustomAttribute<ColumnAttribute>()?.Name ?? x.Name).ToList();

            var sql = @$"UPDATE {_tableName} SET {Columns.Select(x => $"{x}=@{x}")} WHERE 1=1";

            return sql;
        }
    }

    public interface ISqlGenerator<T>
    {
        string GenerateInsertSQL<TSource>(TSource source);

        string GenerateSelectSQL();

        string GenerateUpdateSQL<TSource>(TSource source);
    }
}