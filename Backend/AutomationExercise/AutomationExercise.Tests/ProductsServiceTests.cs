using AutomationExercise.API.Services;
using AutomationExercise.Core.Interfaces;
using AutomationExercise.Core.Models;
using Moq;

namespace AutomationExercise.Tests.Services
{

    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _productService = new ProductService(_mockProductRepository.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ProductsExist_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Description = "Desc 1", Price = 100, Category = "Cat1", ImageUrl = "img1.jpg", Stock = 10, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Product 2", Description = "Desc 2", Price = 200, Category = "Cat2", ImageUrl = "img2.jpg", Stock = 5, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "Product 3", Description = "Desc 3", Price = 300, Category = "Cat1", ImageUrl = "img3.jpg", Stock = 0, CreatedAt = DateTime.UtcNow }
        };

            _mockProductRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());

            var resultList = result.ToList();
            Assert.Equal(1, resultList[0].Id);
            Assert.Equal("Product 1", resultList[0].Name);
            Assert.Equal(100, resultList[0].Price);
            Assert.Equal(10, resultList[0].Stock);

            Assert.Equal(2, resultList[1].Id);
            Assert.Equal(3, resultList[2].Id);

            _mockProductRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_NoProducts_ReturnsEmptyList()
        {
            // Arrange
            var products = new List<Product>();

            _mockProductRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockProductRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_ValidId_ReturnsProduct()
        {
            // Arrange
            int productId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                Category = "Electronics",
                ImageUrl = "test.jpg",
                Stock = 15,
                CreatedAt = DateTime.UtcNow
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal("Test Description", result.Description);
            Assert.Equal(99.99m, result.Price);
            Assert.Equal("Electronics", result.Category);
            Assert.Equal("test.jpg", result.ImageUrl);
            Assert.Equal(15, result.Stock);

            _mockProductRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            int invalidId = 999;

            _mockProductRepository.Setup(r => r.GetByIdAsync(invalidId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(r => r.GetByIdAsync(invalidId), Times.Once);
        }

        [Fact]
        public async Task ProductDto_MapsCorrectly_FromProduct()
        {
            // Arrange
            int productId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Mapping Test",
                Description = "Testing mapping",
                Price = 75.50m,
                Category = "Books",
                ImageUrl = "book.jpg",
                Stock = 25,
                CreatedAt = DateTime.UtcNow
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(product.Description, result.Description);
            Assert.Equal(product.Price, result.Price);
            Assert.Equal(product.Category, result.Category);
            Assert.Equal(product.ImageUrl, result.ImageUrl);
            Assert.Equal(product.Stock, result.Stock);
        }
    }
}