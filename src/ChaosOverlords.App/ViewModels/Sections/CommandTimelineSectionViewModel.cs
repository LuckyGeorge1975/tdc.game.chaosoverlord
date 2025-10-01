using System;
using System.Collections.ObjectModel;
using ChaosOverlords.App.Messaging;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Services.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChaosOverlords.App.ViewModels.Sections;

public sealed class CommandTimelineSectionViewModel : IDisposable
{
    private readonly ObservableCollection<CommandPhaseEntryViewModel> _phases = new();
    private readonly IDisposable _subscription;

    public CommandTimelineSectionViewModel(IMessageHub messageHub)
    {
        if (messageHub is null)
        {
            throw new ArgumentNullException(nameof(messageHub));
        }

        Phases = new ReadOnlyObservableCollection<CommandPhaseEntryViewModel>(_phases);
        _subscription = messageHub.Subscribe<CommandTimelineUpdatedMessage>(OnTimelineUpdated);
    }

    public ReadOnlyObservableCollection<CommandPhaseEntryViewModel> Phases { get; }

    public void Dispose() => _subscription.Dispose();

    private void OnTimelineUpdated(CommandTimelineUpdatedMessage message)
    {
        if (message is null)
        {
            return;
        }

        if (_phases.Count != message.Phases.Count)
        {
            _phases.Clear();
            foreach (var phase in message.Phases)
            {
                _phases.Add(new CommandPhaseEntryViewModel(phase.Phase, phase.State));
            }

            return;
        }

        for (var i = 0; i < message.Phases.Count; i++)
        {
            _phases[i].State = message.Phases[i].State;
        }
    }
}

public sealed partial class CommandPhaseEntryViewModel : ObservableObject
{
    public CommandPhaseEntryViewModel(CommandPhase phase, CommandPhaseState state)
    {
        Phase = phase;
        _state = state;
    }

    public CommandPhase Phase { get; }

    public string DisplayName => Phase.ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActive))]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    private CommandPhaseState _state;

    public bool IsActive => State == CommandPhaseState.Active;

    public bool IsCompleted => State == CommandPhaseState.Completed;
}
