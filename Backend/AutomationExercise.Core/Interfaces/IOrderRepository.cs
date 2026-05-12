using AutomationExercise.Core.Models;

namespace AutomationExercise.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order entity);
        Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);
        Task<Order?> GetOrderWithItemsAsync(int orderId);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetByIdAsync(int id);
    }
}
