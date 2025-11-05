using Reward.Models;
using Reward.BusinessLogic;
using Microsoft.AspNetCore.Mvc;

namespace Reward.Web.Controllers;

[ApiController]
[Route("api/[controller]")] // Sets the base route to /api/transaction
public class TransactionController : ControllerBase
{
    private readonly ICalculationService _calculator;
    private readonly ILogger<TransactionController> _logger;

    // The CalculationService and ILogger are injected via the constructor
    public TransactionController(ICalculationService calculator, ILogger<TransactionController> logger)
    {
        _calculator = calculator;
        _logger = logger;
    }

    /// <summary>
    /// Calculates the total amount, applied discount, and earned loyalty points for a shopping basket.
    /// The full route for this endpoint is POST /api/transaction/calculate
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Calculate([FromBody] TransactionRequest request)
    {
        // Basic request validation
        if (string.IsNullOrEmpty(request.CustomerId) || string.IsNullOrEmpty(request.LoyaltyCard) || request.Basket == null || !request.Basket.Any())
        {
            return BadRequest(new { Error = "Invalid request format. Missing CustomerId, LoyaltyCard, or non-empty Basket." });
        }

        try
        {
            // Execute the main calculation logic
            var response = await _calculator.CalculateBasketAsync(request);

            // Returns Status200OK with the TransactionResponse object
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            // Handle specific business logic validation errors (like date format issues)
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            // Handle unexpected errors gracefully and log the full exception
            _logger.LogError(ex, "An internal error occurred during calculation.");
            // Use Problem() for a standard, machine-readable HTTP 500 response body
            return Problem("An internal error occurred during calculation.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
