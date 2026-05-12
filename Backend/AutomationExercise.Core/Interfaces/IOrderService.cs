using AutomationExercise.Core.DTOs;
using AutomationExercise.Core.Models;

namespace AutomationExercise.Core.Interfaces
{ 
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(int userId, OrderRequestDto orderRequest);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int orderId);
    }
}