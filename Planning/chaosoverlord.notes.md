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

## Offene Punkte / Beobachtungen

- Remote `origin` ist eingerichtet (`feature/*` → GitHub); Branch-Strategie weiterhin: Feature-Branches von `dev`, Phase-Abschlüsse mergen nach `main`.
- Task 14 (Sektorklassen/Basistoleranz) steht noch aus; Finance Preview zeigt bis dahin neutrale Werte.

## Task 4 – Out-of-Scope Elemente
- Silver City Cons (Priority 2): Umsetzung folgt nach Phase-2-Happy-Path; kein Major Rewrite erforderlich, Erweiterungen bleiben additiv.

- KI-Logik für `AiPlayer` sowie echte Entscheidungsbäume folgen erst in Phase 2 (nur `ExecuteTurnAsync`-Placeholder vorhanden).
- `NetworkPlayer` repräsentiert lediglich den Slot-Typ; Synchronisation/Netzwerk-Handshake wird später geplant.
- UI-Anbindung des `GameStateManager` (Turn-Steuerung, Timeline) liegt außerhalb von Phase 1 Task 4 und wird in den Phasenaufgaben adressiert.
- Persistenz und Save/Load des erzeugten `GameState` ist nicht Bestandteil von Task 4; aktuelle Implementierung arbeitet rein im Speicher.

## To-Validate / Ideen

- Ergänzende Checks in CI (z. B. Avalonia Linter, Code-Coverage-Upload) evaluieren, sobald erste Feature-Implementierungen vorliegen.
- Notwendige Dokumentationen für Designer/Art (Mockups, Styleguide) separat strukturieren, wenn UI-Assets vorliegen.
