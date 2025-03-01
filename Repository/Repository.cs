using Dapper;
using System.Data;

namespace KanbanWebApi.Repository
{
    public class Repository<T>(IDbConnection connection) : IRepository<T>
    {
        private readonly IDbConnection _connection = connection;

        public async Task<List<T>> GetListAsync(string sql, object parameters)
        {
            if (parameters != null) return (await _connection.QueryAsync<T>(sql, parameters)).ToList();

            return (await _connection.QueryAsync<T>(sql)).ToList();
        }

        public async Task<T> GetAsync(string sql, object parameters)
        {
            if (parameters != null) return await _connection.QueryFirstOrDefaultAsync<T>(sql, parameters);

            return await _connection.QueryFirstOrDefaultAsync<T>(sql);
        }

        public async Task<int> InsertAsync(string sql, object parameters)
        {
            if (parameters != null) return await _connection.ExecuteAsync(sql, parameters);

            return await _connection.ExecuteAsync(sql);
        }

        public async Task<int> UpdateAsync(string sql, object parameters)
        {
            if (parameters != null) return await _connection.ExecuteAsync(sql, parameters);

            return await _connection.ExecuteAsync(sql);
        }

        public async Task<int> DeleteAsync(string sql, object parameters)
        {
            if (parameters != null) return await _connection.ExecuteAsync(sql, parameters);

            return await _connection.ExecuteAsync(sql);
        }
    }

    public interface IRepository<T>
    {
        Task<List<T>> GetListAsync(string sql, object parameters);

        Task<T> GetAsync(string sql, object parameters);

        Task<int> InsertAsync(string sql, object parameters);

        Task<int> UpdateAsync(string sql, object parameters);

        Task<int> DeleteAsync(string sql, object parameters);
    }
}