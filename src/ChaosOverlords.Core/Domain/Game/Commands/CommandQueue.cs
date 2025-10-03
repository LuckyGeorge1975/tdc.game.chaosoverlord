namespace ChaosOverlords.Core.Domain.Game.Commands;

/// <summary>
///     Describes the outcome of mutating a command queue.
/// </summary>
public enum CommandQueueMutationStatus
{
    Added,
    Replaced,
    Removed,
    NotFound
}

/// <summary>
///     Result metadata produced when a queue mutation occurs.
/// </summary>
public sealed record CommandQueueMutationResult(
    CommandQueueMutationStatus Status,
    PlayerCommand? Command,
    PlayerCommand? ReplacedCommand);

/// <summary>
///     Maintains the set of commands queued by a single player for the current turn.
/// </summary>
public sealed class CommandQueue
{
    private readonly Dictionary<Guid, PlayerCommand> _commandsByGang = new();
    private readonly List<PlayerCommand> _orderedCommands = new();

    public CommandQueue(Guid playerId)
    {
        PlayerId = playerId;
    }

    public Guid PlayerId { get; }

    public IReadOnlyList<PlayerCommand> Commands => _orderedCommands.AsReadOnly();

    public bool HasCommand(Guid gangId)
    {
        return _commandsByGang.ContainsKey(gangId);
    }

    public CommandQueueMutationResult SetCommand(PlayerCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        if (command.PlayerId != PlayerId)
            throw new InvalidOperationException("Command player id does not match queue owner.");

        if (_commandsByGang.TryGetValue(command.GangId, out var existing))
        {
            Replace(existing, command);
            return new CommandQueueMutationResult(CommandQueueMutationStatus.Replaced, command, existing);
        }

        Add(command);
        return new CommandQueueMutationResult(CommandQueueMutationStatus.Added, command, null);
    }

    public CommandQueueMutationResult Remove(Guid gangId)
    {
        if (!_commandsByGang.TryGetValue(gangId, out var existing))
            return new CommandQueueMutationResult(CommandQueueMutationStatus.NotFound, null, null);

        _commandsByGang.Remove(gangId);
        _orderedCommands.Remove(existing);
        return new CommandQueueMutationResult(CommandQueueMutationStatus.Removed, null, existing);
    }

    public void Clear()
    {
        _commandsByGang.Clear();
        _orderedCommands.Clear();
    }

    public IReadOnlyList<PlayerCommand> GetCommandsForPhase(CommandPhase phase)
    {
        if (_orderedCommands.Count == 0) return Array.Empty<PlayerCommand>();

        return _orderedCommands.Where(command => command.Phase == phase).ToList();
    }

    private void Add(PlayerCommand command)
    {
        _commandsByGang[command.GangId] = command;
        _orderedCommands.Add(command);
    }

    private void Replace(PlayerCommand existing, PlayerCommand replacement)
    {
        _commandsByGang[replacement.GangId] = replacement;
        var index = _orderedCommands.IndexOf(existing);
        if (index >= 0)
            _orderedCommands[index] = replacement;
        else
            _orderedCommands.Add(replacement);
    }
}