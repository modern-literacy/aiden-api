namespace AidenApi.Models;

/// <summary>
/// Valid lifecycle states for an AIDEN proposal.
/// States: Draft → NeedsInfo | ReviewReady → Gated → Approved | Waived | Rejected | Escalated → ...
/// </summary>
public enum LifecycleStateValue
{
    Draft,
    NeedsInfo,
    ReviewReady,
    Gated,
    Escalated,
    Approved,
    Waived,
    Rejected,
    Expired
}

/// <summary>
/// Represents the current lifecycle state of a proposal including its full transition history.
/// </summary>
public record LifecycleState
{
    public string ProposalId { get; init; } = string.Empty;
    public string CurrentState { get; init; } = string.Empty;
    public DateTimeOffset StateEnteredAt { get; init; }
    public IReadOnlyList<LifecycleTransition> History { get; init; } = [];
}

/// <summary>
/// A single recorded lifecycle transition.
/// </summary>
public record LifecycleTransition
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public string TriggeredBy { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
