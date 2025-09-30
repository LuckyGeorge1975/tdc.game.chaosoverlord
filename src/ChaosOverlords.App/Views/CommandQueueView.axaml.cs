using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChaosOverlords.App.Views;

public partial class CommandQueueView : UserControl
{
    public CommandQueueView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
