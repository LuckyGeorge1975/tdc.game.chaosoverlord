# Chaos Overlords Remake

## Überblick
Remake des 1996 erschienenen Strategiespiels *Chaos Overlords* auf Basis von **.NET 9** und **Avalonia**. Ziel ist eine moderne, testbare Reimplementation mit klarer Trennung von Spiel-Logik (`ChaosOverlords.Core`), UI (`ChaosOverlords.App`) und Datenhaltung (`ChaosOverlords.Data`). Die Rundenlogik folgt einem deterministischen Happy Path, orchestriert über klar abgegrenzte Services (RNG, Economy, Turn Processing) und ein MVVM-getriebenes UI.

## Aktueller Funktionsumfang (Phase 2 – Stand Task 9)
- Phase 1 vollständig abgeschlossen: Daten-Layer, Domain-Modelle, Szenario-Bootstrap, Avalonia-App-Shell & Dependency Injection.
- `TurnViewModel` inklusive Phasen-State-Machine und Command-Timeline-Visualisierung (Task 7).
- Seeded `IRngService` mit Turn-Event-Log (Writer/Recorder) für deterministische Abläufe und Debugging (Task 8).
- `EconomyService` wendet Upkeep, Sektorsteuern und Site-Modifikatoren an und protokolliert per Spieler-Snapshot im Turn-Log (Task 9).

## Technologie-Stack
- .NET 9 (C#)
- Avalonia UI 11.3
- CommunityToolkit.Mvvm für MVVM, Messenger und Commands
- Microsoft.Extensions.DependencyInjection als Composition Root
- xUnit, Avalonia.Headless.XUnit & Coverlet für automatisierte Tests
- GitHub Actions für CI (Restore → Build → Test)

## Projektstruktur
```
src/
	ChaosOverlords.App/      # Avalonia-Client (Views, ViewModels, App Bootstrap)
	ChaosOverlords.Core/     # Domain-Modelle, Services, GameState & Turn-Processing
	ChaosOverlords.Data/     # Data Layer mit eingebetteten JSON-Ressourcen (Gangs, Items, Sites)
	ChaosOverlords.Tests/    # xUnit- und Headless-UI-Tests gegen Core & App
Planning/                  # Projektplanung, Tasks, Phasen, Notizen
Manual/                    # Referenz-Handbuch und Spielfluss-Dokumentation
Story/                     # Lore- und Con-Entwürfe (Silver City)
.github/                   # CI-Workflows & Copilot-Richtlinien
Directory.Build.props      # Zentrale Build-Einstellungen
Directory.Packages.props   # Zentrale Paketversionierung
```

## Voraussetzungen
- Windows, macOS oder Linux mit installiertem .NET 9 SDK (GA)
- Optional: VS Code mit den empfohlenen Erweiterungen aus `.vscode/extensions.json`

## Schnellstart
```powershell
git clone https://github.com/LuckyGeorge1975/tdc.game.chaosoverlord.git
cd tdc.game.chaosoverlord
git checkout P2_HappyPath    # Aktuelle Phase-Branch als Integrationsbasis
dotnet restore src/ChaosOverlords.sln
dotnet build src/ChaosOverlords.sln --configuration Release
dotnet test src/ChaosOverlords.sln --configuration Release
```

Für den UI-Start (Placeholder-Avalonia-Anwendung):
```powershell
dotnet run --project src/ChaosOverlords.App/ChaosOverlords.App.csproj
```

## Branch- & Workflow-Strategie
- `main`: Release-Branch, erhält Merges nach Abschluss einer Phase (Definition of Done in `Planning/chaosoverlord.phases.dod.md`).
- Phasen-Branches: `P<phase>_<beschreibung>` (z. B. `P2_HappyPath`) dienen als Integrationsbasis für alle Tasks einer Phase.
- Feature-Branches: `P<phase>_T<task>_<beschreibung>`; werden in die passende Phasen-Branch gemergt, sobald alle Task-DoD-Kriterien (`Planning/chaosoverlord.task.dod.md`) erfüllt sind.
- CI: GitHub Actions (`.github/workflows/ci.yml`) laufen auf `main`, aktiven Phasen-Branches und allen PRs Richtung der jeweiligen Phasen-Branch.

## Dokumentation & Artefakte
- Architektur & Layering: `Planning/chaosoverlord.architecture.md`
- Aufgaben & Phasen: `Planning/chaosoverlord.tasks.md`, `Planning/chaosoverlord.phases.md`
- Definition of Done: `Planning/chaosoverlord.task.dod.md`, `Planning/chaosoverlord.phases.dod.md`
- Fortschrittstracking: `Planning/chaosoverlord.progress.md`
- Entwicklungsentscheidungen & Annahmen: `Planning/chaosoverlord.notes.md`
- Lore & Referenzen: `Manual/`, `Story/`

Alle neuen Entscheidungen, Risiken oder Abweichungen sollten unmittelbar in `chaosoverlord.notes.md` dokumentiert werden, damit der Wissensstand nachvollziehbar bleibt.

## Contribution Guide (Kurzfassung)
1. Branch von `dev` erstellen (`P<phase>_T<task>_<beschreibung>` oder `feature/<phase>-<task>-<beschreibung>`).
2. Aufgabe gemäß Taskbeschreibung umsetzen; Tests/Docs aktualisieren.
3. Vor dem Commit lokal ausführen: `dotnet restore`, `dotnet build`, `dotnet test`.
4. Task-Status und Notizen aktualisieren (`chaosoverlord.progress.md`, `chaosoverlord.notes.md`).
5. Pull Request nach `dev` erstellen, CI prüfen, Reviews adressieren.

## Kontakt & Support
Fragen oder Ideen bitte im Issue-Tracker des Repositories erfassen oder über PRs einbringen. Weitere Projektvisionen und Roadmap stehen in `Planning/chaosoverlord.project.md` und den zugehörigen Planungsdokumenten.