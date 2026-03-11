using Microsoft.AspNetCore.Mvc;
using AidenApi.Models;
using System.Text;
using System.Text.Json;

namespace AidenApi.Controllers;

/// <summary>
/// Provides copilot-assisted gap analysis and preflight readiness checks for proposals.
/// Forwards to the engine's copilot module which inspects the proposal against active policy rules.
/// </summary>
[ApiController]
[Route("api/proposals")]
[Produces("application/json")]
public class CopilotController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CopilotController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public CopilotController(IHttpClientFactory httpClientFactory, ILogger<CopilotController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get copilot gap analysis for a proposal.
    /// Returns all policy gaps with severity, affected field paths, remediation hints,
    /// and suggested values to help the submitter address issues before formal review.
    /// </summary>
    [HttpPost("{id}/copilot/gaps")]
    [ProducesResponseType(typeof(CopilotGapResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetGaps([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "id is required" });

        var client = _httpClientFactory.CreateClient("engine");

        try
        {
            var response = await client.PostAsync(
                $"/proposals/{Uri.EscapeDataString(id)}/copilot/gaps",
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
            _logger.LogError(ex, "Engine unreachable at POST /proposals/{Id}/copilot/gaps", id);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Engine unavailable", detail = ex.Message });
        }
    }

    /// <summary>
    /// Run a preflight readiness check for a proposal.
    /// Checks that all critical rules have non-null evidence, MinimalEvidence thresholds
    /// are met, and the proposal is ready for formal gate review.
    /// </summary>
    [HttpPost("{id}/copilot/preflight")]
    [ProducesResponseType(typeof(PreflightResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Preflight([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "id is required" });

        var client = _httpClientFactory.CreateClient("engine");

        try
        {
            var response = await client.PostAsync(
                $"/proposals/{Uri.EscapeDataString(id)}/copilot/preflight",
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
            _logger.LogError(ex, "Engine unreachable at POST /proposals/{Id}/copilot/preflight", id);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Engine unavailable", detail = ex.Message });
        }
    }
}
