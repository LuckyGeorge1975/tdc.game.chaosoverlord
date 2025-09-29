using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChaosOverlords.App.ViewModels;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Services;
using ChaosOverlords.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ChaosOverlords.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
#if DEBUG
        EnableRuntimeXamlFallback();
#endif
        ConfigureServices();
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
            if (_serviceProvider is null)
            {
                throw new InvalidOperationException("Service provider has not been initialized.");
            }

            // Ensure core services are ready before the UI interacts with the controller.
            var session = _serviceProvider.GetRequiredService<IGameSession>();
            session.InitializeAsync().GetAwaiter().GetResult();

            _serviceProvider.GetRequiredService<TurnEventRecorder>();
            _serviceProvider.GetRequiredService<TurnPhaseProcessor>();

            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            desktop.Exit += (_, _) => _serviceProvider?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataService, EmbeddedJsonDataService>();
        services.AddSingleton<IRngService, DeterministicRngService>();
        services.AddSingleton<IScenarioService, ScenarioService>();
        services.AddSingleton<IDefaultScenarioProvider, DefaultScenarioProvider>();
        services.AddSingleton<IGameSession, GameSession>();
        services.AddSingleton<IEconomyService, EconomyService>();
        services.AddSingleton<IRecruitmentService, RecruitmentService>();
        services.AddSingleton<ICommandQueueService, CommandQueueService>();
        services.AddSingleton<ICommandResolutionService, CommandResolutionService>();

        services.AddSingleton<ITurnEventLog, TurnEventLog>();
        services.AddSingleton<ITurnEventWriter, TurnEventWriter>();
        services.AddSingleton<TurnEventRecorder>();
        services.AddSingleton<TurnPhaseProcessor>();
        services.AddSingleton<ITurnController, TurnController>();
        services.AddSingleton<MapViewModel>();
        services.AddSingleton<TurnViewModel>();
        services.AddSingleton<MainViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
}