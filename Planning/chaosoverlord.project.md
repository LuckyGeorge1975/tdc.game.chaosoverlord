# Chaos Overlords Project Description

## Project Goal
Remake *Chaos Overlords* as a turn-based strategy game using **.NET 9 + Avalonia**, structured in **MVVM style** with lightweight **Services**.

## Core Principles
- **Simplicity**: Only add architecture that provides real value.
- **Testability**: Core mechanics (dice checks, economy, combat, research) are testable without UI.
- **Separation**: Game logic lives in `ChaosOverlords.Core`, UI in `ChaosOverlords.App`.
- **Data-driven**: All gangs, items, and sites are loaded from JSON (already available).
- **Transparenz**: Fortschritt je Task/Phase wird in `Planning/chaosoverlord.progress.md` dokumentiert.
- **Nachvollziehbarkeit**: Entscheidungen, Risiken und offene Fragen werden in `Planning/chaosoverlord.notes.md` festgehalten.

## Solution Structure
```
ChaosOverlords.sln
│
├─ src/
│  ├─ ChaosOverlords.App     # Avalonia UI (Views, ViewModels)
│  ├─ ChaosOverlords.Core    # Domain, Services, GameState
│  ├─ ChaosOverlords.Data    # JSON reference data
│  └─ ChaosOverlords.Tests   # xUnit tests for Core
```

## Key Components
- **GameState** – current snapshot (players, gangs, items, sectors, scenario).
- **Services** – IDataService, IScenarioService, ICommandResolver, IEconomyService, IPoliceService, IRecruitmentService, IWinCheckService, ISaveLoadService.
- **MVVM** – ViewModels for main UI, map, sector details, gangs, recruitment, turn control.
