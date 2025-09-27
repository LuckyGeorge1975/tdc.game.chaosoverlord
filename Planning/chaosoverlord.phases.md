# Chaos Overlords Development Phases

> Statuspflege: Nach Abschluss einer Phase die Ergebnisse in `Planning/chaosoverlord.progress.md` dokumentieren (inkl. Verweis auf den Merge nach `main`).

## Phase 1 – Bootstrapping
- Set up solution, projects, and DI container.
- Load JSON reference data (gangs, items, sites).
- Implement `GameState` and `ScenarioService.CreateNewGame`.
- Display map placeholder (8×8 sectors).

## Phase 2 – Rundenlogik „Happy Path“
- TurnViewModel to manage phases.
- EconomyService: upkeep & income.
- RecruitmentService: refill pool, hire gang.
- CommandResolver: queue and execute commands (stub).

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
