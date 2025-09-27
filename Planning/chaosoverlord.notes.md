# Chaos Overlords – Development Notes

> Dokumentiert Entscheidungen, Erkenntnisse und offene Fragen, die nicht direkt aus den Phasen- und Task-Plänen hervorgehen.

## Entscheidungsverlauf

| Datum | Thema | Entscheidung | Begründung / Auswirkungen |
| --- | --- | --- | --- |
| 2025-09-27 | Projektstruktur | `Directory.Build.props` und `Directory.Packages.props` bündeln Framework-/Package-Versionen zentral. | Reduziert Wartungsaufwand, stellt Konsistenz über alle Projekte sicher. |
| 2025-09-27 | Paketversionen | Alle Avalonia- und Testpakete auf aktuellste stabile Versionen angehoben. | Sorgt für aktuelle Bugfixes/Features; CI nutzt standardmäßiges .NET 9 SDK. |
| 2025-09-27 | CI/CD | GitHub Actions Workflow `Build & Test` führt Restore/Build/Test für `main` und `dev` aus. | Stellt Qualität vor Merges sicher; Grundlage für spätere Deployment-Schritte. |
| 2025-09-27 | Referenzdaten | Platzhalter-JSONs und Schwarz/Weiß-SVGs für Gangs/Items/Sites angelegt (`ChaosOverlords.Data/Reference`). | Erlaubt Task 2 mit realem Loader, bis finale Assets verfügbar sind. |

## Offene Punkte / Beobachtungen

- Remote `origin` ist lokal noch nicht konfiguriert. Für Pushes nach GitHub wird die genaue URL des Repositories benötigt.
- Branch-Strategie gemäß DoD: Feature-Branches von `dev`, Phase-Abschlüsse mergen nach `main`.

## To-Validate / Ideen

- Ergänzende Checks in CI (z. B. Avalonia Linter, Code-Coverage-Upload) evaluieren, sobald erste Feature-Implementierungen vorliegen.
- Notwendige Dokumentationen für Designer/Art (Mockups, Styleguide) separat strukturieren, wenn UI-Assets vorliegen.
