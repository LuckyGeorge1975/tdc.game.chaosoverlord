# Chaos Overlords – Architecture Overview

This document captures the current structure of the Chaos Overlords remaster so new contributors can quickly understand how the projects, classes, and services fit together.

## Layering & Projects

```
┌───────────────────────────────────────────────────────────────┐
│ App Layer: ChaosOverlords.App (Avalonia UI)                    │
│   • ViewModels, Views, DI composition root                     │
├───────────────────────────────────────────────────────────────┤
│ Core Layer: ChaosOverlords.Core                                │
│   • Domain models (Game, Player, Gang, Sector, …)              │
│   • Services (Scenario, Session, RNG, Economy, Turn processing)│
│   • Eventing (Turn event log & writer)                         │
├───────────────────────────────────────────────────────────────┤
│ Data Layer: ChaosOverlords.Data                                │
│   • Embedded JSON assets + IDataService implementation         │
├───────────────────────────────────────────────────────────────┤
│ Tests: ChaosOverlords.Tests                                    │
│   • Unit & smoke tests targeting Core/Common functionality     │
└───────────────────────────────────────────────────────────────┘
```

| Project | Responsibility | Key Types |
| --- | --- | --- |
| `ChaosOverlords.App` | Avalonia desktop shell, dependency injection bootstrap, presentation logic via MVVM | `App`, `Program`, `MainViewModel`, `MapViewModel`, `TurnViewModel`, section view models (`TurnManagementSectionViewModel`, `CommandTimelineSectionViewModel`, `FinancePreviewSectionViewModel`, `RecruitmentSectionViewModel`, `CommandQueueSectionViewModel`, `TurnEventsSectionViewModel`) |
| `ChaosOverlords.Core` | Domain entities, scenario bootstrapping, turn controller, services, logging, economy | `Game`, `GameState`, `GameStateManager`, `TurnController`, `ScenarioService`, `GameSession`, `DeterministicRngService`, `EconomyService`, `FinancePreviewService`, `CommandQueueService`, `CommandResolutionService`, `MessageHub`, `TurnEventLog`, `TurnEventWriter`, `TurnPhaseProcessor` |
| `ChaosOverlords.Data` | Static game data (gangs, items, sites) embedded in assembly, loader implementation | `EmbeddedJsonDataService`, JSON asset files |
| `ChaosOverlords.Tests` | Automated verification for core logic and UI smoke tests | `ScenarioServiceTests`, `DeterministicRngServiceTests`, `TurnViewModelTests`, `EconomyServiceTests`, `TurnPhaseProcessorTests`, `AppSmokeTests` |

## Core Layer Modules & Class Relationships

### Scenario Bootstrapping
- **`ScenarioConfig` / `ScenarioPlayerConfig`**: Immutable configuration describing players, map layout, and optional seed.
- **`IScenarioService`** → **`ScenarioService`**:
  - Loads gangs/sites via `IDataService` (from Data layer).
  - Instantiates `Player`/`AiPlayer`/`NetworkPlayer` based on config.
  - Builds runtime `Sector`, `Gang`, and `Game` instances.
  - Seeds the deterministic RNG via `IRngService`.
- **`IDefaultScenarioProvider`** → **`DefaultScenarioProvider`**: Supplies a lightweight starter scenario for happy-path testing.

### Session Management
- **`IGameSession`** → **`GameSession`**:
  - Lazily initializes a campaign using `IScenarioService` and `IDefaultScenarioProvider`.
  - Exposes the active `GameState` and `GameStateManager` for consumers.
  - Ensures initialization occurs exactly once per app lifetime (until disposal).

### Messaging & Coordination
- **`IMessageHub`** → **`MessageHub`**: Lightweight publish/subscribe hub used to decouple services and presentation components. The hub lives in Core so both services and view models can exchange notifications without direct references.
- **App Messaging Records** (`CommandTimelineUpdatedMessage`, `TurnSummaryChangedMessage`): Simple payloads published by `TurnViewModel` to update section view models.

### Game Runtime State
- **`Game`**: Aggregates runtime entities and manages cross-references (players, gangs, items, sectors).
- **`GameState`**: Captures campaign-wide status (turn order, active player, seed, scenario metadata).
- **`GameStateManager`**: Convenience wrapper coordinating per-player turn execution.
- **`TurnController`** (`ITurnController` implementation): Holds the turn-phase state machine and command sub-phase timeline, raising events on state changes.

