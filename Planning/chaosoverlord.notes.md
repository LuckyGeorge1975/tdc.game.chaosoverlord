# Chaos Overlords – Development Notes

> Dokumentiert Entscheidungen, Erkenntnisse und offene Fragen, die nicht direkt aus den Phasen- und Task-Plänen hervorgehen.

## Entscheidungsverlauf

| Datum | Thema | Entscheidung | Begründung / Auswirkungen |
| --- | --- | --- | --- |
| 2025-09-27 | Projektstruktur | `Directory.Build.props` und `Directory.Packages.props` bündeln Framework-/Package-Versionen zentral. | Reduziert Wartungsaufwand, stellt Konsistenz über alle Projekte sicher. |
| 2025-09-27 | Paketversionen | Alle Avalonia- und Testpakete auf aktuellste stabile Versionen angehoben. | Sorgt für aktuelle Bugfixes/Features; CI nutzt standardmäßiges .NET 9 SDK. |
| 2025-09-27 | CI/CD | GitHub Actions Workflow `Build & Test` führt Restore/Build/Test für `main` und `dev` aus. | Stellt Qualität vor Merges sicher; Grundlage für spätere Deployment-Schritte. |
| 2025-09-27 | Referenzdaten | Platzhalter-JSONs und Schwarz/Weiß-SVGs für Gangs/Items/Sites angelegt (`ChaosOverlords.Data/Reference`). | Erlaubt Task 2 mit realem Loader, bis finale Assets verfügbar sind. |
| 2025-09-27 | Referenzdaten-Service | JSON-Dateien als Embedded Resources eingebunden, `EmbeddedJsonDataService` liefert stark typisierte Datensätze. Item-Type-Mapping aktuell mit Annahme 0=Melee, 1=Blade, 2=Ranged, 3=Armor, 4=Misc dokumentiert. | Daten können ohne Dateisystemzugriff geladen werden; Mapping wird bei finalem Balancing validiert. |
| 2025-09-28 | Silver City Cons Roadmap | Silver-City-Anpassungen als Priority-2-Paket (Phase 7 + Tasks S1–S5) geplant; Umsetzung nach Phase-2-Happy-Path. | Architekturänderungen nicht nötig – bestehende Services (Economy, Commands, Save/Load) werden sukzessive erweitert. |
| 2025-09-28 | Avalonia App Smoke Coverage | Debug-Build setzt Runtime-XAML-Fallback ein; Headless UI Smoke-Test prüft MainWindow/MapViewModel-Bootstrap. | Stellt sicher, dass Binding-Fehler früh erkannt werden und Startup-Bugs automatisiert auffallen. |
| 2025-09-28 | Dependency Injection Setup | App nutzt `ServiceCollection`, registriert Daten-/Szenario-Services und ViewModels. | Konsistentes Bootstrapping; erleichtert Tests und spätere Service-Erweiterungen. |
| 2025-09-28 | Turn State Machine Scope | TurnViewModel + Timeline zeigen Phasenfortschritt; Command-Slots visualisieren Status, konkrete Commands folgen in Task 11. | Erlaubt frühe UI-Validierung ohne Vorgriff auf Command Resolver; End-Turn-Enablement via Unit-Test abgesichert. |
| 2025-09-29 | Command Queue Resolver & UI Binding | CommandQueue/Resolution services implementiert, TurnPhaseProcessor integriert; XAML-Remove-Binding bleibt mit Kommentar und `x:CompileBindings="False"`, da der Parent-Kontext nur zur Laufzeit vorliegt. | Sicherer Skeleton-Resolver für Move/Control/Chaos bereit; UI-Binding-Dokumentation vermeidet zukünftige Irrtümer. |
| 2025-09-30 | Turn Dashboard Modularisierung | TurnViewModel teilt Funktionen auf sechs Section-ViewModels auf; zentraler `IMessageHub` synchronisiert Timeline/Turn Summary ohne harte Dependencies. | Verbessert Testbarkeit & Wiederverwendung, neue Panels (Timeline, Finance Preview, Commands) abonnieren nur benötigte Daten. |
| 2025-09-30 | Finance Preview Architektur | `IFinancePreviewService` aggregiert deterministische Vorschau-Daten (City/Sector) aus GameState; UI nutzt gemeinsame ObservableCollections. | Klarer Ort für künftige Modifier (Cons, Items), vermeidet Duplikation der Berechnungslogik im UI. |
| 2025-10-01 | Sektor-Klassen vs. Site-Daten | Klassenspezifische Basistoleranz wurde durch vollständig site-basierte Werte ersetzt. `ScenarioService` weist jedem Sektor deterministisch `SiteData` zu; MapView + Tooltips zeigen Name/Modifier, Economy & Finance lesen direkt aus den Site-Werten. | Reduziert Redundanz (keine getrennten Klassen-Tabellen nötig), garantiert, dass alle 64 Sektoren spielbereit sind und deterministische Seeds bleiben erhalten. |
| 2025-10-01 | Phase-3/4 Roadmap | Phase-3-Aufgaben (ActionFramework, Movement/Influence/Research/Equip) und Phase-4-Aufgaben (Combat/Hide/Search) wurden detailliert geplant. Tasks 15–23 dienen als Leitplanken für deterministische Kernaktionen und kommende Kampfmechaniken. | Sichert, dass der Übergang von Happy-Path zu Kernaktionen fokussiert bleibt und UI/Logging-Auswirkungen früh berücksichtigt werden. |
| 2025-10-01 | Action Resolution Framework Start | `ActionContext`/`ActionResult` plus Dice Utilities (`RollPercent`, `RollDice`) im `IRngService` eingeführt; TurnEventWriter schreibt strukturierte Action-Logs. Control-Command nutzt das Framework bereits, weitere Aktionen folgen in Tasks 16–19. | Legt deterministisches Fundament für Movement/Influence/Research/Equip, ermöglicht Debugging über Würfel-Logging und konsistente Tests für Erfolgs-/Fehlschlagpfade. |

