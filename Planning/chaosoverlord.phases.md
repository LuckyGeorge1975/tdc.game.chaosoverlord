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

## Phase 4 – Research, Equipment & City Officials
**Status:** 🟡 Planned – Aufbauend auf Phase 3, schließt Kernökonomie- und Inventarfluss auf.
- Ziele:
	- Research als Instant‑Aktion mit deterministischem Fortschritt (projekte/tech‑caps, site‑basierte Boni).
	- Equipment‑Management als Transaction‑Aktionen (Equip/Unequip/Give/Sell) inkl. Slot‑/Tech‑Gates und Pricing/Discounts.
	- City Officials (Bribe/Snitch) als Instant‑Aktionen mit klaren Kosten/Effekten (z. B. Tolerance ±3, fixer Cash‑Abzug); Naming‑Klarstellung: „Bribe“ hier ist City Officials, Influence‑Variante wird ggf. „Payoff“ genannt.
	- Optionale Influence‑Methoden (Propaganda/Payoff) als Auswahl mit deterministischen Formeln; kann schrittweise aktiviert werden.
- Umsetzung:
	- Services: `IResearchService`, `IEquipmentService`, Erweiterungen im `ICommandQueueService`/Resolver für Instant/Transaction Slots.
	- Domain: ItemType/Slots (Weapon/Armor/Misc), TechLevel‑Gates; Validierung und Fehlertexte für doppelte/inkompatible Items.
	- Finance Preview: Research‑Kosten/Progress, Equipment‑Käufe, City Officials wirken sichtbar auf City/Sector.
	- UI: Methodenauswahl (optional) neben Influence Target; Inventory‑Panel für Equip/Give/Sell.
- Acceptance:
	- Queue/Preview/Execution decken Research, Equip/Unequip/Give/Sell und Bribe/Snitch ab (deterministisch, Seed‑stabil).
	- Slot‑/Tech‑Gates, Site‑Discounts und Pricing korrekt validiert; verständliche Status/Log‑Meldungen.
	- Finance Preview reflektiert alle neuen Kosten/Nutzen; Unit‑Tests für Pricing, Slots, Research‑Progress, Officials.
	- Influence‑Naming nicht kollidierend (City „Bribe“ vs. Influence „Payoff“), Dokumentation konsistent.

## Phase 5 – Kampf & Verstecken/Aufspüren
**Status:** 🟡 Planned – Aufbauend auf Phase‑4‑Ökosystem (Tasks 20–23).
- Combat Engine (Attack, Terminate) inkl. deterministischer Reports.
- Hide/Search‑Mechaniken mit Stealth/Detect Checks.
- UI‑Overlays & Event‑Log‑Integration für Gefechte/Stealth.

## Phase 6 – Polizei & Szenarien
- Crackdown system (Chaos vs. Tolerance).
- Implement win/loss checks for scenarios.

## Phase 7 – Feinschliff & Saves
- Save/Load service.
- UI polish.
- Stability & performance.

## Phase 8 – Silver City Adaptation (Priority 2)
- Lore- und Fraktions-Remix („Cons“) ausarbeiten.
- Con-Referenzdaten laden und in GameState integrieren.
- Modifier in Wirtschaft/Commands/Movement/Recruitment anwenden.
- UI-Erweiterungen für Con-Auswahl und Darstellung.
- Save/Load & QA für Con-spezifische Features.