### Domain Entities
- **Players**: `PlayerBase` (common bookkeeping), `Player` (human), `AiPlayer`, `NetworkPlayer` (stubs for future networking).
- **`Gang`**: Runtime gang with stat aggregation and equipment management.
- **`Item`**: Equipment with stat modifiers.
- **`Sector`**: Map tile optionally linked to `SiteData`, tracks occupying gangs and controlling player.
- **`StatSheet` / `StatBreakdown`**: Value objects summarizing stats and their sources.

### Randomness
- **`IRngService`**: Abstraction for deterministic random number generation.
- **`DeterministicRngService`**: XorShift128+ implementation with SplitMix64 seeding. Seed stored in `GameState`, ensuring reproducibility.

### Economy & Turn Processing
- **`IEconomyService`** → **`EconomyService`**:
  - Computes upkeep costs, sector tax (+1/controlled sector), and site cash modifiers (positive/negative).
  - Adjusts player balances through `PlayerBase.Credit`/`Debit` and returns an `EconomyReport` comprised of `PlayerEconomySnapshot` entries.
- **`PlayerEconomySnapshot` / `EconomyReport`**: Immutable records summarizing per-player and aggregated financial deltas per turn.
- **`TurnPhaseProcessor`**:
  - Subscribes to `ITurnController` events.
  - Ensures the `IGameSession` is initialized when a turn starts.
  - Invokes `IEconomyService` exactly once during the Upkeep phase and forwards the resulting snapshots to the turn event log via `ITurnEventWriter`.

### Finance Preview
- **`IFinancePreviewService`** → **`FinancePreviewService`**:
  - Builds deterministic projections for upcoming income/expenses per player using the current `GameState`.
  - Aggregates city-wide categories (`FinanceCategory`) and sector-specific projections (`FinanceSectorProjection`), producing a `FinanceProjection` consumed by the UI.
- **Finance Preview Models**:
  - `FinanceCategoryType`, `FinanceCategory`, `FinanceSectorProjection`, `FinanceProjection` describe line items, sector breakdowns, and total adjustments while preserving determinism.

### Command Queue & Resolution
- **`ICommandQueueService`** → **`CommandQueueService`**: Maintains per-gang command assignments, ensuring one action per turn.
- **`ICommandResolutionService`** → **`CommandResolutionService`**: Executes queued commands in timeline order, emitting summaries for the UI via the message hub and event log.

### Event Logging
- **`TurnEventType`** enum: Includes `TurnStarted`, `PhaseAdvanced`, `CommandPhaseAdvanced`, `TurnCompleted`, `Information`, and `Economy`.
- **`TurnEvent`**: Immutable record representing a single log entry (turn, phase, description, timestamp).
- **`ITurnEventLog`** → **`TurnEventLog`**: Ring buffer preserving the last N events, raising change notifications.
- **`ITurnEventWriter`** → **`TurnEventWriter`**: Factory for `TurnEvent` instances. Provides helpers for generic events and economy-specific summaries.
- **`TurnEventRecorder`**: Observes the `TurnController`; uses `ITurnEventWriter` to emit lifecycle events.

### Supporting Services
- **`IDataService`** (Core abstraction) implemented by the Data project.
- **`ServiceCollection` setup** (App project) wires interfaces to implementations and registers view models for DI.

## Data Layer
- **`EmbeddedJsonDataService`** implements `IDataService`, loading JSON assets embedded in `ChaosOverlords.Data`.
- JSON asset files (`gangs.json`, `items.json`, `sites.json`) serialize into `GangData`, `ItemData`, `SiteData` records.
- Provides read-only data sets used by `ScenarioService` during game creation.

