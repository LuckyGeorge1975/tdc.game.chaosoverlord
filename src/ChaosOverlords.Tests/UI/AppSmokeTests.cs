using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using ChaosOverlords.App;
using ChaosOverlords.App.ViewModels;
using ChaosOverlords.App.Views;
using Xunit;

namespace ChaosOverlords.Tests.UI;

public class AppSmokeTests
{
    [AvaloniaFact]
    public void MainWindow_bootstraps_with_default_view_model()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();

        Program.BuildAvaloniaApp()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false
            })
            .SetupWithLifetime(lifetime);

        try
        {
            var window = Assert.IsType<MainWindow>(lifetime.MainWindow);

            var mainViewModel = Assert.IsType<MainViewModel>(window.DataContext);
            Assert.IsType<MapViewModel>(mainViewModel.CurrentViewModel);
        }
        finally
        {
            lifetime.Shutdown();
        }
    }
}
