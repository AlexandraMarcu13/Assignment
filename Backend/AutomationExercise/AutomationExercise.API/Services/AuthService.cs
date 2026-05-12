using AutomationExercise.Core.DTOs;
using AutomationExercise.Core.Interfaces;
using AutomationExercise.Core.Models;
using AutomationExercise.Data.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AutomationExercise.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _tokenExpiryDays;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");

            if (jwtKey.Length < 32)
                throw new InvalidOperationException("JWT Key must be at least 32 characters");

            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            _issuer = _configuration["Jwt:Issuer"] ?? "AutomationExercise";
            _audience = _configuration["Jwt:Audience"] ?? "AutomationExerciseClient";
            _tokenExpiryDays = int.TryParse(_configuration["Jwt:ExpiryDays"], out var days) ? days : 7;
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            ValidateRegistrationInput(registerDto);

            var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            var user = MapToUserEntity(registerDto);
            user.PasswordHash = HashPassword(registerDto.Password);

            var newUser = await _userRepository.AddAsync(user);

            var token = GenerateJwtToken(newUser.Id, newUser.Email);

            return MapToUserResponseDto(newUser, token);
        }

        public async Task<UserResponseDto> LoginAsync(LoginDto loginDto)
        {
            ValidateLoginInput(loginDto);

            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Account is deactivated");
            }

            var token = GenerateJwtToken(user.Id, user.Email);
            return MapToUserResponseDto(user, token);
        }

        public string GenerateJwtToken(int userId, string email)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("userId", userId.ToString())
            };

            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_tokenExpiryDays),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Private Helper Methods

        private void ValidateRegistrationInput(RegisterDto registerDto)
        {
            if (registerDto == null)
                throw new ArgumentNullException(nameof(registerDto));

            if (string.IsNullOrWhiteSpace(registerDto.Email))
                throw new InvalidOperationException("Email is required");

            if (!IsValidEmail(registerDto.Email))
                throw new InvalidOperationException("Invalid email format");

            if (string.IsNullOrWhiteSpace(registerDto.Password))
                throw new InvalidOperationException("Password is required");

            if (registerDto.Password.Length < 6)
                throw new InvalidOperationException("Password must be at least 6 characters");

            if (registerDto.Password.Length > 100)
                throw new InvalidOperationException("Password must not exceed 100 characters");

            if (string.IsNullOrWhiteSpace(registerDto.Username))
                throw new InvalidOperationException("Username is required");

            if (registerDto.Username.Length < 3)
                throw new InvalidOperationException("Username must be at least 3 characters");

            if (registerDto.Username.Length > 50)
                throw new InvalidOperationException("Username must not exceed 50 characters");

            if (!string.IsNullOrWhiteSpace(registerDto.FirstName) && registerDto.FirstName.Length > 100)
                throw new InvalidOperationException("First name must not exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(registerDto.LastName) && registerDto.LastName.Length > 100)
                throw new InvalidOperationException("Last name must not exceed 100 characters");
        }

        private void ValidateLoginInput(LoginDto loginDto)
        {
            if (loginDto == null)
                throw new ArgumentNullException(nameof(loginDto));

            if (string.IsNullOrWhiteSpace(loginDto.Email))
                throw new UnauthorizedAccessException("Email is required");

            if (string.IsNullOrWhiteSpace(loginDto.Password))
                throw new UnauthorizedAccessException("Password is required");
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        private static User MapToUserEntity(RegisterDto dto)
        {
            return new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                FirstName = dto.FirstName?.Trim() ?? string.Empty,
                LastName = dto.LastName?.Trim() ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static UserResponseDto MapToUserResponseDto(User user, string token)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token
            };
        }

        private static string HashPassword(string password)
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

        private static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var combined = Convert.FromBase64String(storedHash);

                var salt = new byte[32];
                var hash = new byte[32];

                Buffer.BlockCopy(combined, 0, salt, 0, 32);
                Buffer.BlockCopy(combined, 32, hash, 0, 32);

                using var deriveBytes = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations: 100000,
                    HashAlgorithmName.SHA256
                );

                var computedHash = deriveBytes.GetBytes(32);

                return CryptographicOperations.FixedTimeEquals(hash, computedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}