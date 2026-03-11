using Microsoft.AspNetCore.Mvc;
using AidenApi.Models;
using System.Text;
using System.Text.Json;

namespace AidenApi.Controllers;

/// <summary>
/// Manages proposal lifecycle — create, retrieve, and trigger state transitions.
/// All business logic lives in the TypeScript engine; this controller is a thin HTTP forwarder.
/// </summary>
[ApiController]
[Route("api/proposals")]
[Produces("application/json")]
public class ProposalController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProposalController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ProposalController(IHttpClientFactory httpClientFactory, ILogger<ProposalController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Create a new proposal by submitting YAML content.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Proposal), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreateProposal([FromBody] CreateProposalRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.YamlContent))
            return BadRequest(new { error = "yamlContent is required" });

        var client = _httpClientFactory.CreateClient("engine");
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("/proposals", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Engine returned {StatusCode} for POST /proposals: {Body}",
                    (int)response.StatusCode, body);
                return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(body));
            }

            return StatusCode(StatusCodes.Status201Created,
                JsonSerializer.Deserialize<object>(body, JsonOptions));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Engine unreachable at POST /proposals");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Engine unavailable", detail = ex.Message });
        }
    }

    /// <summary>Get a proposal by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Proposal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetProposal([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "id is required" });

        var client = _httpClientFactory.CreateClient("engine");

        try
        {
            var response = await client.GetAsync($"/proposals/{Uri.EscapeDataString(id)}");
            var body = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound(new { error = $"Proposal '{id}' not found" });

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(body));

            return Ok(JsonSerializer.Deserialize<object>(body, JsonOptions));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Engine unreachable at GET /proposals/{Id}", id);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Engine unavailable", detail = ex.Message });
        }
    }

    /// <summary>Trigger a lifecycle state transition for a proposal.</summary>
    [HttpPost("{id}/transition")]
    [ProducesResponseType(typeof(LifecycleState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Transition(
        [FromRoute] string id,
        [FromBody] TransitionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ToState))
            return BadRequest(new { error = "toState is required" });
        if (string.IsNullOrWhiteSpace(request.TriggeredBy))
            return BadRequest(new { error = "triggeredBy is required" });

        var client = _httpClientFactory.CreateClient("engine");
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(
                $"/proposals/{Uri.EscapeDataString(id)}/transition", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(body));

            return Ok(JsonSerializer.Deserialize<object>(body, JsonOptions));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Engine unreachable at POST /proposals/{Id}/transition", id);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Engine unavailable", detail = ex.Message });
        }
    }
}
