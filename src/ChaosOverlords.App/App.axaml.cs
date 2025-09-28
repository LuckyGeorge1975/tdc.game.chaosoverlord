using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChaosOverlords.App.ViewModels;

namespace ChaosOverlords.App;

public partial class App : Application
{
    public override void Initialize()
    {
#if DEBUG
        EnableRuntimeXamlFallback();
#endif
        AvaloniaXamlLoader.Load(this);
    }

#if DEBUG
    private static void EnableRuntimeXamlFallback()
    {
        // Debug-only safeguard: disable compiled XAML assumptions so runtime binding issues surface early.
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        try
        {
            var loaderType = typeof(AvaloniaXamlLoader);
            var member = loaderType.GetMember("AssumeCompiled", flags).FirstOrDefault();

            switch (member)
            {
                case PropertyInfo property when property.CanWrite:
                    property.SetValue(null, false);
                    break;
                case FieldInfo field:
                    field.SetValue(null, false);
                    break;
                default:
                    Debug.WriteLine("AvaloniaXamlLoader.AssumeCompiled member not found; runtime XAML fallback unavailable.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Runtime XAML fallback could not be enabled: {ex.Message}");
        }
    }
#endif

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mapViewModel = new MapViewModel();
            var mainViewModel = new MainViewModel(mapViewModel);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}