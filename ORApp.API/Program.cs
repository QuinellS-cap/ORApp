// ORApp.API/Program.cs

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ORApp.API;
using ORApp.API.Repositories; // If you use repositories
using ORApp.API.Services; // For service interfaces/impl
using ORApp.Data; // For DbContext
using ORApp.Shared.Services; // If needed directly in API
using System.Text; // For Encoding

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Good for development

// Configure DbContext
builder.Services.AddDbContext<OddsRaidersDbContext>(options =>
    options.UseSqlServer( // Or UseNpgsql for Postgres
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
    if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
    {
        throw new InvalidOperationException("JWT settings are missing or invalid in configuration.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

// Configure Authorization (Must come after Authentication config)
builder.Services.AddAuthorization();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();
builder.Services.AddScoped<IFixtureService, FixtureService>();
// Add other services as needed, e.g., repositories if used directly by services
// builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Password Hasher Service (Example using BCrypt.NET-Next)
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Ensure HTTPS in production

// Important: Authentication must be before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Inline definitions for simplicity in this context
// In a real project, these would likely be in separate files.

public interface IPasswordHasherService
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}

public class PasswordHasherService : IPasswordHasherService
{
    public string HashPassword(string password)
    {
        // Use a strong hashing algorithm like BCrypt
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        // Verify the provided password against the hash
        return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
    }
}

namespace ORApp.API
{
    public class JwtSettings
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        // DurationInMinutes might be used for token generation, but often hard-coded in service
    }
}