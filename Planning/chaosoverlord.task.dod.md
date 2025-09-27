# Chaos Overlords – Task Definition of Done

## Workflow
1. Feature-Branch von `dev` erstellen (`feature/<phase>-<task>-<beschreibung>`).
2. Umsetzung + Tests durchführen, Commits klein und thematisch halten.
3. Pull Request gegen `dev` eröffnen, automatische Reviewer (mind. 1 Entwickler + 1 Designer/Game-Owner) zuweisen.
4. GitHub Actions müssen für den Branch und den PR grün sein.
5. Nach Freigabe via PR in `dev` mergen. Direkte Pushes auf `dev` oder `main` sind untersagt.

## Abschlusskriterien für einen Task
- **Scope erfüllt**: Alle Anforderungen aus `Planning/chaosoverlord.tasks.md` (oder dem Task-Ticket) umgesetzt, inklusive Randfälle und Fehlerbehandlung.
- **Tests**: Neue/aktualisierte Unit- und Integrationstests decken die Änderungen ab; keine Regressionen (lokal + CI).
- **Static Checks**: Linter/Formatter/Analyzer laufen fehlerfrei (CI + lokal, falls verfügbar).
- **Spielbarkeit**: Relevanter Spielpfad wurde als Smoke-Test manuell verifiziert (Screenshot/kurzes Video im PR dokumentiert).
- **Dokumentation**: Betroffene Projekt- oder Design-Dokumente, Release Notes, Changelogs sind aktualisiert.
- **Progress Tracking**: `Planning/chaosoverlord.progress.md` enthält den Task mit Status (Done/Blocked/Obsolet) und kurzen Notizen.
- **Entwicklungsnotizen**: Abweichungen, neue Annahmen oder wichtige Entscheidungen sind in `Planning/chaosoverlord.notes.md` ergänzt.
- **Internationalisierung/UI**: Strings, Tooltips, Accessibility-Prüfpunkte berücksichtigt (falls UI-relevant).
- **Abhängigkeiten**: Neue Libraries oder Assets sind registriert (z.B. `Directory.Packages.props`, `Manual/` oder `Data/`).
- **Review**: Peer-Review-Kommentare adressiert, offene Fragen im PR beantwortet.
- **Nacharbeiten**: Offene Folgeaufgaben oder bekannte Bugs im Issue-Tracker verlinkt und priorisiert.
- **Branch Hygiene**: Feature-Branch nach Merge gelöscht; `dev` lokal aktualisiert.

## Optionales Template für PR-Beschreibung
```
## Ziel
- <kurze Beschreibung>

## Umsetzung
- [ ] Code
- [ ] Tests
- [ ] Docs/Assets

## Validierung
- [ ] Lokaler Build/Test
- [ ] GitHub Actions (Link)
- [ ] Manuelle Prüfung (Beschreibung/Screenshot)

## Offene Punkte
- <Liste>
```