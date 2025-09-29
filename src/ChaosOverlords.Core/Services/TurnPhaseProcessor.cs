using System;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Game.Events;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Coordinates phase-dependent service execution whenever the turn controller advances.
/// </summary>
public sealed class TurnPhaseProcessor : IDisposable
{
    private readonly ITurnController _turnController;
    private readonly IGameSession _gameSession;
    private readonly IEconomyService _economyService;
    private readonly ITurnEventWriter _eventWriter;
    private bool _upkeepProcessed;
    private int _processedTurnNumber;
    private bool _isDisposed;

    public TurnPhaseProcessor(
        ITurnController turnController,
        IGameSession gameSession,
        IEconomyService economyService,
        ITurnEventWriter eventWriter)
    {
        _turnController = turnController ?? throw new ArgumentNullException(nameof(turnController));
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        _economyService = economyService ?? throw new ArgumentNullException(nameof(economyService));
        _eventWriter = eventWriter ?? throw new ArgumentNullException(nameof(eventWriter));

        _turnController.StateChanged += OnTurnStateChanged;
        _turnController.TurnCompleted += OnTurnCompleted;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _turnController.StateChanged -= OnTurnStateChanged;
        _turnController.TurnCompleted -= OnTurnCompleted;
        _isDisposed = true;
    }

    private void OnTurnStateChanged(object? sender, EventArgs e)
    {
        if (!_turnController.IsTurnActive)
        {
            return;
        }

        var turnNumber = _turnController.TurnNumber;
        if (turnNumber != _processedTurnNumber)
        {
            _processedTurnNumber = turnNumber;
            _upkeepProcessed = false;
        }

        if (_turnController.CurrentPhase == TurnPhase.Upkeep && !_upkeepProcessed)
        {
            EnsureSessionInitialised();
            ProcessUpkeep(turnNumber);
            _upkeepProcessed = true;
        }
    }

    private void OnTurnCompleted(object? sender, EventArgs e)
    {
        _upkeepProcessed = false;
    }

    private void ProcessUpkeep(int turnNumber)
    {
        var report = _economyService.ApplyUpkeep(_gameSession.GameState, turnNumber);
        foreach (var snapshot in report.PlayerSnapshots)
        {
            _eventWriter.WriteEconomy(turnNumber, TurnPhase.Upkeep, snapshot);
        }
    }

    private void EnsureSessionInitialised()
    {
        if (_gameSession.IsInitialized)
        {
            return;
        }

        _gameSession.InitializeAsync().GetAwaiter().GetResult();
    }
}
