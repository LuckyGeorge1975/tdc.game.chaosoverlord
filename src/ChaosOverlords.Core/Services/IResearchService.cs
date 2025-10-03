using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Research service contracts for previewing and executing research as an Instant command.
///     Initial implementation is a stub to establish DI and tests without changing gameplay yet.
/// </summary>
public interface IResearchService
{
    ResearchPreview BuildPreview(GameState state, Guid playerId);

    ResearchActionResult Execute(GameState state, Guid playerId, string projectId, int turnNumber);
}

public readonly record struct ResearchPreview(
    string? ActiveProjectId,
    int EstimatedProgress,
    int EstimatedCost);

public enum ResearchActionStatus
{
    NotImplemented = 0,
    Success = 1,
    Failed = 2
}

public sealed record ResearchActionResult(
    ResearchActionStatus Status,
    string Message,
    string? ProjectId,
    int ProgressApplied,
    int CostApplied);