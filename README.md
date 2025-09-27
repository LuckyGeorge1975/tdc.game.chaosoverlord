# Chaos Overlords Remake

## Überblick
Remake des 1996 erschienenen Strategiespiels *Chaos Overlords* auf Basis von **.NET 9** und **Avalonia**. Ziel ist eine moderne, testbare Reimplementation mit klarer Trennung von Spiel-Logik (`ChaosOverlords.Core`), UI (`ChaosOverlords.App`) und Datenhaltung (`ChaosOverlords.Data`).

## Technologie-Stack
- .NET 9 (C#)
- Avalonia UI 11.3
- xUnit + Coverlet für Tests
- GitHub Actions für CI (Restore → Build → Test)

## Projektstruktur
```
src/
	ChaosOverlords.App/      # Avalonia-Client (Views, ViewModels, App Bootstrap)
	ChaosOverlords.Core/     # Domain-Modelle, Services, GameState
	ChaosOverlords.Data/     # Data Layer, JSON-Ressourcen (tba)
	ChaosOverlords.Tests/    # xUnit-Tests gegen Core
Planning/                  # Projektplanung, Tasks, Phasen, Notizen
Manual/                    # Referenz-Handbuch und Spielfluss-Dokumentation
.github/workflows/ci.yml   # Build & Test Pipeline
Directory.Build.props      # Zentrale Build-Einstellungen
Directory.Packages.props   # Zentrale Paketversionierung
```

## Voraussetzungen
- Windows, macOS oder Linux mit .NET 9 SDK (GA) installiert
- Optional: VS Code mit den empfohlenen Erweiterungen aus `.vscode/extensions.json`

## Schnellstart
```powershell
git clone https://github.com/LuckyGeorge1975/tdc.game.chaosoverlord.git
cd tdc.game.chaosoverlord
git checkout dev
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
- `dev`: Integrations-Branch für laufende Arbeiten.
- Feature-Branches: `feature/<phase>-<task>-<beschreibung>`; werden in `dev` gemergt, sobald alle Task-DoD-Kriterien (`Planning/chaosoverlord.task.dod.md`) erfüllt sind.
- CI: GitHub Actions (`.github/workflows/ci.yml`) laufen auf `main`, `dev` und allen PRs Richtung `dev`.

## Dokumentation & Artefakte
- Aufgaben & Phasen: `Planning/chaosoverlord.tasks.md`, `Planning/chaosoverlord.phases.md`
- Definition of Done: `Planning/chaosoverlord.task.dod.md`, `Planning/chaosoverlord.phases.dod.md`
- Fortschrittstracking: `Planning/chaosoverlord.progress.md`
- Entwicklungsentscheidungen & Annahmen: `Planning/chaosoverlord.notes.md`
- Originalspiel-Referenz: Files im Ordner `Manual/`

Alle neuen Entscheidungen, Risiken oder Abweichungen sollten unmittelbar in `chaosoverlord.notes.md` dokumentiert werden, damit der Wissensstand nachvollziehbar bleibt.

## Contribution Guide (Kurzfassung)
1. Branch von `dev` erstellen (`feature/<phase>-<task>-<beschreibung>`).
2. Aufgabe gemäß Taskbeschreibung umsetzen; Tests/Docs aktualisieren.
3. `dotnet restore`, `dotnet build`, `dotnet test` vor dem Commit lokal ausführen.
4. Task-Status und Notizen aktualisieren (`chaosoverlord.progress.md`, `chaosoverlord.notes.md`).
5. Pull Request nach `dev` erstellen, CI prüfen, Reviews adressieren.

## Kontakt & Support
Fragen oder Ideen bitte im Issue-Tracker des Repositories erfassen oder über PRs einbringen. Weitere Projektvisionen und Roadmap stehen in `Planning/chaosoverlord.project.md` und den zugehörigen Planungsdokumenten.