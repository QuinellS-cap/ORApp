// ORApp.API/Controllers/SubscriptionController.cs (Assuming it exists in the repo structure)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Required for endpoints needing auth
using ORApp.API.Services;
using ORApp.Shared.Models;

namespace ORApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Standard API route
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("{userId:int}")] // Ensure userId is an integer route parameter
        [Authorize] // Requires a valid JWT
        public async Task<ActionResult<SubscriptionDto>> GetSubscription(int userId)
        {
            // Security: Ensure the logged-in user can only access their own subscription
            var loggedInUserId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            if (userId != loggedInUserId)
            {
                return Forbid(); // Return 403 Forbidden
            }

            var subscription = await _subscriptionService.GetSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                // Return a default DTO indicating no subscription exists
                // Or return NotFound() if preferred
                return Ok(new SubscriptionDto
                {
                    UserId = userId,
                    IsPaid = false,
                    IsCancelled = true, // Indicate no active subscription
                    StartDate = DateTime.MinValue,
                    EndDate = DateTime.MinValue
                });
            }
            return Ok(subscription);
        }

        [HttpPost]
        [Authorize] // Requires a valid JWT
        public async Task<ActionResult<CreateSubscriptionResponseDto>> CreateSubscription([FromBody] CreateSubscriptionRequestDto request)
        {
            // Security: Ensure the logged-in user can only create a subscription for themselves
            var loggedInUserId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            if (request.UserId != loggedInUserId)
            {
                return Forbid(); // Return 403 Forbidden
            }

            var result = await _subscriptionService.CreateSubscriptionAsync(request);
            if (result.Success)
            {
                // Return the PayFast URL to the client
                return Ok(result);
            }
            else
            {
                // Return errors if creation fails
                return BadRequest(result);
            }
        }

        // Endpoint for PayFast to notify us of payment status
        // This must be publicly accessible
        [HttpPost("payfast-notify")]
        [AllowAnonymous] // Public endpoint for PayFast webhook
        public async Task<IActionResult> PayFastNotify([FromForm] Dictionary<string, string> formData)
        {
            // IMPORTANT: Verify the notification is genuine from PayFast
            // This involves calculating a signature based on the received data and your PayFast secret.
            // A real implementation is crucial for security.
            // For MVP, we'll assume it's valid if basic required fields are present.
            // var isValid = VerifyPayFastSignature(formData); // Implement this method
            // if (!isValid) return BadRequest("Invalid signature from PayFast.");

            // Extract relevant data from the form data
            var paymentId = formData["m_payment_id"]; // This is our internal payment ID sent to PayFast
            var payFastReference = formData["payfast_reference"]; // PayFast's reference for the transaction
            var status = formData["payment_status"]; // e.g., "COMPLETE", "FAILED", "PENDING"

            // Update the payment and subscription status in the database
            await _subscriptionService.UpdatePaymentAndSubscriptionAsync(paymentId, payFastReference, status);

            // PayFast expects an OK response (200) to acknowledge receipt
            return Ok();
        }
    }
}