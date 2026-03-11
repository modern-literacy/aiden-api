using Microsoft.AspNetCore.Mvc;
using AidenApi.Models;
using System.Text;
using System.Text.Json;

namespace AidenApi.Controllers;

/// <summary>
/// Triggers and retrieves AIDEN policy review results for a proposal.
/// All evaluation logic lives in the TypeScript engine.
/// </summary>
[ApiController]
[Route("api/proposals")]
[Produces("application/json")]
public class ReviewController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReviewController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ReviewController(IHttpClientFactory httpClientFactory, ILogger<ReviewController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Trigger a full policy review for a proposal.
    /// The engine evaluates all active policy rules for the proposal's tier,
    /// computes R_eff, runs the gate engine, and stores results.
    /// </summary>
    [HttpPost("{id}/review")]
    [ProducesResponseType(typeof(ReviewResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> TriggerReview([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "id is required" });

        var client = _httpClientFactory.CreateClient("engine");

        try
        {
            // POST with empty body — engine uses stored proposal YAML
            var response = await client.PostAsync(
                $"/proposals/{Uri.EscapeDataString(id)}/review",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            var body = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound(new { error = $"Proposal '{id}' not found" });

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(body));

            return Ok(JsonSerializer.Deserialize<object>(body, JsonOptions));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Engine unreachable at POST /proposals/{Id}/review", id);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Engine unavailable — gate decision folded to BLOCK", detail = ex.Message });
        }
    }

    /// <summary>
    /// Retrieve the most recent review result for a proposal.
    /// Returns the gate decision, per-section assurance profiles, and report artifacts.
    /// </summary>
    [HttpGet("{id}/review")]
    [ProducesResponseType(typeof(ReviewResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetReview([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "id is required" });

        var client = _httpClientFactory.CreateClient("engine");

        try
        {
            var response = await client.GetAsync($"/proposals/{Uri.EscapeDataString(id)}/review");
            var body = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound(new { error = $"No review found for proposal '{id}'" });

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(body));

            return Ok(JsonSerializer.Deserialize<object>(body, JsonOptions));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Engine unreachable at GET /proposals/{Id}/review", id);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Engine unavailable", detail = ex.Message });
        }
    }
}
