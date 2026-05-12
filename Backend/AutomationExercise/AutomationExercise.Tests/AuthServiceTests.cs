using AutomationExercise.API.Services;
using AutomationExercise.Core.DTOs;
using AutomationExercise.Core.Interfaces;
using AutomationExercise.Core.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace AutomationExercise.Tests.Services
{

    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("test-secret-key-with-at-least-32-characters");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(c => c["Jwt:ExpiryDays"]).Returns("7");

            _authService = new AuthService(_mockUserRepository.Object, _mockConfiguration.Object);
        }


        [Fact]
        public async Task RegisterAsync_ValidUser_ReturnsUserResponseDto()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            var newUser = new User
            {
                Id = 1,
                Email = registerDto.Email,
                Username = registerDto.Username,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(newUser);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(registerDto.Email, result.Email);
            Assert.Equal(registerDto.Username, result.Username);
            Assert.Equal(registerDto.FirstName, result.FirstName);
            Assert.Equal(registerDto.LastName, result.LastName);
            Assert.NotNull(result.Token);
            Assert.True(result.Token.Length > 0);

            _mockUserRepository.Verify(r => r.GetByEmailAsync(registerDto.Email), Times.Once);
            _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ExistingEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "existing@example.com",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            var existingUser = new User { Id = 1, Email = registerDto.Email };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerDto));

            Assert.Equal("User with this email already exists", exception.Message);
            _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _authService.RegisterAsync(null!));

            Assert.Contains("registerDto", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_EmptyEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerDto));

            Assert.Equal("Email is required", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_InvalidEmailFormat_ThrowsInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "invalid-email",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerDto));

            Assert.Equal("Invalid email format", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_EmptyPassword_ThrowsInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "",
                FirstName = "Test",
                LastName = "User"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerDto));

            Assert.Equal("Password is required", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_PasswordLessThan6Chars_ThrowsInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "12345",
                FirstName = "Test",
                LastName = "User"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerDto));

            Assert.Equal("Password must be at least 6 characters", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_EmptyUsername_ThrowsInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "",
                Email = "test@example.com",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerDto));

            Assert.Equal("Username is required", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_UsernameLessThan3Chars_ThrowsInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "ab",
                Email = "test@example.com",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerDto));

            Assert.Equal("Username must be at least 3 characters", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUserResponseDto()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = HashPasswordForTest("Test123!")
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loginDto.Email, result.Email);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("Test", result.FirstName);
            Assert.Equal("User", result.LastName);
            Assert.NotNull(result.Token);
        }

        [Fact]
        public async Task LoginAsync_InvalidEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "Test123!"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginDto));

            Assert.Equal("Invalid email or password", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword!"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                Username = "testuser",
                IsActive = true,
                PasswordHash = HashPasswordForTest("CorrectPassword123!")
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginDto));

            Assert.Equal("Invalid email or password", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_InactiveAccount_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                Username = "testuser",
                IsActive = false,
                PasswordHash = HashPasswordForTest("Test123!")
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginDto));

            Assert.Equal("Account is deactivated", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _authService.LoginAsync(null!));

            Assert.Contains("loginDto", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_EmptyEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "",
                Password = "Test123!"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginDto));

            Assert.Equal("Email is required", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_EmptyPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = ""
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginDto));

            Assert.Equal("Password is required", exception.Message);
        }


        [Fact]
        public void GenerateJwtToken_ValidInputs_ReturnsValidTokenString()
        {
            // Arrange
            int userId = 1;
            string email = "test@example.com";

            // Act
            var token = _authService.GenerateJwtToken(userId, email);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            Assert.Contains(jsonToken.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
            Assert.Contains(jsonToken.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
            Assert.Contains(jsonToken.Claims, c => c.Type == "userId" && c.Value == userId.ToString());
        }

        [Fact]
        public void GenerateJwtToken_DifferentUsers_ReturnsDifferentTokens()
        {
            // Arrange
            int userId1 = 1;
            string email1 = "user1@example.com";

            int userId2 = 2;
            string email2 = "user2@example.com";

            // Act
            var token1 = _authService.GenerateJwtToken(userId1, email1);
            var token2 = _authService.GenerateJwtToken(userId2, email2);

            // Assert
            Assert.NotEqual(token1, token2);
        }


        private static string HashPasswordForTest(string password)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(
                password,
                saltSize: 32,
                iterations: 100000,
                HashAlgorithmName.SHA256
            );

            var salt = deriveBytes.Salt;
            var hash = deriveBytes.GetBytes(32);

            var combined = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);

            return Convert.ToBase64String(combined);
        }
    }
}