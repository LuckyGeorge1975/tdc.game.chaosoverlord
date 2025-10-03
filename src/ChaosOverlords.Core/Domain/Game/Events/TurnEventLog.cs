namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
///     Ring-buffer style implementation of <see cref="ITurnEventLog" />.
/// </summary>
public sealed class TurnEventLog : ITurnEventLog
{
    private readonly Queue<TurnEvent> _events = new();

    public TurnEventLog(int capacity = 64)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");

        Capacity = capacity;
    }

    public event EventHandler? EventsChanged;

    public int Capacity { get; }

    public IReadOnlyList<TurnEvent> Events => _events.ToArray();

    public void Append(TurnEvent entry)
    {
        if (entry is null) throw new ArgumentNullException(nameof(entry));

        _events.Enqueue(entry);

        while (_events.Count > Capacity) _events.Dequeue();

        EventsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        if (_events.Count == 0) return;

        _events.Clear();
        EventsChanged?.Invoke(this, EventArgs.Empty);
    }
}