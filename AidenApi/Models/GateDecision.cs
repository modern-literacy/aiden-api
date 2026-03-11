namespace AidenApi.Models;

/// <summary>
/// The gate decision output from the AIDEN engine for a reviewed proposal.
/// </summary>
public record GateDecision
{
    public string Outcome { get; init; } = string.Empty;  // APPROVE | CONDITIONAL | BLOCK
    public IReadOnlyList<string> Rationale { get; init; } = [];
    public bool RequiresHuman { get; init; } = true;
    public AutonomyBudget AutonomyBudget { get; init; } = new();
    public bool EscalationRequired { get; init; }
    public IReadOnlyList<string> OverrideRequires { get; init; } = [];
    public string Tier { get; init; } = string.Empty;
    public IReadOnlyList<string> WaiversApplied { get; init; } = [];
    public DateTimeOffset Timestamp { get; init; }
}

public record AutonomyBudget
{
    public int Consumed { get; init; }
    public int Remaining { get; init; }
    public int Ceiling { get; init; }
}

/// <summary>
/// Full review result returned from the engine, including assurance profiles per domain.
/// </summary>
public record ReviewResult
{
    public string ProposalId { get; init; } = string.Empty;
    public GateDecision? GateDecision { get; init; }
    public IReadOnlyList<SectionProfile> SectionProfiles { get; init; } = [];
    public double? OverallREff { get; init; }
    public string? OverallFormality { get; init; }
    public DateTimeOffset ReviewedAt { get; init; }
}

/// <summary>
/// Per-domain assurance profile from the scorer.
/// </summary>
public record SectionProfile
{
    public string Domain { get; init; } = string.Empty;
    public string Formality { get; init; } = string.Empty;
    public double RuleCoverage { get; init; }
    public double? REff { get; init; }
    public RuleTallies Tallies { get; init; } = new();
    public int CriticalFails { get; init; }
    public int HighFails { get; init; }
}

public record RuleTallies
{
    public int Passed { get; init; }
    public int Failed { get; init; }
    public int Abstained { get; init; }
    public int Degraded { get; init; }
    public int Waived { get; init; }
}

/// <summary>
/// Copilot gap analysis result.
/// </summary>
public record CopilotGapResult
{
    public string ProposalId { get; init; } = string.Empty;
    public IReadOnlyList<PolicyGap> Gaps { get; init; } = [];
    public int CriticalGapCount { get; init; }
    public int TotalGapCount { get; init; }
}

public record PolicyGap
{
    public string RuleId { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string FieldPath { get; init; } = string.Empty;
    public string RemediationHint { get; init; } = string.Empty;
    public object? SuggestedValue { get; init; }
}

/// <summary>
/// Preflight readiness check result.
/// </summary>
public record PreflightResult
{
    public string ProposalId { get; init; } = string.Empty;
    public bool Ready { get; init; }
    public IReadOnlyList<string> BlockingIssues { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
