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
| Phase 2 | Task 14 – Map & Sector-Model: Klassen & Basistoleranz | Done | Sector state now site-driven; all sectors seeded with SiteData feeding map tooltips and economy/finance previews (2025-10-01) |
| Phase 3 | Task 15 – Action Resolution Framework & Dice Utilities | Done | ActionContext/Result-Modelle + Dice-Rolls implementiert; Edge-Case-Tests (Auto-Thresholds, Modifier-Aggregation) ergänzt; Control-Command schreibt strukturierte Turn-Events (2025-10-01) |
| Phase 3 | Task 16 – Movement & Map Interaction Upgrade | Done | 8‑Nachbarschaft (orthogonal+diagonal), max. 6 eigene Gangs/Sektor, Ausführung in Execution‑Phase; UI‑Highlighting für kontrollierte Sektoren; Tests für Adjazenz, Kapazität, Timing (2025-10-02) |
| Phase 3 | Task 17 – Influence Actions (Control & Support Shifts) | Done | Influence end‑to‑end inkl. Queue/Resolver, RNG‑Integration, UI‑Wiring, permissive Queue‑Checks; autoritative Ausführung, Widerstands‑Reduktion und deterministische Event‑Logs; Desync‑Test ergänzt (2025-10-02) |
| Phase 3 | Sofortmaßnahme – Turn‑Event‑Logging & Config | Done | Datei‑basiertes Logging mit `IOptions`‑Bound `LoggingOptions`, Auto‑Scroll‑Toggle, „Open Logs Folder“‑Button; Retention (`MaxRetainedFiles`), Pfadauflöser, Windows‑Share‑Fix, File‑Logging‑Test (2025-10-02) |
| Phase 3 | Task 18 – Research & Equipment Management | Planned | Add research progress, equipment inventory, and finance hooks including tests for edge cases |
| Phase 3 | Task 19 – UI & Logging Integration für Kernaktionen | Planned | Surface new actions in UI panels, tooltips, and event logs; create smoke scenario covering a full core-action loop |
| Phase 4 | Task 20 – Combat Resolution Engine | Planned | Build deterministic combat engine covering attack/terminate flows with comprehensive reporting |
| Phase 4 | Task 21 – Hide & Search Mechanics | Planned | Implement stealth checks and counteractions tied to the action framework |
| Phase 4 | Task 22 – Combat & Stealth UI | Planned | Deliver accessible overlays/dialogs for combat/hide results with timeline/map indicators |
| Phase 4 | Task 23 – Event Log & Economy Integration | Planned | Integrate combat/stealth outcomes with event log and economy preview, ensuring regression coverage |
| Phase 3 | Sofortmaßnahme – Sector Influence Runtime | Done | Sector hat `InfluenceResistance`, `IsInfluenced`, `ReduceInfluenceResistance`, `ResetInfluence`; zugehörige Unit-Tests in `SectorTests` ergänzt (2025-10-01) |
