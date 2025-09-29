using System;
using System.Globalization;
using System.Linq;
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
    private readonly IRecruitmentService _recruitmentService;
    private readonly ITurnEventWriter _eventWriter;
    private readonly ICommandResolutionService _commandResolutionService;
    private bool _upkeepProcessed;
    private bool _recruitmentRefreshed;
    private bool _commandsResolved;
    private int _processedTurnNumber;
    private bool _isDisposed;

    public TurnPhaseProcessor(
        ITurnController turnController,
        IGameSession gameSession,
        IEconomyService economyService,
        IRecruitmentService recruitmentService,
        ITurnEventWriter eventWriter,
        ICommandResolutionService commandResolutionService)
    {
        _turnController = turnController ?? throw new ArgumentNullException(nameof(turnController));
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        _economyService = economyService ?? throw new ArgumentNullException(nameof(economyService));
        _recruitmentService = recruitmentService ?? throw new ArgumentNullException(nameof(recruitmentService));
        _eventWriter = eventWriter ?? throw new ArgumentNullException(nameof(eventWriter));
        _commandResolutionService = commandResolutionService ?? throw new ArgumentNullException(nameof(commandResolutionService));

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
            _recruitmentRefreshed = false;
            _commandsResolved = false;
        }

        if (_turnController.CurrentPhase == TurnPhase.Upkeep && !_upkeepProcessed)
        {
            EnsureSessionInitialised();
            ProcessUpkeep(turnNumber);
            _upkeepProcessed = true;
        }

        if (_turnController.CurrentPhase == TurnPhase.Execution && !_commandsResolved)
        {
            EnsureSessionInitialised();
            ProcessCommands(turnNumber);
            _commandsResolved = true;
        }
    }

    private void OnTurnCompleted(object? sender, EventArgs e)
    {
        _upkeepProcessed = false;
        _recruitmentRefreshed = false;
        _commandsResolved = false;
    }

    private void ProcessUpkeep(int turnNumber)
    {
        if (!_recruitmentRefreshed)
        {
            RefreshRecruitment(turnNumber);
            _recruitmentRefreshed = true;
        }

        var report = _economyService.ApplyUpkeep(_gameSession.GameState, turnNumber);
        foreach (var snapshot in report.PlayerSnapshots)
        {
            _eventWriter.WriteEconomy(turnNumber, TurnPhase.Upkeep, snapshot);
        }
    }

    private void RefreshRecruitment(int turnNumber)
    {
        var results = _recruitmentService.RefreshPools(_gameSession.GameState, turnNumber);
        foreach (var result in results)
        {
            if (!result.HasChanges)
            {
                continue;
            }

            var optionNames = string.Join(", ", result.Pool.Options.Select(o => o.GangName));
            var description = string.Format(
                CultureInfo.CurrentCulture,
                "{0}: recruitment pool refreshed ({1})",
                result.Pool.PlayerName,
                optionNames);

            _eventWriter.Write(turnNumber, TurnPhase.Upkeep, TurnEventType.Recruitment, description);
        }
    }

    private void ProcessCommands(int turnNumber)
    {
        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var report = _commandResolutionService.Execute(state, playerId, turnNumber);

        if (report.Entries.Count == 0)
        {
            var playerName = state.CurrentPlayer.Name;
            var description = string.Format(
                CultureInfo.CurrentCulture,
                "{0} had no commands to execute.",
                playerName);

            _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, description);
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
