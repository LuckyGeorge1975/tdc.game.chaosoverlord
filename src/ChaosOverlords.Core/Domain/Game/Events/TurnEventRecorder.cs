using System;
using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
/// Observes the turn controller and records high-level lifecycle events in the log.
/// </summary>
public sealed class TurnEventRecorder : IDisposable
{
    private readonly ITurnController _controller;
    private readonly ITurnEventWriter _eventWriter;
    private bool _wasActive;
    private TurnPhase _lastPhase;
    private CommandPhase? _lastCommandPhase;
    private int _lastCompletedTurn;
    private bool _isDisposed;

    public TurnEventRecorder(ITurnController controller, ITurnEventWriter eventWriter)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _eventWriter = eventWriter ?? throw new ArgumentNullException(nameof(eventWriter));

        _controller.StateChanged += OnStateChanged;
        _controller.TurnCompleted += OnTurnCompleted;

        _wasActive = _controller.IsTurnActive;
        _lastPhase = _controller.CurrentPhase;
        _lastCommandPhase = _controller.ActiveCommandPhase;
        _lastCompletedTurn = _controller.TurnNumber - 1;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _controller.StateChanged -= OnStateChanged;
        _controller.TurnCompleted -= OnTurnCompleted;
        _isDisposed = true;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        var turnNumber = _controller.TurnNumber;

        if (_controller.IsTurnActive && !_wasActive)
        {
            _eventWriter.Write(turnNumber, _controller.CurrentPhase, TurnEventType.TurnStarted, "Turn started.");
        }

        if (_controller.IsTurnActive)
        {
            if (_controller.CurrentPhase != _lastPhase)
            {
                _eventWriter.Write(turnNumber, _controller.CurrentPhase, TurnEventType.PhaseAdvanced, $"Advanced to {_controller.CurrentPhase} phase.");
            }

            if (_controller.ActiveCommandPhase != _lastCommandPhase && _controller.ActiveCommandPhase.HasValue)
            {
                _eventWriter.Write(turnNumber, _controller.CurrentPhase, TurnEventType.CommandPhaseAdvanced, $"Command phase {_controller.ActiveCommandPhase} activated.", _controller.ActiveCommandPhase);
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

        _eventWriter.Write(completedTurn, TurnPhase.Elimination, TurnEventType.TurnCompleted, "Turn completed.");
    }
}
