# Chaos Overlords – Phase 1 Tasks

> Statuspflege: Abgeschlossene, blockierte oder obsolet gewordene Tasks in `Planning/chaosoverlord.progress.md` erfassen.

## Task 1 – Solution Setup

**Status:** ✅ Done – solution scaffolded and projects linked (see `chaosoverlord.progress.md`).

- Create solution `ChaosOverlords.sln`.
- Add projects: `ChaosOverlords.App` (Avalonia), `ChaosOverlords.Core`, `ChaosOverlords.Data`, `ChaosOverlords.Tests`.
- Configure references: App → Core + Data, Tests → Core.

## Task 2 – Reference Data Loader

**Status:** ✅ Done – embedded JSON loader complete with unit tests (see `chaosoverlord.progress.md`).

- Add JSON files (gangs, items, sites) to `ChaosOverlords.Data` as EmbeddedResources.
- Implement `IDataService` in Core.
- Implement `EmbeddedJsonDataService` to load and parse JSON into models.

## Task 3 – Domain Models

**Status:** ✅ Done – runtime models wired to GameData with stat breakdowns (see `chaosoverlord.progress.md`).

- Create models for `GangRef`, `ItemRef`, `SiteRef`.
- Create runtime models: `Game`, `Player`, `Gang`, `Item`, `Sector`.

## Task 4 – Scenario Service

**Status:** ✅ Done – Scenario bootstrap service creates seeded multi-player GameState via IScenarioService (2025-09-28).

- Add `ScenarioConfig` and `ScenarioType` enum.
- Implement `IScenarioService`.
- Model player slots (Human/Easy/Medium/Hard AI/Network) via `IPlayer` interface + `ExecuteTurnAsync`.
- Provide `CreateNewGame` that initializes `GameState`/`GameStateManager` with multiple players, HQ sectors, start cash, start gang.

## Task 5 – Avalonia App Shell

**Status:** ✅ Done – App-Shell steht, Debug-XAML-Fallback & Headless-Smoke-Test sichern Start-Up (2025-09-28).

- Setup Avalonia App project with CommunityToolkit.Mvvm.
- Implement `MainViewModel` and placeholder `MapViewModel` (8×8 grid dummy).
- Bind simple view with placeholder sectors.
- Headless UI smoke test stellt sicher, dass `MainWindow` mit `MapViewModel` lädt; Debug-Build nutzt Runtime-XAML-Fallback für frühe Binding-Fehler.

## Task 6 – Dependency Injection

**Status:** ✅ Done – ServiceCollection richtet Daten-/Szenario-Services und ViewModels für die App ein (2025-09-28).

- Setup `ServiceCollection` in App.
- Register all services (IDataService, IScenarioService, …) with stub implementations where needed.
- ViewModels über DI auflösen; `MainWindow` erhält `MainViewModel` aus dem Container.

**Acceptance criteria:**

- App starts.
- New Game can be created.
- 8×8 placeholder map shown.
- Reference data loads successfully.

---

# Chaos Overlords – Phase 2 Tasks (Rundenlogik „Happy Path“)

> Ziel: Ein lauffähiger, deterministischer Grund-Loop ohne Kampf/Verstecken, mit Ökonomie/Hire, Befehls-Queue und sichtbarer Phasen-Timeline. Keine Detail-Mechanik-Tuning – nur Happy-Path.

## Task 7 – TurnViewModel & Phasen-State-Machine

**Status:** ✅ Done – Turn state machine with command timeline UI, command gating & unit tests (2025-09-28)**Beschreibung:** Implementiere eine schlanke Runden-State-Machine mit den Rundenphasen: Upkeep/Income → Command → Execution → Hire → Elimination. Abbildung der **Command-Subphasen** (Instant, Combat, Transaction, Chaos, Movement, Control) nur als *Timeline-Slots* im UI (noch keine vollumfängliche Auflösung).**Akzeptanzkriterien:**

- `TurnViewModel` verwaltet den Phasenfortschritt, kann eine Runde starten/beenden.
- Command-Subphasen werden im UI als Timeline angezeigt; Befehle erscheinen im passenden Slot.
- „Next Phase“-/„End Turn“-Buttons sind logisch gesperrt/freigeschaltet.
  **Hinweise (für Copilot):**
- Enum `TurnPhase { Upkeep, Command, Execution, Hire, Elimination }`.
- Enum `CommandPhase { Instant, Combat, Transaction, Chaos, Movement, Control }`.
- Eventing via `IEventAggregator`/Messenger (CommunityToolkit.Mvvm) für UI-Updates.

## Task 8 – RngService (deterministisch) & Turn-Event-Log

