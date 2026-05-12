using AutomationExercise.Core.Interfaces;
using AutomationExercise.Core.Models;
using AutomationExercise.Data.Database;
using Microsoft.Data.SqlClient;

namespace AutomationExercise.Data.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Orders")
        {
        }

        public async Task<Order> AddWithTransactionAsync(Order entity, SqlConnection connection, SqlTransaction transaction)
        {
            var query = @"
            INSERT INTO Orders (UserId, OrderDate, ShippingAddress, City, PostalCode, Country, TotalAmount, Status)
            VALUES (@UserId, @OrderDate, @ShippingAddress, @City, @PostalCode, @Country, @TotalAmount, @Status);
            SELECT SCOPE_IDENTITY();";

            using var command = new SqlCommand(query, connection, transaction);

            command.Parameters.AddWithValue("@UserId", entity.UserId);
            command.Parameters.AddWithValue("@OrderDate", entity.OrderDate);
            command.Parameters.AddWithValue("@ShippingAddress", entity.ShippingAddress);
            command.Parameters.AddWithValue("@City", string.IsNullOrEmpty(entity.City) ? "" : entity.City);
            command.Parameters.AddWithValue("@PostalCode", string.IsNullOrEmpty(entity.PostalCode) ? "" : entity.PostalCode);
            command.Parameters.AddWithValue("@Country", string.IsNullOrEmpty(entity.Country) ? "" : entity.Country);
            command.Parameters.AddWithValue("@TotalAmount", entity.TotalAmount);
            command.Parameters.AddWithValue("@Status", entity.Status);

            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
            entity.Id = newId;

            return entity;
        }

        public async Task<OrderItem> AddOrderItemWithTransactionAsync(OrderItem orderItem, SqlConnection connection, SqlTransaction transaction)
        {
            var query = @"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice) 
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice);
            SELECT SCOPE_IDENTITY();";

            using var command = new SqlCommand(query, connection, transaction);

            command.Parameters.AddWithValue("@OrderId", orderItem.OrderId);
            command.Parameters.AddWithValue("@ProductId", orderItem.ProductId);
            command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
            command.Parameters.AddWithValue("@UnitPrice", orderItem.UnitPrice);
            command.Parameters.AddWithValue("@TotalPrice", orderItem.TotalPrice);

            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
            orderItem.Id = newId;

            return orderItem;
        }

        public override async Task<Order> AddAsync(Order entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var result = await AddWithTransactionAsync(entity, connection, transaction);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var result = await AddOrderItemWithTransactionAsync(orderItem, connection, transaction);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order?> GetOrderWithItemsAsync(int orderId)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null) return null;

            order.OrderItems = await GetOrderItemsAsync(orderId);
            return order;
        }

        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            var items = new List<OrderItem>();
            using var connection = _connectionFactory.CreateConnection();
            var query = @"
            SELECT oi.*, p.Name as ProductName, p.Price as ProductPrice 
            FROM OrderItems oi 
            JOIN Products p ON oi.ProductId = p.Id 
            WHERE oi.OrderId = @OrderId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = new OrderItem
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                    TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice"))
                };
                items.Add(item);
            }

            return items;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = new List<Order>();
            using var connection = _connectionFactory.CreateConnection();
            var query = "SELECT * FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var order = MapToEntity(reader);
                orders.Add(order);
            }

            return orders;
        }

        protected override Order MapToEntity(SqlDataReader reader)
        {
            return new Order
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                City = reader.GetString(reader.GetOrdinal("City")),
                PostalCode = reader.GetString(reader.GetOrdinal("PostalCode")),
                Country = reader.GetString(reader.GetOrdinal("Country")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                Status = reader.GetString(reader.GetOrdinal("Status"))
            };
        }
    }
}