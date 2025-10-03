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

**Status:** ✅ Done – Sector economics driven by site metadata; map/finance panels reflect site income & tolerance (2025-10-01).

**Beschreibung:** Ergänze `SectorState` um Sektorklasse (Lower…Upper) und Basistoleranz/Income als *Konstanten* oder *Config* (noch keine Snitch/Bribe-Dynamik).

**Akzeptanzkriterien:**

- Sektorklasse wird in der Map angezeigt (dezente Badges/Legend).
- Basistoleranz/Income sind pro Klasse konfigurierbar und laufen in Economy/Preview ein.

**Resultat:** Klassendefinitionen wurden auf site-getriebene Statistikwerte umgestellt. Jeder Sektor besitzt nun deterministischen `SiteData`-Seed (inkl. Income/Tolerance). MapView zeigt Site-Namen direkt, Tooltips listen sämtliche Modifikatoren. Economy- und Finance-Services lesen die Werte aus den Sites, wodurch Placeholder-Klassendaten obsolet sind.

---

# Chaos Overlords – Phase 3 Tasks (Kernaktionen)

> Ziel: Die zentralen Spieleraktionen (Influence, Research, Equip, erweiterte Bewegung) implementieren, einschließlich Würfel-/Check-Utilities und deterministischer Logging-Pipeline. Fokus auf Happy-Path ohne komplexe Gegenreaktionen.

## Task 15 – Action Resolution Framework & Dice Utilities

**Status:** ✅ Done – ActionContext/ActionResult-Modelle, Dice-Rolls im IRngService und Turn-Log-Ausgabe implementiert; Edge-Case-Tests (Auto-Thresholds, Mod-Aggregation) ergänzt; Control-Command schreibt strukturierte Action-Logs (2025-10-01).

- Implementiere ein generisches `ActionContext`/`ActionResult`-Modell inklusive Erfolgs-/Fehlschlag-Enums.
- Ergänze `IRngService` um Prüfmethoden (z. B. `RollPercent`, `RollDice`), die deterministisch logging-fähige Würfelwürfe liefern.
- Stelle Hilfsklassen bereit (z. B. `ActionDifficulty`, Modifikatoren) und schreibe Unit-Tests für Grenzfälle (Min/Max, automatische Erfolge/Fehlschläge).
- Log-Ausgabe (Turn Event Log) erhält strukturierte Einträge mit Würfelwerten und Modifikatoren.
- **Zwischenstand 2025-10-01:** Framework-Basistypen + Tests vorhanden, Control-Command schreibt nun strukturierte Action-Logs; weitere Commands folgen in Tasks 16/17.

## Task 16 – Movement & Map Interaction Upgrade

**Status:** ✅ Done – Bewegung gemäß Manual umgesetzt (8‑Nachbarschaft, Kapazitätsgrenze), UI‑Highlighting und deterministische Logs vorhanden (2025-10-02).

- Move-Command bleibt 1 Schritt in einen angrenzenden Sektor (Manual: “Move... Moves gang to an adjacent sector. Shortcut: drag in 9-sector display”).
  Adjazenz umfasst orthogonal UND diagonal (8-neighborhood, konsistent mit 9-Sektor-Display).
  Keine Multi-Step-Pfade in einer Ausführung.
- Kapazität: Max. 6 eigene Gangs pro Sektor (Manual). Bewegung in volle Sektoren ist ungültig.
- Füge Karteninteraktionen hinzu: Offenlegen von Sector-Details beim Betreten, Aktualisierung von Fog/Intel-Platzhaltern.
- Aktualisiere CommandResolver + Tests für neue Pfadvalidierungen.
- UI: MapView markiert legale Ziele; Timeline/Event-Log spiegelt neue Bewegungsresultate.

Hinweise:
- Phase 3 Scope: kein Kampf/Stealth; nur Bewegung, Regeln für Zielgültigkeit und deterministische Logs.
- Tests für: Adjazenz (8-neighborhood verankert), volle Sektoren, und illegale Ziele.

## Task 17 – Influence Actions (Control & Support Shifts)

**Status:** ✅ Done – Influence end‑to‑end inkl. Queue/Resolver, RNG‑Integration, UI‑Wiring, Previews und Tests (2025-10-02).

