using System;
using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
/// Observes the turn controller and records high-level lifecycle events in the log.
/// </summary>
public sealed class TurnEventRecorder : IDisposable
{
    private readonly ITurnController _controller;
    private readonly ITurnEventLog _log;
    private bool _wasActive;
    private TurnPhase _lastPhase;
    private CommandPhase? _lastCommandPhase;
    private int _lastCompletedTurn;
    private long _sequenceNumber;

    public TurnEventRecorder(ITurnController controller, ITurnEventLog log)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _log = log ?? throw new ArgumentNullException(nameof(log));

        _controller.StateChanged += OnStateChanged;
        _controller.TurnCompleted += OnTurnCompleted;

        _wasActive = _controller.IsTurnActive;
        _lastPhase = _controller.CurrentPhase;
        _lastCommandPhase = _controller.ActiveCommandPhase;
        _lastCompletedTurn = _controller.TurnNumber - 1;
    }

    public void Dispose()
    {
        _controller.StateChanged -= OnStateChanged;
        _controller.TurnCompleted -= OnTurnCompleted;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (_controller.IsTurnActive && !_wasActive)
        {
            _log.Append(CreateEvent(_controller.TurnNumber, _controller.CurrentPhase, _controller.ActiveCommandPhase, TurnEventType.TurnStarted, "Turn started."));
        }

        if (_controller.IsTurnActive)
        {
            if (_controller.CurrentPhase != _lastPhase)
            {
                _log.Append(CreateEvent(_controller.TurnNumber, _controller.CurrentPhase, null, TurnEventType.PhaseAdvanced, $"Advanced to {_controller.CurrentPhase} phase."));
            }

            if (_controller.ActiveCommandPhase != _lastCommandPhase && _controller.ActiveCommandPhase.HasValue)
            {
                _log.Append(CreateEvent(_controller.TurnNumber, _controller.CurrentPhase, _controller.ActiveCommandPhase, TurnEventType.CommandPhaseAdvanced, $"Command phase {_controller.ActiveCommandPhase} activated."));
            }
        }

        _wasActive = _controller.IsTurnActive;
        _lastPhase = _controller.CurrentPhase;
        _lastCommandPhase = _controller.ActiveCommandPhase;
    }

    private void OnTurnCompleted(object? sender, EventArgs e)
    {
        var completedTurn = _controller.TurnNumber - 1;
        if (completedTurn <= _lastCompletedTurn)
        {
            completedTurn = _controller.TurnNumber;
        }

        _lastCompletedTurn = completedTurn;

        _log.Append(CreateEvent(completedTurn, TurnPhase.Elimination, null, TurnEventType.TurnCompleted, "Turn completed."));
    }

    private TurnEvent CreateEvent(int turnNumber, TurnPhase phase, CommandPhase? commandPhase, TurnEventType type, string description)
    {
        var timestamp = DateTimeOffset.UtcNow;
        _sequenceNumber++;
        return new TurnEvent(turnNumber, phase, commandPhase, type, description, timestamp);
    }
}
