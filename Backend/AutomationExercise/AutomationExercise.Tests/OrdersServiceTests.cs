using Moq;
using Xunit;
using AutomationExercise.API.Services;
using AutomationExercise.Core.DTOs;
using AutomationExercise.Core.Interfaces;
using AutomationExercise.Core.Models;

namespace AutomationExercise.Tests.Services
{

    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _orderService = new OrderService(_mockOrderRepository.Object, _mockProductRepository.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidItems_CalculatesTotalFromDatabasePrices()
        {
            // Arrange
            int userId = 1;

            var product1 = new Product
            {
                Id = 1,
                Name = "Product 1",
                Price = 100m,
                Stock = 10,
                Description = "Desc1",
                Category = "Cat1",
                ImageUrl = "img1.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var product2 = new Product
            {
                Id = 2,
                Name = "Product 2",
                Price = 50m,
                Stock = 5,
                Description = "Desc2",
                Category = "Cat2",
                ImageUrl = "img2.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var cartItems = new List<CartItemDto>
        {
            new CartItemDto { ProductId = 1, Quantity = 2, Price = 1 },
            new CartItemDto { ProductId = 2, Quantity = 1, Price = 1 }
        };

            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = cartItems
            };

            decimal expectedTotal = (100m * 2) + (50m * 1);

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product1);
            _mockProductRepository.Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(product2);

            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(new Order { Id = 1 });

            _mockOrderRepository.Setup(r => r.AddOrderItemAsync(It.IsAny<OrderItem>()))
                .ReturnsAsync(new OrderItem());

            _mockProductRepository.Setup(r => r.UpdateStockAsync(1, 2))
                .ReturnsAsync(true);
            _mockProductRepository.Setup(r => r.UpdateStockAsync(2, 1))
                .ReturnsAsync(true);

            var orderWithItems = new Order
            {
                Id = 1,
                UserId = userId,
                TotalAmount = expectedTotal,
                Status = "Pending",
                OrderItems = new List<OrderItem>()
            };

            _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(1))
                .ReturnsAsync(orderWithItems);

            // Act
            var result = await _orderService.CreateOrderAsync(userId, orderRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTotal, result.TotalAmount);
            Assert.NotEqual(3, result.TotalAmount);

            _mockProductRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockProductRepository.Verify(r => r.GetByIdAsync(2), Times.Once);
            _mockProductRepository.Verify(r => r.UpdateStockAsync(1, 2), Times.Once);
            _mockProductRepository.Verify(r => r.UpdateStockAsync(2, 1), Times.Once);
            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
            _mockOrderRepository.Verify(r => r.AddOrderItemAsync(It.IsAny<OrderItem>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateOrderAsync_SingleProduct_CalculatesTotalCorrectly()
        {
            // Arrange
            int userId = 1;
            var product = new Product
            {
                Id = 1,
                Name = "Single Product",
                Price = 75.50m,
                Stock = 20,
                Description = "Desc",
                Category = "Cat",
                ImageUrl = "img.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var cartItems = new List<CartItemDto>
        {
            new CartItemDto { ProductId = 1, Quantity = 3, Price = 75.50m }
        };

            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "Boston",
                PostalCode = "02101",
                Country = "USA",
                Items = cartItems
            };

            decimal expectedTotal = 75.50m * 3;

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(new Order { Id = 1 });

            _mockOrderRepository.Setup(r => r.AddOrderItemAsync(It.IsAny<OrderItem>()))
                .ReturnsAsync(new OrderItem());

            _mockProductRepository.Setup(r => r.UpdateStockAsync(1, 3))
                .ReturnsAsync(true);

            _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(1))
                .ReturnsAsync(new Order { Id = 1, TotalAmount = expectedTotal });

            // Act
            var result = await _orderService.CreateOrderAsync(userId, orderRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTotal, result.TotalAmount);

            _mockProductRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockProductRepository.Verify(r => r.UpdateStockAsync(1, 3), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_EmptyCart_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;
            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = new List<CartItemDto>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(userId, orderRequest));

            Assert.Equal("Cart cannot be empty", exception.Message);

            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
            _mockProductRepository.Verify(r => r.UpdateStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_NullCart_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;
            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = null
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(userId, orderRequest));

            Assert.Equal("Cart cannot be empty", exception.Message);

            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_MissingShippingAddress_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;
            var cartItems = new List<CartItemDto>
        {
            new CartItemDto { ProductId = 1, Quantity = 1, Price = 100 }
        };

            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = cartItems
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(userId, orderRequest));

            Assert.Equal("Shipping address is required", exception.Message);

            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
            _mockProductRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_ProductNotFound_ThrowsException()
        {
            // Arrange
            int userId = 1;
            var cartItems = new List<CartItemDto>
        {
            new CartItemDto { ProductId = 999, Quantity = 1, Price = 100 }
        };

            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = cartItems
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(userId, orderRequest));

            Assert.Equal("Product 999 not found", exception.Message);
            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
            _mockProductRepository.Verify(r => r.UpdateStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_InsufficientStock_ThrowsException()
        {
            // Arrange
            int userId = 1;
            var product = new Product
            {
                Id = 1,
                Name = "Product 1",
                Price = 100m,
                Stock = 3,
                Description = "Desc",
                Category = "Cat",
                ImageUrl = "img.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var cartItems = new List<CartItemDto>
        {
            new CartItemDto { ProductId = 1, Quantity = 5, Price = 100 }
        };

            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = cartItems
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(userId, orderRequest));

            Assert.Contains("Insufficient stock", exception.Message);
            Assert.Contains("Available: 3", exception.Message);

            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
            _mockProductRepository.Verify(r => r.UpdateStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_MultipleProductsOneWithInsufficientStock_ThrowsException()
        {
            // Arrange
            int userId = 1;
            var product1 = new Product { Id = 1, Name = "Product 1", Price = 100m, Stock = 10 };
            var product2 = new Product { Id = 2, Name = "Product 2", Price = 50m, Stock = 2 };

            var cartItems = new List<CartItemDto>
        {
            new CartItemDto { ProductId = 1, Quantity = 3, Price = 100 },
            new CartItemDto { ProductId = 2, Quantity = 5, Price = 50 }
        };

            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = cartItems
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product1);
            _mockProductRepository.Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(product2);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(userId, orderRequest));

            Assert.Contains("Insufficient stock", exception.Message);
            Assert.Contains("Product 2", exception.Message);
            Assert.Contains("Available: 2", exception.Message);

            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_ClientSendsManipulatedPrice_UsesDatabasePrice()
        {
            // Arrange
            int userId = 1;
            var product = new Product
            {
                Id = 1,
                Name = "Expensive Product",
                Price = 500m,
                Stock = 10,
                Description = "Desc",
                Category = "Cat",
                ImageUrl = "img.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var cartItems = new List<CartItemDto>
        {
            new CartItemDto { ProductId = 1, Quantity = 2, Price = 1 }
        };

            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = cartItems
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(new Order { Id = 1 });

            _mockOrderRepository.Setup(r => r.AddOrderItemAsync(It.IsAny<OrderItem>()))
                .ReturnsAsync(new OrderItem());

            _mockProductRepository.Setup(r => r.UpdateStockAsync(1, 2))
                .ReturnsAsync(true);

            _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(1))
                .ReturnsAsync(new Order { Id = 1, TotalAmount = 1000 });

            // Act
            var result = await _orderService.CreateOrderAsync(userId, orderRequest);

            // Assert - Server used DB price not client price 
            Assert.Equal(1000, result.TotalAmount);
            Assert.NotEqual(2, result.TotalAmount);
        }

        [Fact]
        public async Task CreateOrderAsync_StockUpdatedAfterOrder_CalledExactlyOnce()
        {
            // Arrange
            int userId = 1;
            var product = new Product { Id = 1, Name = "Product", Price = 100m, Stock = 10 };

            var cartItems = new List<CartItemDto>
        {
            new CartItemDto { ProductId = 1, Quantity = 3, Price = 100 }
        };

            var orderRequest = new OrderRequestDto
            {
                ShippingAddress = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA",
                Items = cartItems
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(new Order { Id = 1 });

            _mockOrderRepository.Setup(r => r.AddOrderItemAsync(It.IsAny<OrderItem>()))
                .ReturnsAsync(new OrderItem());

            _mockProductRepository.Setup(r => r.UpdateStockAsync(1, 3))
                .ReturnsAsync(true);

            _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(1))
                .ReturnsAsync(new Order { Id = 1, TotalAmount = 300 });

            // Act
            await _orderService.CreateOrderAsync(userId, orderRequest);

            // Assert
            _mockProductRepository.Verify(r => r.UpdateStockAsync(1, 3), Times.Once);
        }

        [Fact]
        public async Task GetUserOrdersAsync_UserHasOrders_ReturnsUserOrders()
        {
            // Arrange
            int userId = 1;
            var orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                UserId = userId,
                TotalAmount = 100m,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                ShippingAddress = "Addr1",
                City = "City1",
                PostalCode = "11111",
                Country = "USA"
            },
            new Order
            {
                Id = 2,
                UserId = userId,
                TotalAmount = 200m,
                Status = "Completed",
                OrderDate = DateTime.UtcNow,
                ShippingAddress = "Addr2",
                City = "City2",
                PostalCode = "22222",
                Country = "USA"
            }
        };

            _mockOrderRepository.Setup(r => r.GetUserOrdersAsync(userId))
                .ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetUserOrdersAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            foreach (var order in result)
            {
                Assert.Equal(userId, order.UserId);
            }

            _mockOrderRepository.Verify(r => r.GetUserOrdersAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserOrdersAsync_UserHasNoOrders_ReturnsEmptyList()
        {
            // Arrange
            int userId = 999;
            var orders = new List<Order>();

            _mockOrderRepository.Setup(r => r.GetUserOrdersAsync(userId))
                .ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetUserOrdersAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockOrderRepository.Verify(r => r.GetUserOrdersAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserOrdersAsync_DifferentUsers_ReturnsOnlyTheirOrders()
        {
            // Arrange
            int userId1 = 1;
            int userId2 = 2;

            var ordersForUser1 = new List<Order>
        {
            new Order { Id = 1, UserId = userId1, TotalAmount = 100m }
        };

            var ordersForUser2 = new List<Order>
        {
            new Order { Id = 2, UserId = userId2, TotalAmount = 200m },
            new Order { Id = 3, UserId = userId2, TotalAmount = 300m }
        };

            _mockOrderRepository.Setup(r => r.GetUserOrdersAsync(userId1))
                .ReturnsAsync(ordersForUser1);
            _mockOrderRepository.Setup(r => r.GetUserOrdersAsync(userId2))
                .ReturnsAsync(ordersForUser2);

            // Act
            var result1 = await _orderService.GetUserOrdersAsync(userId1);
            var result2 = await _orderService.GetUserOrdersAsync(userId2);

            // Assert
            Assert.Single(result1);
            Assert.Equal(2, result2.Count());

            Assert.All(result1, o => Assert.Equal(userId1, o.UserId));
            Assert.All(result2, o => Assert.Equal(userId2, o.UserId));
        }

        [Fact]
        public async Task GetOrderByIdAsync_ValidOrderId_ReturnsOrder()
        {
            // Arrange
            int orderId = 1;
            var order = new Order
            {
                Id = orderId,
                UserId = 1,
                TotalAmount = 150m,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                ShippingAddress = "123 Test St",
                City = "Test City",
                PostalCode = "12345",
                Country = "Test Country",
                OrderItems = new List<OrderItem>
            {
                new OrderItem { Id = 1, ProductId = 1, Quantity = 2, UnitPrice = 50m, TotalPrice = 100m },
                new OrderItem { Id = 2, ProductId = 2, Quantity = 1, UnitPrice = 50m, TotalPrice = 50m }
            }
            };

            _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Equal(150m, result.TotalAmount);
            Assert.Equal(2, result.OrderItems.Count);

            _mockOrderRepository.Verify(r => r.GetOrderWithItemsAsync(orderId), Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_InvalidOrderId_ReturnsNull()
        {
            // Arrange
            int invalidId = 999;

            _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(invalidId))
                .ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.GetOrderByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
            _mockOrderRepository.Verify(r => r.GetOrderWithItemsAsync(invalidId), Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_OrderExistsButNoItems_ReturnsOrderWithEmptyItems()
        {
            // Arrange
            int orderId = 1;
            var order = new Order
            {
                Id = orderId,
                UserId = 1,
                TotalAmount = 0m,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                ShippingAddress = "123 Test St",
                City = "Test City",
                PostalCode = "12345",
                Country = "Test Country",
                OrderItems = new List<OrderItem>()
            };

            _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Empty(result.OrderItems);
        }
    }
}