**Status:** ✅ Done – Seeded `IRngService` + turn event log wired through recorder/writer (2025-09-29)**Beschreibung:** Erzeuge einen zentralen `IRngService` (Seed im `GameState`). Lege ein Event-Log an (`TurnEvent`, Typ + Payload) und protokolliere wichtige Ereignisse (Upkeep/Income, Hire, Control-Gewinne/Verluste, geplante Crackdowns als Platzhalter).**Akzeptanzkriterien:**

- Identische Seeds → identische Ereignisse (Smoke-Test).
- „Last Turn Events“-Panel zeigt protokollierte Ereignisse der letzten Runde.
  **Hinweise:**
- `Random` über `Xorshift`/`System.Random` mit Seed; keine `new Random()`-Streuungen.
- Log nur *anhängen*; Rotation auf N Runden vorsehen.

## Task 9 – EconomyService (Upkeep & Income)

**Status:** ✅ Done – EconomyService applies upkeep/tax/site cash, logs snapshots, tests in place (2025-09-29)**Beschreibung:** Implementiere die Berechnung von Upkeep (Summe Gang-Upkeep), Sektorsteuer (+1 pro kontrolliertem Sektor), sowie Site-Cash-Modifier (positiv/negativ). Bribe/Snitch/Einkäufe werden *angekündigt*, fließen in die Vorschau ein (s. Task 11).**Akzeptanzkriterien:**

- Upkeep/Income werden in Upkeep-Phase angewendet und im Log erfasst.
- Negative Site-Cash-Werte reduzieren den Cash korrekt.
- Unit-Tests für typische Mischfälle (kein Kampf/Chaos erforderlich).
  **Hinweise:**
- Wertequellen: `SectorState` (Kontrolle), `SiteRef` (Cash-Delta), `Gang.Upkeep`.
- Spätere Erweiterung: Chaos-Einnahmen werden erst in Phase 3/5 aufgelöst.

## Task 10 – RecruitmentService (Pool & Hire)

**Status:** ✅ Done – Service + Hire-UI verdrahtet, deterministische Pool-Refreshes & Edge-Case-Tests abgeschlossen (2025-09-29)**Beschreibung:** Implementiere Rekrutierungspool (3 Optionen), Hiren/Ablehnen, Kostenabzug, Zufallsnachschub pro Runde. Platzhalter-KI: keine.**Akzeptanzkriterien:**

- In Hire-Phase erscheint ein Panel mit 3 Gangs.
- Hire zieht Cash ab und fügt `Gang` im gewählten Sektor hinzu.
- Ablehnen ersetzt die Option erst in der nächsten Runde.
  **Hinweise:**
- Pool persistent in `GameState`.
- Tests für: unzureichendes Cash, Doppelhiring gesperrt, Upkeep im Turn der Anwerbung.

## Task 11 – Command Queue & Resolver (Skeleton)

**Status:** ✅ Done – Command queue domain, resolver, and UI queue management delivered with unit tests (2025-09-29)**Beschreibung:** Pro Gang eine Aktion je Runde. Erzeuge Command-Objekte (Move, Control, Chaos – *Chaos nur gestubbt, keine Crackdown-Logik in Phase 2*). Execution-Resolver führt die Befehle in Command-Subphasenreihenfolge aus, ohne Kampf/Verstecken.**Akzeptanzkriterien:**

- Queue verhindert Mehrfachbefehle je Gang.
- Move versetzt legal (adjazente Felder), Control versucht Kontrolle neutraler Sektoren (vereinfachte Formel), Chaos erzeugt nur eine „Schätzung“ (kein Geldauskehr in Phase 2).
- UI zeigt Queue und Resultate im Event-Log.
  **Hinweise:**
- Control: verwende vereinfachte deterministische Formel (z. B. FORCE+CONTROL – SectorIncome – Support ≥ 1 → Erfolg). Exakte Formel wird in Phase 3/5 ersetzt.
- Chaos: nur `ProjectedChaos` als Zahl im Sektor speichern (für Task 11).

## Task 12 – Finance Preview (City & Sector)

**Status:** ✅ Done – FinancePreviewService liefert deterministische Projektion, TurnViewModel teilt City/Sector-Collections mit FinancePreviewSectionViewModel, UI styled für Einnahmen/Ausgaben (2025-09-30)

**Beschreibung:** UI-Panel nach Vorbild „Finance“ des Originals: Gegenüberstellung der erwarteten Zu-/Abflüsse der *nächsten* Runde. In Phase 2 ohne echte Chaos-Einnahmen, dafür mit „Chaos(Estimate)“.

**Akzeptanzkriterien:**