## Offene Punkte / Beobachtungen

- Remote `origin` ist eingerichtet (`feature/*` → GitHub); Branch-Strategie: Feature-Branches von der aktiven Phasen-Branch (z. B. `P3_CoreActions`), Phase-Abschlüsse mergen nach `main`.
- Chaos-Payout bleibt ein Platzhalter; echte Auszahlung + Crackdown-Hooks werden in Phase 3 adressiert.
- Prüfen, ob zusätzliche Integrationstests nach Umsetzung des ActionFrameworks benötigt werden (Phase 3 Task 19).

## Würfel-Notation: Prozentwurf ↔ d6-Erfolge

Ziel: Balancing konsistent halten, egal ob eine Aktion als Prozentwurf (1–100) oder als d6-Erfolgsprobe (x Würfel auf 5+/6+) gedacht ist.

- Referenzmodell: d6-Erfolge mit Schwelle 5+ (≈ 33,33%/Würfel) oder 6+ (≈ 16,67%/Würfel).
- Prozentile-Check im Code: `IRngService.RollPercent()` gegen effektive Erfolgswahrscheinlichkeit p.
- Abbildung: Für n Würfel mit Erfolgschance q pro Würfel (z. B. q=1/3 bei 5+) ergibt sich Erfolgswahrscheinlichkeit p = 1 - (1 - q)^n.
	- Beispiele:
		- 1W6@5+: p ≈ 33%
		- 2W6@5+: p ≈ 56%
		- 3W6@5+: p ≈ 70%
		- 1W6@6+: p ≈ 16,7%
		- 2W6@6+: p ≈ 30,6%
- Kalibrierung im ActionFramework: Modifikatoren verändern n (effektive Würfel) oder q (Schwelle) nicht direkt, sondern p. Wenn das Design in „Würfel + Zielzahl“ denkt, rechne n,q → p und setze die Prozentchance entsprechend.
- Logging: ActionResult speichert sowohl p (in %) als auch den geworfenen Prozentwert; für Debugging kann optional n,q notiert werden, wenn die Aktion aus einer d6-Notation stammt.

Hinweis: Für sehr kleine/hohe p greift die automatische Erfolg/Fehlschlag-Schwelle (z. B. p ≤ 1% ⇒ auto fail, p ≥ 99% ⇒ auto success), damit Edge-Cases deterministisch und testbar bleiben.

## Task 4 – Out-of-Scope Elemente
- Silver City Cons (Priority 2): Umsetzung folgt nach Phase-2-Happy-Path; kein Major Rewrite erforderlich, Erweiterungen bleiben additiv.

- KI-Logik für `AiPlayer` sowie echte Entscheidungsbäume folgen erst in Phase 2 (nur `ExecuteTurnAsync`-Placeholder vorhanden).
- `NetworkPlayer` repräsentiert lediglich den Slot-Typ; Synchronisation/Netzwerk-Handshake wird später geplant.
- UI-Anbindung des `GameStateManager` (Turn-Steuerung, Timeline) liegt außerhalb von Phase 1 Task 4 und wird in den Phasenaufgaben adressiert.
- Persistenz und Save/Load des erzeugten `GameState` ist nicht Bestandteil von Task 4; aktuelle Implementierung arbeitet rein im Speicher.

## To-Validate / Ideen

- Ergänzende Checks in CI (z. B. Avalonia Linter, Code-Coverage-Upload) evaluieren, sobald erste Feature-Implementierungen vorliegen.
- Notwendige Dokumentationen für Designer/Art (Mockups, Styleguide) separat strukturieren, wenn UI-Assets vorliegen.
