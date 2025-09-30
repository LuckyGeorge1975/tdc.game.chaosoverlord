# Chaos Overlords – Task Progress

| Phase | Task | Status | Notes |
| --- | --- | --- | --- |
| Phase 1 | Task 1 – Solution Setup | Done | Solution + projects scaffolded, CI workflow added, Release build & tests passing |
| Phase 1 | Task 2 – Reference Data Loader | Done | IDataService + EmbeddedJsonDataService implemented, JSON embedded, unit tests passing (2025-09-27) |
| Phase 1 | Task 3 – Domain Models | Done | Runtime gangs/items/sectors link directly to GameData, stat breakdowns exposed, tests passing (2025-09-28) |
| Phase 1 | Task 4 – Scenario Service | Done | ScenarioService + GameStateManager create multi-player GameState, AI/network controllers stubbed awaiting later logic (2025-09-28) |
| Phase 1 | Task 5 – Avalonia App Shell | Done | App-Shell mit Main-/MapViewModel, Debug-XAML-Fallback und headless Smoke-Test für Startup abgesichert (2025-09-28) |
| Phase 1 | Task 6 – Dependency Injection | Done | ServiceCollection registriert Daten-/Szenario-Services und liefert MainViewModel/MapViewModel für die App (2025-09-28) |
| Phase 2 | Task 7 – TurnViewModel & Phasen-State-Machine | Done | TurnViewModel steuert Phasen & Command-Timeline, UI zeigt Slots/States, Buttons gating + Unit-Test abgesichert (2025-09-28) |
| Phase 2 | Task 8 – RngService & Turn-Event-Log | Done | Deterministic `IRngService`, seeded via GameState; turn event log records controller state transitions (2025-09-29) |
| Phase 2 | Task 9 – EconomyService (Upkeep & Income) | Done | EconomyService applies upkeep/tax/site cash, turn log captures per-player snapshots, unit tests cover mixed cases (2025-09-29) |
| Phase 2 | Task 10 – RecruitmentService (Pool & Hire) | Done | Recruitment service w/ deterministic pool refresh, Hire phase UI & event logging wired, unit tests cover hire/decline edge cases (2025-09-29) |
| Phase 2 | Task 11 – Command Queue & Resolver (Skeleton) | Done | Command queue/service/resolver wired into turn processor, UI queue management with comment-preserving binding, new unit tests passing (2025-09-29) |
| Phase 2 | Task 12 – Finance Preview (City & Sector) | Done | FinancePreviewService projects city/sector deltas, TurnViewModel exposes shared collections, new section view model renders panel (2025-09-30) |
| Phase 2 | Task 13 – Phasen-Timeline UI (Befehlsvisualisierung) | Done | MessageHub broadcasts timeline snapshots, CommandTimeline section updates reactively, panel styled for phase states (2025-09-30) |