- City-Ansicht: Upkeep, New Recruits, Equipment (nur placeholder), City Officials (Bribes), Sector Tax, Site Protection, Chaos (Estimate), Cash Adjustment.
- Sector-Ansicht: gleiche Werte gefiltert auf einen Sektor.
- Rote Zahlen = Kosten, graue = Einnahmen.
  **Hinweise:**
- Bindings aus `EconomyService` und `c` pro Sektor.
- Wirtschaftliche Updates aus Task 9 im UI sichtbarer machen (z. B. Aggregation der Turn-Log-Einträge in einer Finance-Übersicht).
- Später: echte Chaos-Auszahlung & Crackdown (Phase 5).

## Task 13 – Phasen-Timeline UI (Befehlsvisualisierung)

**Status:** ✅ Done – Modularer CommandTimeline-Abschnitt aktualisiert sich über MessageHub-Snapshots, ViewModel trennt Anzeige/Logik, Styles markieren aktive/abgeschlossene Slots (2025-09-30)

**Beschreibung:** Timeline-Widget, das die 6 Command-Subphasen zeigt und pro Gang die gequeuten Befehle in den Slot legt (Drag&Drop-Reihenfolge optional).

**Akzeptanzkriterien:**

- Befehle erscheinen im korrekten Slot.
- Execution-Start animiert den Fortschritt durch die Slots.
- Accessibility: Tastaturfokus, Tooltips (später Odds).
  **Hinweise:**
- Re-usable Control; vorbereitet auf Odds/Icons in Phase 3–5.

## Task 14 – Map & Sector-Model: Klassen & Basistoleranz (Read-Only)

**Status:** 🟡 Planned**Beschreibung:** Ergänze `SectorState` um Sektorklasse (Lower…Upper) und Basistoleranz/Income als *Konstanten* oder *Config* (noch keine Snitch/Bribe-Dynamik).**Akzeptanzkriterien:**

- Sektorklasse wird in der Map angezeigt (dezente Badges/Legend).
- Basistoleranz/Income sind pro Klasse konfigurierbar und laufen in Economy/Preview ein.
  **Hinweise:**
- Werte als `appsettings.json` oder Data-JSON; finale Zahlenabstimmung in Phase 5.

---

# Priority 2 – Silver City Adaptation

> Ziel: Lore- & Copyright-konforme Neuinterpretation („Cons“) der Fraktionen, ohne Phase-1/2-Fortschritt zu blockieren. Umsetzung erfolgt nach Abschluss der Phase-2-Happy-Path-Meilensteine.

## Task S1 – Con Referenzdaten

**Status:** 🟡 Planned – JSON-Struktur und Domain-Modelle vorbereiten.

- `ConRef` + `ConModifiers` Modelle in Core.
- `IDataService`/`EmbeddedJsonDataService` um `cons.json` erweitern.
- Validierungstest für Referenzdaten.

## Task S2 – Con Auswahl & Szenariofilter

**Status:** 🟡 Planned – Auswahl im New-Game-Flow & Szenarioeinschränkungen.

- Player/`GameState` um Con-Zugehörigkeit erweitern.
- ScenarioService filtert Szenarien basierend auf `scenarioExclusions`.
- UI: Con-Picker + gesperrte Szenarien (Tooltip).

## Task S3 – Con Modifiers anwenden

**Status:** 🟡 Planned – Modifikatoren in Services verdrahten.

- Economy-, Command-, Movement-, Recruitment-Services um Con-Modifier ergänzen.
- Unique-Item-Pool (Stub) markieren.
- Tests für mind. drei Modifikatortypen.

## Task S4 – UI Darstellung & Feedback

**Status:** 🟡 Planned – HUD- und Tooltip-Erweiterungen.

- Con-Badge/Tooltip im HUD.
- Detailanzeige im Spielerstatus (aktive Boni, Sperren).
- Optional: Icons/Assets für Cons.

## Task S5 – Persistenz & QA

**Status:** 🟡 Planned – Save/Load & Stabilität.

- Save/Load-Service speichert Con-Zugehörigkeit + Einmal-Boni-Zustand.
- Integrationstests (New Game → Save → Load mit Con).
- Dokumentation in `Manual/` und Release Notes ergänzen.

---

## Out of Scope (Phase 2)

- Kampf, Verstecken/Aufspüren, detaillierte Einfluss-/Forschungsauflösung, Crackdown-Events, Polizei-KI, Item-Fertigung/Discounts.

## Review & Tests (Phase 2)

- Unit-Tests: Economy, Hire, deterministischer RNG, einfacher Control-Erfolg.
- Smoke-Test: Start → Commands → Execution → Hire → nächste Runde.
- UI-Checks: Timeline sichtbar, Finance Preview korrekt, Events geloggt.
