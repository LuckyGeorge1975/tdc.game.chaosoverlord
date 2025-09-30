# Chaos Overlords Development Phases

> Statuspflege: Nach Abschluss einer Phase die Ergebnisse in `Planning/chaosoverlord.progress.md` dokumentieren (inkl. Verweis auf den Merge nach `main`).

## Phase 1 – Bootstrapping
**Status:** ✅ Completed (2025-09-28) – Tasks 1–6 umgesetzt, App startet via DI, Map & New Game funktionsfähig.
- Set up solution, projects, and DI container.
- Load JSON reference data (gangs, items, sites).
- Implement `GameState` and `ScenarioService.CreateNewGame`.
- Display map placeholder (8×8 sectors).

## Phase 2 – Rundenlogik „Happy Path“
**Status:** 🚧 In Progress (2025-09-30) – Turn dashboard modularisiert (MessageHub + Abschnitt-ViewModels), Finance Preview & Timeline live; verbleibend: Sektorklassen/-toleranz (Task 14) und Chaos-Auszahlung (Phase 3/5).
- TurnViewModel steuert Phasen inkl. Command-Subphasen (✅).
- EconomyService: upkeep & income inkl. Event-Log (✅).
- RecruitmentService: Pool-Refresh, Hire/Ablehnen, deterministisch (✅).
- Command Queue/Resolver: Move/Control/Chaos (Stub) + Timeline-Visualisierung (✅).
- Finance Preview: City/Sector-Projektion als eigener Abschnitt (✅).
- Map & Sector Basisklassen/Toleranz (Task 14) (⏳).

## Phase 3 – Kernaktionen
- Implement Influence, Research, Equip, Move.
- Dice/Check utilities.
- Map interactions.

## Phase 4 – Kampf & Verstecken/Aufspüren
- Attack & Terminate resolution.
- Hide & Search.
- Combat dialogs.

## Phase 5 – Polizei & Szenarien
- Crackdown system (Chaos vs. Tolerance).
- Implement win/loss checks for scenarios.

## Phase 6 – Feinschliff & Saves
- Save/Load service.
- UI polish.
- Stability & performance.

## Phase 7 – Silver City Adaptation (Priority 2)
- Lore- und Fraktions-Remix („Cons“) ausarbeiten.
- Con-Referenzdaten laden und in GameState integrieren.
- Modifier in Wirtschaft/Commands/Movement/Recruitment anwenden.
- UI-Erweiterungen für Con-Auswahl und Darstellung.
- Save/Load & QA für Con-spezifische Features.
