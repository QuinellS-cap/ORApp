// ORApp.API/Services/AuthService.cs (Assuming it exists in the repo structure)

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ORApp.API.Models;
using ORApp.Data; // For User entity
using ORApp.Data.Context;
using ORApp.Shared.Models; // For DTOs
using System.IdentityModel.Tokens.Jwt; // For JWT generation
using System.Security.Claims; // For JWT claims
using System.Text; // For encoding the key

namespace ORApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ORDBContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IPasswordHasherService _passwordHasher;

        public AuthService(ORDBContext context, IOptions<JwtSettings> jwtSettings, IPasswordHasherService passwordHasher)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Email address is already registered." }
                };
            }

            // Create new user entity
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber, // Optional field
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                CreatedDate = DateTime.UtcNow, // Set creation date
                UpdatedDate = DateTime.UtcNow  // Set update date
            };

            // Add and save to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Map user entity to DTO
            var userDto = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };

            // Return success response with token and user info
            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                User = userDto
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // Verify user exists and password is correct
            if (user != null && _passwordHasher.VerifyHashedPassword(user.PasswordHash, request.Password))
            {
                // Generate JWT token
                var token = GenerateJwtToken(user);

                // Map user entity to DTO
                var userDto = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    CreatedDate = user.CreatedDate,
                    UpdatedDate = user.UpdatedDate
                };

                // Return success response with token and user info
                return new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    User = userDto
                };
            }

            // Return failure response if user not found or password incorrect
            return new AuthResponseDto
            {
                Success = false,
                Errors = new List<string> { "Invalid email or password." }
            };
        }

        public string GenerateJwtToken(User user) // Takes the Data.User entity
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()), // Use the integer Id
                    new Claim(ClaimTypes.Name, user.Email), // Name claim
                    new Claim(ClaimTypes.Email, user.Email) // Email claim
                    // Add other claims if needed
                }),
                // Set token expiration (e.g., 24 hours for MVP)
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // Return the serialized token string
        }
    }
}