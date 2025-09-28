# Chaos Overlords – Phases Definition of Done

## Globale Abschlusskriterien (für jede Phase)
- Alle der Phase zugeordneten Tasks sind abgeschlossen, in einem Review bestätigt und in den Branch `dev` gemergt.
- Der aktuelle Stand von `dev` besteht sämtliche GitHub-Actions-Pipelines (Build, Tests, Static Analysis).
- Relevante Dokumentation ist aktualisiert (Projektplan, Changelogs, UI-Screenshots/Mockups in `Planning/` oder `Manual/`).
- `Planning/chaosoverlord.progress.md` listet abschließend alle Tasks der Phase mit Status (Done/Blocked/Obsolet) und markiert die Phase als abgeschlossen.
- Relevante Entscheidungen oder Abweichungen sind in `Planning/chaosoverlord.notes.md` dokumentiert.
- Alle offenen Fragen, Bugs oder technische Schulden sind erfasst und priorisiert; Blocker sind vor dem Merge aufgelöst.
- Ein kurzer Playtest/Smoke-Test bestätigt das Ziel der Phase auf den unterstützten Plattformen.
- Release-Notizen zur Phase sind vorbereitet (Kurzfassung der Ergebnisse, bekannte Einschränkungen).
- Stakeholder (Product Owner/Game Design) haben den Abschluss schriftlich bestätigt.

## Phasenspezifische Kriterien

### Phase 1 – Bootstrapping
- Lösungstruktur (`ChaosOverlords.sln` + Projekte) entspricht `Planning/chaosoverlord.project.md`.
- Referenzdaten (Gangs, Items, Sites) werden aus JSON geladen, Validierungstests decken typische Fehler ab.
- `GameState` und `ScenarioService.CreateNewGame` erzeugen ein spielbares Grundsetup (inkl. Startgang/-cash).
- Placeholder-Map (8×8) wird in der Avalonia App korrekt gerendert.
- Start-Szenario lässt sich via UI anlegen; App startet fehlerfrei unter Windows und (falls vorgesehen) Linux/macOS.

### Phase 2 – Rundenlogik „Happy Path“
- TurnViewModel steuert sämtliche Unterphasen gemäß Design.
- Wirtschaftssystem (Upkeep/Income) ist vollständig implementiert und testabgedeckt (Edge Cases + Min/Max Werte).
- Rekrutierung auffrchtigt Rekrutierungspool, Gang-Anheuern fließt in GameState ein.
- CommandResolver kann alle Happy-Path-Kommandos erfassen, speichern und stubweise ausführen.
- Automatisierte Tests prüfen mindestens einen vollständigen Happy-Path-Turn.

### Phase 3 – Kernaktionen
- Aktionen Einfluss, Forschung, Ausrüsten, Bewegen funktionieren inklusive Würfel-/Check-Utilities.
- Map-Interaktionen decken alle im Manual beschriebenen Pfade ab.
- Fehlerhafte Eingaben werden abgefangen und getestet.
- UI aktualisiert States unmittelbar nach jeder Aktion.

### Phase 4 – Kampf & Verstecken/Aufspüren
- Kampf- und Terminate-Logik mit vollständigen Ausgangsfällen implementiert und getestet.
- Hide/Search-Mechaniken besitzen stichhaltige Erfolgs-/Fehlerbedingungen und UI-Feedback.
- Dialoge/Overlays für Gefechte sind implementiert und barrierefrei bedienbar.
- Simulations-/Kampftests laufen in CI.

### Phase 5 – Polizei & Szenarien
- Crackdown-System bildet Chaos/Tolerance-Dynamik mit Persistenz ab.
- Win/Loss-Checks greifen für sämtliche Szenariotypen und werden von Tests abgedeckt.
- Szenario-spezifische Balancingdaten sind dokumentiert und versioniert.
- Telemetrie/Logs für Polizeievents ermöglichen Debugging.

### Phase 6 – Feinschliff & Saves
- Save/Load-Service unterstützt mindestens drei Spielstände, inkl. Fehler-Handling.
- UI-Polish (Transitions, Tooltips, Icons) ist abgeschlossen und durch Styleguide dokumentiert.
- Stabilitäts-/Performance-Benchmarks liegen unter definierten Schwellwerten.
- Abschluss-Regressionstests (manuell + automatisiert) sind dokumentiert und gelaufen.
- Phase Review abgeschlossen; Merge nach `main` durchgeführt.

### Phase 7 – Silver City Adaptation (Prio 2)
- `cons.json` wird geladen, validiert und in Domain-Modelle (`ConRef`, `ConModifiers`) überführt.
- Spielerzustand speichert Con-Zugehörigkeit; Szenariologik respektiert Exclusions.
- Mindestens drei Con-Modifikatortypen sind funktional nachweisbar (Tests + UI).
- Con-Auswahl im New-Game-Flow, HUD-Badge und Tooltips sind implementiert.
- Save/Load erhält alle Con-relevanten Zustände; Dokumentation/Release-Notes aktualisiert.