- Implementiere Influence-Command (z. B. Propaganda/Bribe) basierend auf Gang-Werten, Sector-Tolerance und Site-Support.
- Verwende ActionFramework/Dice Utilities; Erfolg modifiziert Sector-Kontrolle oder Support besser/weniger tolerant.
- Finance Preview berücksichtigt kurzfristige Einflusskosten und künftige Steueränderungen.
- Tests decken Erfolg, Fehlschlag, kritische Ergebnisse sowie Event-Log-Einträge ab.

## Task 18 – Research & Equipment Management

**Status:** ✅ Done – Grundfunktionen und UI-Ansätze implementiert (2025-10-03).

- Fabrication-Flow eingeführt (Queue + Resolver), Finance Preview berücksichtigt Fabrication-Kosten.
- Research-UI verbessert: AutoComplete mit „Name (Cost)“, Research-Kategorie in Finance Preview sichtbar.
- EquipmentService: Equip/Unequip/Give/Sell implementiert; Kapazitätslimit pro Gang (2 Items) und Validierungen.
- Tests: Fabrication, Give/Sell, Finance Research-Linie + Kapazitätsgrenze grün.

## Task 19 – Finance HUD + City Financial Dialog

**Status:** 🟡 Planned – UI-Oberflächen auf bestehende Projektion aufsetzen.

- CO.FinanceHUDIndicator im Footer zeigt Net +/- der nächsten Runde; Klick öffnet Dialog.
- CO.CityFinancialDialog listet Kategorien (Upkeep, Recruits, Research, Equipment, Officials, Sector Tax, Site Protection, Chaos Estimate, Cash Adjustment) mit Summen.
- Anbindung an bestehenden IFinancePreviewService; minimaler Drilldown.
- Tests/Checks: Bindings fehlerfrei (keine Binding-Errors), simple VM-Snapshot-Test.

## Task 20 – Event Feed + Last Turn Events Dialog

**Status:** 🟡 Planned – Sichtbare Rückmeldungen der Turn-Events.

- CO.EventFeedPanel als nicht-blockierender Panel (rechts/links) mit klickbaren Events.
- Optional: CO.LastTurnEventsDialog als modal am Rundenende.
- Adapter vom Turn-Event-Writer auf eine in-memory Collection (deterministisch, seed-stabil).
- Tests/Checks: Einträge erscheinen nach Execution; Klick wählt Sektor (falls referenziert).

### Nächste Schritte (Backlog)
- `appsettings.{Environment}.json`‑Overlays und README‑Dokumentation ergänzen.
- Auto‑Scroll pausieren, wenn der Nutzer nach oben scrollt; Wiederaufnahme, wenn an das Ende gesprungen wird.
- Optional: Log‑Rotation nach Größe/Zeit; robustere IO‑Fehlerbehandlung beim Trimmen.
- Previews weiter formatieren (Icons/Farben), Odds‑Tooltips vorbereiten.

---

# Chaos Overlords – Phase 4 Tasks (Research, Equipment & City Officials)

> Ziel: Kernökonomie und Inventarfluss komplettieren; Instant/Transaction‑Slots beleben.

## Task 21 – Equipment Give/Sell Dialogs

**Status:** 🟡 Planned – Dünne Dialogs über bestehende Services.

- CO.EquipmentGiveDialog: Inventar → Zielgang, Validierungen (Kapazität/Tech) via IEquipmentService.
- CO.EquipmentSellDialog: Liste verkaufbarer Items + Erlöse; Bestätigung.
- Reuse bestehender Services/Validierungen; kleine Item/Gang VMs.
- Tests/Checks: UI-Bindings und Fehlerzustände über einfache VM-Tests.

## Task 22 – ItemDetailPanel mit Delta-Vorschau

**Status:** 🟡 Planned – Wiederverwendbares Delta-Panel.

- Kompakter View zeigt aktuelle vs. mit-Item Werte (+X/−Y) farblich.
- Integration in Give/Sell und später Purchase/Research.
- Erfordert konsistente Effektprojektion in VM.

## Task 23 – Research UI Polish

**Status:** 🟡 Planned – Kleines UI-Feintuning.

- Case-insensitive Filter für AutoComplete; Label mit Progress/Turns (IResearchService Preview).
- Optional einfacher Research-Dialog mit Liste + Fortschritt.

