// ORApp.API/Controllers/AuthController.cs (Assuming it exists in the repo structure)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // If needed for other endpoints
using ORApp.API.Services;
using ORApp.Shared.Models;
using System.ComponentModel.DataAnnotations; // For validation attributes if used

namespace ORApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Standard API route
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            // ModelState validation is crucial
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new AuthResponseDto { Success = false, Errors = errors });
            }

            var result = await _authService.RegisterAsync(request);
            if (result.Success)
            {
                // Return the token and user info upon successful registration
                return Ok(result);
            }
            else
            {
                // Return errors if registration fails (e.g., duplicate email)
                return BadRequest(result);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            // ModelState validation for login
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new AuthResponseDto { Success = false, Errors = errors });
            }

            var result = await _authService.LoginAsync(request);
            if (result.Success)
            {
                // Return the token and user info upon successful login
                return Ok(result);
            }
            else
            {
                // Return 401 Unauthorized if credentials are invalid
                return Unauthorized(result);
            }
        }

        // Logout endpoint is typically client-side (clearing JWT), but API can handle refresh token invalidation if used.
        // For JWT-only, it's often a no-op on the server.
        [HttpPost("logout")]
        [Authorize] // Requires a valid JWT
        public async Task<ActionResult> Logout()
        {
            // For stateless JWT, the server doesn't store the token.
            // The client is responsible for clearing it locally.
            // If using refresh tokens, this endpoint would invalidate the server-side refresh token.
            // For MVP with JWT only, simply return Ok.
            return Ok(new { message = "Logged out successfully (client-side token clearing required)" });
        }
    }
}