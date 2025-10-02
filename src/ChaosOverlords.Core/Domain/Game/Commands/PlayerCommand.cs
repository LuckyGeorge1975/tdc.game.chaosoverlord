using System;
using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Game.Commands;

/// <summary>
/// Identifies the high-level kind of command a player can queue for a gang during the command phase.
/// </summary>
public enum PlayerCommandKind
{
    Move,
    Influence,
    Control,
    Chaos
}

/// <summary>
/// Base representation for a queued player command. Specific command types derive from this type and provide
/// additional payload required to resolve the action during execution.
/// </summary>
public abstract record PlayerCommand(
    Guid CommandId,
    Guid PlayerId,
    Guid GangId,
    int TurnNumber)
{
    /// <summary>
    /// High-level command category.
    /// </summary>
    public abstract PlayerCommandKind Kind { get; }

    /// <summary>
    /// Command sub-phase in which this command is resolved.
    /// </summary>
    public abstract CommandPhase Phase { get; }
}

/// <summary>
/// Command directing a gang to relocate to an adjacent sector during the movement sub-phase.
/// </summary>
public sealed record MoveCommand(
    Guid CommandId,
    Guid PlayerId,
    Guid GangId,
    int TurnNumber,
    string SourceSectorId,
    string TargetSectorId) : PlayerCommand(CommandId, PlayerId, GangId, TurnNumber)
{
    public override PlayerCommandKind Kind => PlayerCommandKind.Move;

    public override CommandPhase Phase => CommandPhase.Movement;
}

/// <summary>
/// Command attempting to influence a site within a controlled sector during the instant sub-phase.
/// </summary>
public sealed record InfluenceCommand(
    Guid CommandId,
    Guid PlayerId,
    Guid GangId,
    int TurnNumber,
    string SectorId) : PlayerCommand(CommandId, PlayerId, GangId, TurnNumber)
{
    public override PlayerCommandKind Kind => PlayerCommandKind.Influence;

    public override CommandPhase Phase => CommandPhase.Instant;
}

/// <summary>
/// Command attempting to take control of a sector during the control sub-phase.
/// </summary>
public sealed record ControlCommand(
    Guid CommandId,
    Guid PlayerId,
    Guid GangId,
    int TurnNumber,
    string SectorId) : PlayerCommand(CommandId, PlayerId, GangId, TurnNumber)
{
    public override PlayerCommandKind Kind => PlayerCommandKind.Control;

    public override CommandPhase Phase => CommandPhase.Control;
}

/// <summary>
/// Command projecting chaos activity in a sector. In Phase 2 this only records a deterministic projection value.
/// </summary>
public sealed record ChaosCommand(
    Guid CommandId,
    Guid PlayerId,
    Guid GangId,
    int TurnNumber,
    string SectorId,
    int ProjectedChaos) : PlayerCommand(CommandId, PlayerId, GangId, TurnNumber)
{
    public override PlayerCommandKind Kind => PlayerCommandKind.Chaos;

    public override CommandPhase Phase => CommandPhase.Chaos;
}