## App Layer
- **`App`**: Avalonia application entry point. Configures services, ensures `GameSession`, `TurnEventRecorder`, and `TurnPhaseProcessor` are ready before `MainWindow` is shown.
- **`MainWindow` / Views**: Standard Avalonia window bound to `MainViewModel`.
- **ViewModels**:
  - `MainViewModel`: Aggregates child view models and exposes the composed turn dashboard.
  - `MapViewModel`: Placeholder 8×8 grid; future hook for real sector state.
  - `TurnViewModel`: Observes `ITurnController`, `ITurnEventLog`, and Core services to project turn state, finance projections, recruitment pool, and command queue. It publishes summary messages through `IMessageHub` so section view models stay synchronized.
  - Section ViewModels (`TurnManagementSectionViewModel`, `CommandTimelineSectionViewModel`, `FinancePreviewSectionViewModel`, `RecruitmentSectionViewModel`, `CommandQueueSectionViewModel`, `TurnEventsSectionViewModel`): Encapsulate UI bindings per dashboard panel, subscribing to the message hub or owner view model for updates.

## Tests
- **Core verification**: `ScenarioServiceTests`, `DeterministicRngServiceTests`, `EconomyServiceTests`, `TurnPhaseProcessorTests`.
- **Domain entities**: `GameTests`, `GangTests`, `SectorTests`, etc.
- **UI smoke**: `AppSmokeTests` to ensure the Avalonia shell boots with DI wiring.

## Key Flows & Use Cases

### Application Startup / New Game Flow
1. `Program.Main` constructs `AppBuilder` and launches Avalonia.
2. `App.Initialize` composes services via `ServiceCollection` (registering Core/Data services and view models).
3. During `OnFrameworkInitializationCompleted`:
   - `IGameSession.InitializeAsync()` creates a new campaign using `DefaultScenarioProvider` + `ScenarioService`.
   - `TurnEventRecorder` and `TurnPhaseProcessor` subscribe to `ITurnController` events.
   - `MainViewModel` is resolved and assigned to `MainWindow`.
4. UI loads with the default map view and turn panel ready; `TurnViewModel` already has access to seeded `ITurnController` state and the event log.

### Turn Cycle Flow (Happy Path)
1. User triggers `StartTurnCommand` (or AI executes automatically).
2. `TurnController` sets `TurnPhase.Upkeep` and raises `StateChanged`.
3. `TurnPhaseProcessor` detects Upkeep →
   - Ensures `GameSession` is initialized.
   - Calls `EconomyService.ApplyUpkeep()` on the active `GameState`.
   - Receives an `EconomyReport`; writes each snapshot via `TurnEventWriter.WriteEconomy()`.
4. `TurnEventLog` raises `EventsChanged` → `TurnViewModel` refreshes its observable list; UI now shows economy results for the turn.
5. `TurnViewModel` publishes `CommandTimelineUpdatedMessage` payloads through the message hub, keeping the command timeline section in sync with the controller.
6. `TurnViewModel` requests a fresh `FinanceProjection` from `IFinancePreviewService`, updating city/sector previews for the active player.
7. Subsequent calls to `AdvancePhaseCommand` progress through Command → Execution → Hire → Elimination.
8. When `TurnController.EndTurn()` fires, `TurnEventRecorder` logs completion and resets state for the next round.

### Finance Preview Flow
1. `TurnViewModel` observes `ITurnController.StateChanged` and `IGameSession.GameState` updates.
2. Whenever the active player or phase changes, the view model calls `IFinancePreviewService.BuildProjection()` with the seeded `GameState` and player id.
3. The resulting `FinanceProjection` populates shared observable collections. Section view models (city + sector panels) mirror these via property-changed notifications.
4. UI bindings format positive/negative amounts and update the total cash adjustment heading.

### Data Retrieval Flow
- `ScenarioService` invokes `IDataService.GetGangsAsync()` / `GetSitesAsync()` during campaign creation.
- `EmbeddedJsonDataService` deserializes embedded JSON assets and returns read-only collections.
- Runtime entities (`Gang`, `Sector`, etc.) retain references to their immutable data counterparts for bonuses & metadata.

### Deterministic RNG Flow
1. `ScenarioService` chooses or generates a seed (persisted in `ScenarioConfig`/`GameState`).
2. `IRngService.Reset(seed)` primes the `DeterministicRngService`.
3. Systems requiring randomness (future recruitment, combat) consume `IRngService` methods; replaying with the same seed reproduces outcomes.

---

This overview will evolve as upcoming tasks (timeline polish, sector class metadata, Silver City cons) land. Please keep it in sync when introducing new services or modifying interaction patterns.