## Task 24 – Right Panel Shell (Actions)

**Status:** 🟡 Planned – Einstiegsfläche für Aktionen.

- CO.RightPanelView mit Buttons (Events, City, Financial, Gangs, Ranking, Done) und optional einklappbarem EventFeed.
- Buttons öffnen die Dialoge/Panele aus Tasks 19–23.

## Task 25 – Research Service & Instant Command

**Status:** 🟡 Planned – deterministischer Fortschritt & Caps.

- Implementiere `IResearchService` mit Projekten, Progress und Caps; Site‑Boni (z. B. Lab) einrechnen.
- CommandQueue/Resolver: Research als Instant; Preview zeigt Projekt, Kosten, erwarteten Fortschritt.
- Tests: deterministische Seeds, Cap‑Erreichung, Multi‑Turn‑Progress.

## Task 26 – City Officials (Bribe/Snitch) Instant Commands

**Status:** 🟡 Planned – Equip/Unequip/Give/Sell mit Slots/Tech.

- Implementiere `IEquipmentService` (Equip/Unequip/Give/Sell); Slots (Weapon/Armor/Misc), TechLevel‑Gates, doppelte/inkompatible Items verhindern.
- Pricing mit Site‑Discounts (Factories/Markets); Events/Fehlertexte.
- Finance Preview integriert Käufe/Verkäufe; UI‑Inventory‑Panel.
- Tests: Slot‑Validierung, Discounts, Preis‑Rundung, Give/Sell‑Kantenfälle.

## Task 27 – Influence Method Options (optional)

**Status:** 🟡 Planned – klare Kosten/Effekte, keine Police‑KI.

- Definiere Bribe/Snitch als Instant (Tolerance ±3; fixer Cash‑Abzug, z. B. 3); deterministisch, Seed‑stabil.
- Naming‑Klarstellung vs. Influence: City „Bribe“ bleibt, Influence‑Variante ggf. „Payoff“.
- Finance Preview zeigt Officials‑Kosten; Tests für Kettenaktionen und Ökonomieeffekte.

## Task 28 – UI & Logging Integration für Kernaktionen

**Status:** 🟡 Planned – Sichtbarkeit der neuen Aktionen sicherstellen.

- CommandTimeline und TurnManagement-Panel erhalten Badges für Influence/Research/Equip-Aktionen.
- Map/Recruitment/Finance-Views zeigen Ergebnisse (z. B. neue Boni, laufende Projekte) mit Tooltips.
- TurnEventLog fasst Kernaktionen pro Phase zusammen; Datei‑Logging mit Retention aktiv.
- Smoke-Test-Szenario, das einen kompletten Kernaktions-Zyklus durchläuft.

**Status:** 🟡 Planned – Propaganda/Payoff als auswählbare Methode.

- enum `InfluenceMethod { Propaganda, Payoff }`; UI‑Dropdown; Resolver/Preview mit Formeln.
- Tests: Beide Methoden, deterministisch, Previews korrekt.

---

# Chaos Overlords – Phase 5 Tasks (Kampf & Verstecken)

> Ziel: Kampf- und Stealth-Mechaniken inklusive UI-Feedback, basierend auf Phase‑4‑Ökosystem.

## Task 29 – Combat Resolution Engine

… (Inhalte wie bisher Task 20, verschoben)

## Task 30 – Hide & Search Mechanics

… (Inhalte wie bisher Task 21, verschoben)

## Task 31 – Combat & Stealth UI

… (Inhalte wie bisher Task 22, verschoben)

## Task 32 – Event Log & Economy Integration

… (Inhalte wie bisher Task 23, verschoben)

---

# Out of Scope (Phase 2)

- Kampf, Verstecken/Aufspüren, detaillierte Einfluss-/Forschungsauflösung, Crackdown-Events, Polizei-KI, Item-Fertigung/Discounts.

# Review & Tests (Phase 2)

- Unit-Tests: Economy, Hire, deterministischer RNG, einfacher Control-Erfolg.
- Smoke-Test: Start → Commands → Execution → Hire → nächste Runde.
- UI-Checks: Timeline sichtbar, Finance Preview korrekt, Events geloggt.
