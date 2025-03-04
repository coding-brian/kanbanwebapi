using Dapper;
using System.Data;

namespace KanbanWebApi.Repository
{
    public class Repository<T>(IDbConnection connection) : IRepository<T>
    {
        public readonly IDbConnection _connection = connection;

        public async Task<List<T>> GetListAsync(string sql, object parameters)
        {
            if (parameters != null) return (await _connection.QueryAsync<T>(sql, parameters)).ToList();

            return (await _connection.QueryAsync<T>(sql)).ToList();
        }

        public async Task<T> GetAsync(string sql, object parameters)
        {
            if (parameters != null) return await _connection.QueryFirstOrDefaultAsync<T>(sql, parameters);

            return await _connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<int> InsertAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            if (parameters != null) return await _connection.ExecuteAsync(sql, parameters, transaction);

            return await _connection.ExecuteAsync(sql, transaction);
        }

        public async Task<int> UpdateAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            if (parameters != null) return await _connection.ExecuteAsync(sql, parameters, transaction);

            return await _connection.ExecuteAsync(sql, transaction);
        }

        public async Task<int> DeleteAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            if (parameters != null) return await _connection.ExecuteAsync(sql, parameters, transaction);

            return await _connection.ExecuteAsync(sql, transaction);
        }
    }

    public interface IRepository<T>
    {
        Task<List<T>> GetListAsync(string sql, object parameters);

        Task<T> GetAsync(string sql, object parameters);

        Task<int> InsertAsync(string sql, object parameters, IDbTransaction transaction = null);

        Task<int> UpdateAsync(string sql, object parameters, IDbTransaction transaction = null);

        Task<int> DeleteAsync(string sql, object parameters, IDbTransaction transaction = null);
    }
}