namespace AidenApi.Models;

/// <summary>
/// Represents an AI system proposal submitted for AIDEN review.
/// </summary>
public record Proposal
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Team { get; init; } = string.Empty;
    public string SubmittedBy { get; init; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; init; }

    /// <summary>Raw YAML content of the proposal (base64 or plain text)</summary>
    public string? YamlContent { get; init; }

    /// <summary>Structured scope metadata extracted from YAML</summary>
    public ProposalScope? Scope { get; init; }
}

public record ProposalScope
{
    public string Tier { get; init; } = "internal-tool";
    public bool Phi { get; init; }
    public string? DeploymentBoundary { get; init; }
    public string? ModelBoundary { get; init; }
    public string? UserCohort { get; init; }
    public string? TimeWindow { get; init; }
    public string? PlatformVersion { get; init; }
}

/// <summary>
/// Request body for creating a new proposal via POST /api/proposals.
/// </summary>
public record CreateProposalRequest
{
    /// <summary>Full YAML content of the proposal</summary>
    public string YamlContent { get; init; } = string.Empty;
}

/// <summary>
/// Request body for lifecycle transitions via POST /api/proposals/{id}/transition.
/// </summary>
public record TransitionRequest
{
    public string ToState { get; init; } = string.Empty;
    public string TriggeredBy { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
