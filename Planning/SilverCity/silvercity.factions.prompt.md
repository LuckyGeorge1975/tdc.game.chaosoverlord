# Task: Fraktionen & Diplomatie (MVP) implementieren

**Ziel:** Zusätzlich zur Con-Zugehörigkeit wählen Spieler **eine Fraktion** (oder neutral). Fraktionen geben **kleine, situative Boni/Mali** und eröffnen **Diplomatie** (Ruf, NAP, Handelsabkommen, Gemeinsame Operation). Daten in `factions.json`, Beschreibung in `silvercity.factions.md`.

## 1) Modelle (Core)

* `FactionRef`
  * `Id`, `Name`, `Tags:string[]`, `Modifiers:FactionModifiers`, `DiplomaticBias:Dictionary<string,int>`
* `FactionModifiers` (halte an vorhandenen Keys aus `factions.json` fest)
  * z. B. `combatBonusIfEquipped:int`, `defenseBonusIfEquipped:int`,

    `itemCostMultiplierByType:Dictionary<string,double>`,

    `diceBonusForCommands:Dictionary<string,int>`,

    `researchEfficiencyMultiplier:double`,

    `sectorScopedModifiers: List<SectorScopeMod>` (mit `scope` Enums: `in_dump`, `with_clinic`, …),

    usw. – **nur die Felder implementieren, die im MVP genutzt werden.**
* `SectorScopeMod`
  * `Scope`, optionale Felder (`StealthBonus`, `ControlDiceBonus`, `CrackdownStageDelta`, `HealDiceBonus` …)
* `DiplomacyState`
  * `Reputation[PlayerId,PlayerId]:int`, `ActiveAgreements:list`, `Cooldowns`, `EventsLog`
* `AgreementType` enum: `Nap`, `Trade`, `JointOp`.

> **Hinweis:** Keine Over-Engineering – verwende optionale Felder (nullable) und simple Additiv-/Multiplikativ-Helper.

## 2) Data Layer

* `IDataService` → `GetFactions()` ergänzen.
* `EmbeddedJsonDataService` lädt `factions.json`.
* `ScenarioService` lässt Fraktionswahl parallel zur Con-Wahl zu (neutral möglich).

## 3) PlayerState erweitern

* `SelectedFactionId:string?`
* `AppliedFactionModifiers:FactionModifiers` (aggregiert aus `FactionRef` → einfacher Snapshot)
* `Diplomacy:DiplomacyState` (im `GameState`, nicht pro Spieler mehrfach)

## 4) Anwendung der Modifiers (MVP)

* **EconomyService**
  * Itemkosten: `itemCostMultiplierByType` und (falls vorhanden) `itemCostMultiplier`.
  * Cash-Einnahmen per Tag: `cashIncomeMultiplierBySiteTag` (falls gesetzt).
  * Bribe: `bribeCostMultiplier`.
* **CommandResolver**
  * Würfelboni per Befehl: `diceBonusForCommands[Influence/Heal/Chaos/Control]`.
  * Combat/Defense Offsets (nur wenn Gang ausgerüstet ist, falls `…IfEquipped`).
* **Movement**
  * Kostenfreie Moves (`freeMovesPerTurn`), zusätzliche Kosten (`moveExtraCashCostInTightQuarters`).
* **Research**
  * `researchEfficiencyMultiplier`.
* **Police/Crackdown**
  * `crackdownStageDelta` in Sektoren mit Scope `in_dump`.
  * `crackdownChanceBonusOnGridChaos` (kleines Add-On in Economy/Police check).
* **Sektor-Scopes**
  * Helper `bool SectorMatches(scope)` (Flags z. B. `IsDump`, `HasClinic`, `HasFactory`, `HasDepot`).

## 5) Diplomatie (MVP)

* **Reputation** (−100…+100) Matrix initialisieren:
  * Start +10 gleiche Fraktion, −10 deklarierte Antagonismen (aus `diplomaticBias` wechselseitig anwenden).
* **Abkommen** (einfacher State-Machine-Ansatz):
  * **NAP** : Dauer 10 Runden; Effekte: `borderAttackChanceMultiplier=0.75`, `mutualChaosDelta=-1`. Breach: −40 bilateral, −10 global, temp Trait „unglaubwuerdig“ (−1 Influence 5 Runden).
  * **Handel** : Dauer 8; Effekte: `bilateralItemCostMultiplier=0.90`, `giveCapacityDelta=+1`. Breach: −25 bilateral.
  * **JointOp** : Cooldown 5; Zielsektor marker; Effekte dort: `sectorCombatBonus=+1`, `sectorControlBonus=+1`, `sectorChaosMultiplier=0.5`. Erfolg +15, Fail −5.
* **Drift/Events** :
* Grenzkonflikt: −1/Runde bei wiederholten Kämpfen in gleichen Grenzsektoren.
* Hilfeleistung: +5 mit 20 % Chance (Event-Hook beim Heal in fremdem Sektor).

## 6) UI (App/Avalonia)

* **New Game Wizard** : Fraktions-Picker (Karten mit Name, 2–3 Kernboni, 1 Malus, Bias-Hinweis).
* **HUD** : Fraktions-Badge neben Con-Icon; Tooltip listet **aktive** Effekte (nur die, die wirklich greifen).
* **Diplomatie-Panel** :
* Tabelle Spieler vs. Ruf, Buttons: „Nichtangriffspakt vorschlagen“, „Handelsabkommen vorschlagen“, „Gemeinsame Operation markieren“.
* Abkommen-Status je Partei (Restlaufzeit/Cooldown).
* Event-Log mit letzten 5 Einträgen (Breach, Erfolg, Hilfeleistung).

## 7) Save/Load

* Persistiere `SelectedFactionId`, angewandte Modifiers (oder rehydriere aus Ref), Diplomatie-State (Rufmatrix, laufende Abkommen, Cooldowns, Events).

## 8) Tests (Core/xUnit)

* Laden & Validierung `factions.json` → alle IDs eindeutig, Felder erwartungsgemäß.
* Economy: Itemkosten-Multiplikator greift (mit/ohne Con-Stapelung).
* Commands: `diceBonusForCommands[Influence]` erhöht Erfolgswurf.
* Diplomatie: NAP reduziert Grenzangriffswahrscheinlichkeit; Breach setzt „unglaubwuerdig“.
* Scopes: `in_dump` Effekte nur in Dump-Sektoren.

## Akzeptanzkriterien

* New Game: Fraktionswahl sichtbar & wählbar; Neutral möglich.
* Diplomatie-Panel: NAP/Handel/Joint-Op können vorgeschlagen, angenommen und beendet werden.
* Mind. **3 Modifiers** aus unterschiedlichen Kategorien wirken nachweislich (z. B. Influence-Würfel, Itemkosten, NAP-Effekt).
* Save/Load bewahrt Fraktion & Diplomatie-State.

**Richtlinien**

* Minimaler Code-Einschnitt, keine übertriebene Architektur.
* Defensive null-Checks bei optionalen Feldern.
* Tooltips/Labels aus `silvercity.factions.md` übernehmen.
