# Chaos Overlords – Phase 1 Tasks

> Statuspflege: Abgeschlossene, blockierte oder obsolet gewordene Tasks in `Planning/chaosoverlord.progress.md` erfassen.

## Task 1 – Solution Setup
- Create solution `ChaosOverlords.sln`.
- Add projects: `ChaosOverlords.App` (Avalonia), `ChaosOverlords.Core`, `ChaosOverlords.Data`, `ChaosOverlords.Tests`.
- Configure references: App → Core + Data, Tests → Core.

## Task 2 – Reference Data Loader
- Add JSON files (gangs, items, sites) to `ChaosOverlords.Data` as EmbeddedResources.
- Implement `IDataService` in Core.
- Implement `EmbeddedJsonDataService` to load and parse JSON into models.

## Task 3 – Domain Models
- Create models for `GangRef`, `ItemRef`, `SiteRef`.
- Create runtime models: `GameState`, `PlayerState`, `GangInstance`, `ItemInstance`, `SectorState`.

## Task 4 – Scenario Service
- Add `ScenarioConfig` and `ScenarioType` enum.
- Implement `IScenarioService`.
- Provide `CreateNewGame` that initializes GameState with HQ, start cash, start gang.

## Task 5 – Avalonia App Shell
- Setup Avalonia App project with CommunityToolkit.Mvvm.
- Implement `MainViewModel` and placeholder `MapViewModel` (8×8 grid dummy).
- Bind simple view with placeholder sectors.

## Task 6 – Dependency Injection
- Setup `ServiceCollection` in App.
- Register all services (IDataService, IScenarioService, …) with stub implementations where needed.

**Acceptance criteria:**
- App starts.
- New Game can be created.
- 8×8 placeholder map shown.
- Reference data loads successfully.
