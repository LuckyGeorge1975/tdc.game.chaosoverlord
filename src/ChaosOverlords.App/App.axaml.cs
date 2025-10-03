using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChaosOverlords.App.ViewModels;
using ChaosOverlords.Core.Configuration;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Services;
using ChaosOverlords.Core.Services.Messaging;
using ChaosOverlords.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChaosOverlords.App;

public class App : Application
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
                case PropertyInfo { CanWrite: true } property:
                    property.SetValue(null, false);
                    break;
                case FieldInfo field:
                    field.SetValue(null, false);
                    break;
                default:
                    Debug.WriteLine(
                        "AvaloniaXamlLoader.AssumeCompiled member not found; runtime XAML fallback unavailable.");
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
                throw new InvalidOperationException("Service provider has not been initialized.");

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

        // Build configuration: appsettings.json + environment variables (keep it simple for now)
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables();

        var configuration = configBuilder.Build();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddSingleton<IDataService, EmbeddedJsonDataService>();
        services.AddSingleton<IRngService, DeterministicRngService>();
        services.AddSingleton<IScenarioService, ScenarioService>();
        services.AddSingleton<IDefaultScenarioProvider, DefaultScenarioProvider>();
        services.AddSingleton<IGameSession, GameSession>();
        services.AddSingleton<IEconomyService, EconomyService>();
        services.AddSingleton<IRecruitmentService, RecruitmentService>();
        services.AddSingleton<IResearchService>(sp => new ResearchService(sp.GetRequiredService<IDataService>()));
        services.AddSingleton<IEquipmentService, EquipmentService>();
        services.AddSingleton<ICommandQueueService, CommandQueueService>();
        services.AddSingleton<ICommandResolutionService>(sp => new CommandResolutionService(
            sp.GetRequiredService<ITurnEventWriter>(),
            sp.GetRequiredService<IRngService>(),
            sp.GetRequiredService<IResearchService>(),
            sp.GetRequiredService<IDataService>(),
            sp.GetRequiredService<IEquipmentService>()));
        services.AddSingleton<IFinancePreviewService>(sp =>
            new FinancePreviewService(sp.GetRequiredService<IDataService>()));
        services.AddSingleton<IMessageHub, MessageHub>();

        // Bind domain logging options and expose the bound POCO for constructor injection (keeps Core free of Options deps)
        services.Configure<LoggingOptions>(configuration.GetSection("Logging:TurnEvents"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<LoggingOptions>>().Value);
        services.AddSingleton<ITurnEventLog, TurnEventLog>();
        services.AddSingleton<ILogPathProvider, LogPathProvider>();
        services.AddSingleton<ITurnEventWriter, FileTurnEventWriter>();
        services.AddSingleton<TurnEventRecorder>();
        services.AddSingleton<TurnPhaseProcessor>();
        services.AddSingleton<ITurnController, TurnController>();
        services.AddSingleton<MapViewModel>();
        services.AddSingleton<TurnViewModel>();
        services.AddSingleton<MainViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
}