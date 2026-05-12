using AutomationExercise.Core.Interfaces;
using AutomationExercise.Core.Models;
using AutomationExercise.Data.Database;
using Microsoft.Data.SqlClient;

namespace AutomationExercise.Data.Repositories
{

    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Products")
        {
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            using var connection = _connectionFactory.CreateConnection();
            var query = "UPDATE Products SET Stock = Stock - @Quantity WHERE Id = @ProductId AND Stock >= @Quantity";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.Parameters.AddWithValue("@Quantity", quantity);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        protected override Product MapToEntity(SqlDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}