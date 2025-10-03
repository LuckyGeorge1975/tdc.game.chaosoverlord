using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChaosOverlords.App.ViewModels.Sections;

namespace ChaosOverlords.App.Views;

public partial class TurnEventsView : UserControl
{
    public TurnEventsView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => HookAutoScroll();
        DetachedFromVisualTree += (_, _) => UnhookAutoScroll();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void HookAutoScroll()
    {
        if (DataContext is TurnEventsSectionViewModel vm) vm.Events.CollectionChanged += OnEventsCollectionChanged;
    }

    private void UnhookAutoScroll()
    {
        if (DataContext is TurnEventsSectionViewModel vm) vm.Events.CollectionChanged -= OnEventsCollectionChanged;
    }

    private void OnEventsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DataContext is not TurnEventsSectionViewModel vm || !vm.AutoScrollToLatest) return;

        if (this.FindControl<ScrollViewer>("EventsScrollViewer") is { } scroll)
            // Scroll to bottom to reveal the latest entries
            scroll.ScrollToEnd();
    }
}