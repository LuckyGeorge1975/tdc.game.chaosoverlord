# Chaos Overlords Development Phases

> Statuspflege: Nach Abschluss einer Phase die Ergebnisse in `Planning/chaosoverlord.progress.md` dokumentieren (inkl. Verweis auf den Merge nach `main`).

## Phase 1 – Bootstrapping
**Status:** ✅ Completed (2025-09-28) – Tasks 1–6 umgesetzt, App startet via DI, Map & New Game funktionsfähig.
- Set up solution, projects, and DI container.
- Load JSON reference data (gangs, items, sites).
- Implement `GameState` and `ScenarioService.CreateNewGame`.
- Display map placeholder (8×8 sectors).

## Phase 2 – Rundenlogik „Happy Path“
**Status:** ✅ Completed (2025-10-01) – Turn dashboard modularisiert (MessageHub + Abschnitt-ViewModels), Finance Preview & Timeline live, alle Tasks 7–14 ausgeliefert; Chaos-Auszahlung bleibt explizit Phase 3/5.
- TurnViewModel steuert Phasen inkl. Command-Subphasen (✅).
- EconomyService: upkeep & income inkl. Event-Log (✅).
- RecruitmentService: Pool-Refresh, Hire/Ablehnen, deterministisch (✅).
- Command Queue/Resolver: Move/Control/Chaos (Stub) + Timeline-Visualisierung (✅).
- Finance Preview: City/Sector-Projektion als eigener Abschnitt (✅).
- Map & Sector Basisklassen/Toleranz (Task 14) (✅ via site-driven stats).

## Phase 3 – Kernaktionen
**Status:** 🔄 In Progress – Tasks 15–17 umgesetzt (ActionFramework, Movement, Influence). Nächste Schritte: Research/Equip (Task 18), UI/Logging‑Erweiterungen (Task 19).
- ActionFramework + Dice Utilities liefern deterministische Checks (✅).
- Movement (✅) & Influence (✅) integriert in Queue/Resolver inkl. deterministischer Turn‑Events.
- Logging & UI‑Polish: Datei‑Logging mit Retention, Auto‑Scroll, „Open Logs Folder“, Previews für Control/Influence (✅).
- Ausstehend: Research & Equipment Management (Task 18), erweiterte UI/Logging‑Integration & Smoke‑Szenario (Task 19).

## Phase 3.1 – Influence‑Methoden (Propaganda/Bribe)
**Status:** 🟡 Planned – Aufbauend auf Phase 3, klar abgrenzbarer Nachtrag für Influence.
- Ziel: Einfluss als auswählbare Methode anbieten (Propaganda/Bribe) statt generischer Einheitsaktion.
- Domain: enum `InfluenceMethod { Propaganda, Bribe }` und Erweiterung der Queue/Resolver‑Signatur.
- UI: Dropdown neben „Influence Target“ zur Methodenauswahl; Preview zeigt Methode + Formel/Parameter.
- Regeln (erste Iteration, deterministisch):
	- Propaganda: Net = Influence + MediaBonus − (2×Support + Security)
	- Bribe: Net = Influence + ⌊CashSpent/2⌋ − (Support + 2×Security); CashSpent vom Player‑Cash abziehen, Cap pro Action.
- Acceptance (Phase abgeschlossen, wenn):
	- Methoden‑Dropdown sichtbar und bindet an Queue/Preview/Execution.
	- Previews geben Methode, Terme und Net‑Ergebnis korrekt aus (inkl. Cash‑Abzug bei Bribe).
	- Deterministisches Verhalten gegeben (gleicher Seed ⇒ gleiche Ergebnisse); Unit‑Tests für beide Methoden vorhanden.
	- Keine Änderungen an Police/Crackdown (bleibt Phase 5), keine neuen Items/Traits nötig (kann später folgen).

## Phase 4 – Kampf & Verstecken/Aufspüren
**Status:** 🟡 Planned – Aufbauend auf Phase-3-Framework (Tasks 20–23).
- Combat Engine (Attack, Terminate) inkl. deterministischer Reports.
- Hide/Search-Mechaniken mit Stealth/Detect Checks.
- UI-Overlays & Event-Log-Integration für Gefechte/Stealth.

## Phase 5 – Polizei & Szenarien
- Crackdown system (Chaos vs. Tolerance).
- Implement win/loss checks for scenarios.

## Phase 6 – Feinschliff & Saves
- Save/Load service.
- UI polish.
- Stability & performance.

## Phase 7 – Silver City Adaptation (Priority 2)
- Lore- und Fraktions-Remix („Cons“) ausarbeiten.
- Con-Referenzdaten laden und in GameState integrieren.
- Modifier in Wirtschaft/Commands/Movement/Recruitment anwenden.
- UI-Erweiterungen für Con-Auswahl und Darstellung.
- Save/Load & QA für Con-spezifische Features.
