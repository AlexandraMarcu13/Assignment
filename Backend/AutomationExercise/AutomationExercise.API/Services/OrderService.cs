using AutomationExercise.Core.DTOs;
using AutomationExercise.Core.Interfaces;
using AutomationExercise.Core.Models;
using AutomationExercise.Data.Repositories;

namespace AutomationExercise.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<Order> CreateOrderAsync(int userId, OrderRequestDto orderRequest)
        {
            // Validate cart is not empty
            if (orderRequest.Items == null || !orderRequest.Items.Any())
            {
                throw new InvalidOperationException("Cart cannot be empty");
            }

            // Validate shipping address
            if (string.IsNullOrWhiteSpace(orderRequest.ShippingAddress))
            {
                throw new InvalidOperationException("Shipping address is required");
            }

            // Validate and calculate totals from database
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in orderRequest.Items)
            {
                // Fetch product from database to ensure correct price
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product {item.ProductId} not found");
                }

                if (product.Stock < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.Stock}");
                }

                // Use database price, NOT client price
                var unitPrice = product.Price;
                var itemTotal = unitPrice * item.Quantity;
                totalAmount += itemTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = itemTotal
                });
            }

            // Create order WITHOUT items first (just the order header)
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = orderRequest.ShippingAddress,
                City = orderRequest.City ?? "",
                PostalCode = orderRequest.PostalCode ?? "",
                Country = orderRequest.Country ?? "",
                TotalAmount = totalAmount,
                Status = "Pending"
            };

            // Save order and get the ID
            var createdOrder = await _orderRepository.AddAsync(order);

            // Now add each order item with the new OrderId
            foreach (var item in orderItems)
            {
                item.OrderId = createdOrder.Id;
                await _orderRepository.AddOrderItemAsync(item);

                // Update stock
                await _productRepository.UpdateStockAsync(item.ProductId, item.Quantity);
            }

            // Return order with items
            return await _orderRepository.GetOrderWithItemsAsync(createdOrder.Id) ?? createdOrder;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _orderRepository.GetUserOrdersAsync(userId);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepository.GetOrderWithItemsAsync(orderId);
        }
    }
}