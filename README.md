# Chaos Overlords Remake

## Überblick
Remake des 1996 erschienenen Strategiespiels *Chaos Overlords* auf Basis von **.NET 9** und **Avalonia**. Ziel ist eine moderne, testbare Reimplementation mit klarer Trennung von Spiel-Logik (`ChaosOverlords.Core`), UI (`ChaosOverlords.App`) und Datenhaltung (`ChaosOverlords.Data`). Die Rundenlogik folgt einem deterministischen Happy Path, orchestriert über klar abgegrenzte Services (RNG, Economy, Turn Processing) und ein MVVM-getriebenes UI.

## Aktueller Funktionsumfang (Phase 2 abgeschlossen, Phase 3 in Arbeit)
- Phase 1 & 2: vollständig abgeschlossen (Daten-Layer, Domain-Modelle, Szenario-Bootstrap, modulare Turn-Dashboard-Abschnitte inkl. Finance Preview & Timeline).
- Phase 3: Action/Dice-Utilities (Task 15) sowie Kernaktionen Movement (Task 16) und Influence (Task 17) sind umgesetzt und getestet.
	- Movement: 8‑Nachbarschaft (orthogonal+diagonal), Kapazitätsgrenze 6 eigene Gangs pro Sektor, Ausführung in Execution‑Phase, deterministische Logs.
	- Influence: End‑to‑End inkl. Queue/Resolver, deterministische Checks via `IRngService`, ausführliche Action‑Events; permissive Queue‑Validierung, autoritative Ausführung.
- Logging & UI‑Polish: Datei‑basiertes Turn‑Event‑Logging mit `IOptions`‑Konfiguration, Log‑Retention, „Open Logs Folder“‑Button, Auto‑Scroll‑Toggle im Events‑Panel.
- Preview & Map: Control/Influence‑Preview im UI; kontrollierte Sektoren werden auf der Karte farblich markiert.
- `EconomyService` & `FinancePreviewService`: deterministische Upkeep-/Preview‑Daten; Recruitment‑Service verwaltet den Hire‑Pool inkl. Tests.

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
git checkout P3_CoreActions  # Aktive Phase-3-Branch als Integrationsbasis
dotnet restore src/ChaosOverlords.sln
dotnet build src/ChaosOverlords.sln --configuration Release
dotnet test src/ChaosOverlords.sln --configuration Release
```

Für den UI-Start (Placeholder-Avalonia-Anwendung):
```powershell
dotnet run --project src/ChaosOverlords.App/ChaosOverlords.App.csproj
```

## Konfiguration & Logging

- App‑Konfiguration: `appsettings.json` wird beim App‑Start geladen; `LoggingOptions` wird via `IOptions` gebunden (mit optionalen Umgebungsüberlagerungen).
- Turn‑Event‑Logging (Datei + In‑Memory):
	- Properties: `Enabled` (bool), `LogDirectory` (string, absolut oder relativ), `FileNamePrefix` (string, Default `turn`), `MaxRetainedFiles` (int ≥ 0).
	- Pfadauflösung: Leerer/relativer Pfad wird über einen `ILogPathProvider` in einen absoluten Pfad aufgelöst; existiert der Ordner nicht, wird er erstellt.
	- Retention: Es werden nur die jüngsten `MaxRetainedFiles` Dateien mit passendem Prefix behalten (ältere werden beim Start bestmöglich entfernt).
	- Sharing: Logdateien werden so geöffnet, dass paralleles Lesen möglich ist (Read/Write‑Sharing); das Events‑Panel spiegelt Einträge in Echtzeit.
- UI‑Komfort:
	- „Open Logs Folder“‑Button im Turn‑Events‑Panel öffnet den Log‑Ordner plattformgerecht.
	- Auto‑Scroll‑Toggle hält die Liste bei neuen Einträgen am Ende; bei Bedarf kann Auto‑Scroll deaktiviert werden.

Tests: Es existiert ein file‑backed Logging‑Test, der Header‑Zeile und Event‑Append verifiziert (`FileTurnEventWriterTests`).

## Branch- & Workflow-Strategie
- `main`: Release-Branch, erhält Merges nach Abschluss einer Phase (Definition of Done in `Planning/chaosoverlord.phases.dod.md`).
- Phasen-Branches: `P<phase>_<beschreibung>` (z. B. `P3_CoreActions`) dienen als Integrationsbasis für alle Tasks einer Phase.
- Feature-Branches: `P<phase>_T<task>_<beschreibung>`; starten von der aktiven Phasen-Branch und werden nach erfüllter Task-DoD (`Planning/chaosoverlord.task.dod.md`) zurückgemergt.
- CI: GitHub Actions (`.github/workflows/ci.yml`) laufen auf `main`, aktiven Phasen-Branches und allen PRs Richtung der jeweiligen Phasen-Branch.

## Dokumentation & Artefakte
- Architektur & Layering: `Planning/chaosoverlord.architecture.md`
- Aufgaben & Phasen: `Planning/chaosoverlord.tasks.md`, `Planning/chaosoverlord.phases.md`
- Definition of Done: `Planning/chaosoverlord.task.dod.md`, `Planning/chaosoverlord.phases.dod.md`
- Fortschrittstracking: `Planning/chaosoverlord.progress.md`
- Phase‑Status & DoD: `Planning/chaosoverlord.phases.md`, `Planning/chaosoverlord.phases.dod.md`
- Entwicklungsentscheidungen & Annahmen: `Planning/chaosoverlord.notes.md`
- Lore & Referenzen: `Story/`

Alle neuen Entscheidungen, Risiken oder Abweichungen sollten unmittelbar in `chaosoverlord.notes.md` dokumentiert werden, damit der Wissensstand nachvollziehbar bleibt.

## Contribution Guide (Kurzfassung)
1. Branch von der aktiven Phasen-Branch (z. B. `P3_CoreActions`) erstellen (`P<phase>_T<task>_<beschreibung>` oder `feature/<phase>-<task>-<beschreibung>`).
2. Aufgabe gemäß Taskbeschreibung umsetzen; Tests/Docs aktualisieren.
3. Vor dem Commit lokal ausführen: `dotnet restore`, `dotnet build`, `dotnet test`.
4. Task-Status und Notizen aktualisieren (`chaosoverlord.progress.md`, `chaosoverlord.notes.md`).
5. Pull Request zur aktiven Phasen-Branch erstellen, CI prüfen, Reviews adressieren.

## Kontakt & Support
Fragen oder Ideen bitte im Issue-Tracker des Repositories erfassen oder über PRs einbringen. Weitere Projektvisionen und Roadmap stehen in `Planning/chaosoverlord.project.md` und den zugehörigen Planungsdokumenten.