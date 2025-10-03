using System.Collections.ObjectModel;

namespace ChaosOverlords.Core.Domain.Game.Actions;

/// <summary>
///     Describes the immutable inputs that define an action being resolved.
/// </summary>
public sealed class ActionContext
{
    private readonly ReadOnlyCollection<ActionModifier> _modifiers;

    public ActionContext(
        int turnNumber,
        Guid actorId,
        string actorName,
        string actionName,
        TurnPhase phase,
        CommandPhase? commandPhase,
        ActionDifficulty difficulty,
        IEnumerable<ActionModifier>? modifiers = null,
        string? targetId = null,
        string? targetName = null)
    {
        if (turnNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber,
                "Turn number must be greater than zero.");

        if (actorId == Guid.Empty) throw new ArgumentException("Actor id must be provided.", nameof(actorId));

        if (string.IsNullOrWhiteSpace(actorName))
            throw new ArgumentException("Actor name must be provided.", nameof(actorName));

        if (string.IsNullOrWhiteSpace(actionName))
            throw new ArgumentException("Action name must be provided.", nameof(actionName));

        Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));

        TurnNumber = turnNumber;
        ActorId = actorId;
        ActorName = actorName;
        ActionName = actionName;
        Phase = phase;
        CommandPhase = commandPhase;
        TargetId = string.IsNullOrWhiteSpace(targetId) ? null : targetId;
        TargetName = targetName;

        var materialised = modifiers?.ToArray() ?? Array.Empty<ActionModifier>();
        _modifiers = new ReadOnlyCollection<ActionModifier>(materialised);
    }

    /// <summary>
    ///     Turn number during which the action is resolved.
    /// </summary>
    public int TurnNumber { get; }

    /// <summary>
    ///     Identifier of the acting entity.
    /// </summary>
    public Guid ActorId { get; }

    /// <summary>
    ///     Display name of the acting entity.
    /// </summary>
    public string ActorName { get; }

    /// <summary>
    ///     Name of the action being performed.
    /// </summary>
    public string ActionName { get; }

    /// <summary>
    ///     Optional target identifier (free-form, e.g. sector id).
    /// </summary>
    public string? TargetId { get; }

    /// <summary>
    ///     Optional human readable target name.
    /// </summary>
    public string? TargetName { get; }

    /// <summary>
    ///     Turn phase in which the action is resolved.
    /// </summary>
    public TurnPhase Phase { get; }

    /// <summary>
    ///     Command sub-phase linked to the action.
    /// </summary>
    public CommandPhase? CommandPhase { get; }

    /// <summary>
    ///     Structural difficulty configuration used during resolution.
    /// </summary>
    public ActionDifficulty Difficulty { get; }

    /// <summary>
    ///     Modifiers applied to the action.
    /// </summary>
    public IReadOnlyList<ActionModifier> Modifiers => _modifiers;
}