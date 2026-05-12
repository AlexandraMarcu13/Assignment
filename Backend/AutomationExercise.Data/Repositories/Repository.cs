using AutomationExercise.Core.Interfaces;
using AutomationExercise.Data.Database;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace AutomationExercise.Data.Repositories
{

    public abstract class Repository<T> : IRepository<T> where T : class, new()
    {
        protected readonly IDbConnectionFactory _connectionFactory;
        protected readonly string _tableName;
        protected readonly string _idColumn;

        protected Repository(IDbConnectionFactory connectionFactory, string tableName, string idColumn = "Id")
        {
            _connectionFactory = connectionFactory;
            _tableName = tableName;
            _idColumn = idColumn;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var query = $"SELECT * FROM {_tableName} WHERE {_idColumn} = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapToEntity(reader);
            }

            return null;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var results = new List<T>();
            using var connection = _connectionFactory.CreateConnection();
            var query = $"SELECT * FROM {_tableName}";

            using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(MapToEntity(reader));
            }

            return results;
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var results = await GetAllAsync();
            return results.AsQueryable().Where(predicate).ToList();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _idColumn && p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                .Where(p => !p.PropertyType.IsGenericType)
                .ToList();

            var columns = string.Join(", ", properties.Select(p => p.Name));
            var parameters = string.Join(", ", properties.Select(p => "@" + p.Name));

            var query = $@"INSERT INTO {_tableName} ({columns}) 
                       VALUES ({parameters});
                       SELECT SCOPE_IDENTITY();";

            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(query, connection);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity) ?? DBNull.Value;
                command.Parameters.AddWithValue("@" + prop.Name, value);
            }

            await connection.OpenAsync();
            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());

            var idProperty = typeof(T).GetProperty(_idColumn);
            if (idProperty != null)
            {
                idProperty.SetValue(entity, newId);
            }

            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _idColumn)
                .ToList();

            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
            var idValue = typeof(T).GetProperty(_idColumn)?.GetValue(entity);

            var query = $"UPDATE {_tableName} SET {setClause} WHERE {_idColumn} = @Id";

            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(query, connection);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity) ?? DBNull.Value;
                command.Parameters.AddWithValue("@" + prop.Name, value);
            }

            command.Parameters.AddWithValue("@Id", idValue ?? 0);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var query = $"DELETE FROM {_tableName} WHERE {_idColumn} = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            var results = await GetAllAsync();
            return results.AsQueryable().Any(predicate);
        }

        protected abstract T MapToEntity(SqlDataReader reader);
    }
}