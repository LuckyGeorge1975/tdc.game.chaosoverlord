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
| Priority 2 | Task S1 – Con Referenzdaten | Planned | JSON + Domain-Modelle vorbereiten; wartet auf Abschluss Phase-2-Happy-Path |
| Priority 2 | Task S2 – Con Auswahl & Szenariofilter | Planned | Con-Picker & Szenariofilter nach Cons; Umsetzung nach S1 |
| Priority 2 | Task S3 – Con Modifiers anwenden | Planned | Modifier in Economy/Commands/Movement/Recruitment integrieren |
| Priority 2 | Task S4 – UI Darstellung & Feedback | Planned | HUD-Badge & Tooltips ergänzen |
| Priority 2 | Task S5 – Persistenz & QA | Planned | Save/Load + Tests/Doku für Con-Features |
