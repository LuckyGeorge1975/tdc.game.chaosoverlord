# Task: Con-Zugehörigkeit (Fraktionen) implementieren

**Ziel:** Spieler können optional einem **Con** (Konsortium) angehören. Die Zugehörigkeit verleiht **Boni/Modifikatoren** und **schließt** bestimmte **Szenarien** aus. Neutraler Start ohne Con bleibt möglich.

## Dateien & Daten

* Lies `ChaosOverlords.Data/cons.json` (siehe bereitgestellte Datei im Projektordner).
* Dokumentation/Referenz: `silvercity.cons.md`.

## Änderungen (minimal-invasiv, MVVM)

1. **Core/Models**
   * `ConRef` (Id, Name, Focus[], HqDistrict, Modifiers, Penalties, ScenarioExclusions[], Notes).
   * `ConModifiers` (Felder: `incomeMultiplierBySiteTag:Dictionary<string,double>`, `influenceDiceBonus:int`, `itemCostMultiplier:double`, `gangDefenseBonus:int`, `techLevelBonus:int`, `stealthBonus:int`, `detectBonus:int`, `healRateMultiplier:double`, `freeEliteGangUses:int`, `combatBonus:int`, `ownSectorCrackdownChanceMultiplier:double`, `diceBonusForCommands:Dictionary<string,int>`, `uniqueItemPools:string[]`, `extraMovesPerTurn:int`, `moveCashCost:int`).
   * `PlayerState` erweitern um `SelectedConId:string?` und `AppliedConModifiers:ConModifiers`.
2. **Data Loading**
   * `IDataService` um `IReadOnlyList<ConRef> GetCons()` erweitern.
   * `EmbeddedJsonDataService` ergänzt um Laden/Deserialisieren von `cons.json`.
3. **Scenario-Filter**
   * `IScenarioService`/`IWinCheckService`/`NewGameWizard` so erweitern, dass verfügbare Szenarien anhand von `SelectedConId` gefiltert werden (`scenarioExclusions`).
   * UI: Szenarioauswahl zeigt gesperrte Szenarien disabled mit Tooltip „Durch Con-Zugehörigkeit gesperrt“.
4. **Modifiers anwenden**
   * **EconomyService** : `incomeMultiplierBySiteTag`, `itemCostMultiplier`, `ownSectorCrackdownChanceMultiplier`.
   * **Combat/Stats** : `combatBonus`, `gangDefenseBonus`, `stealthBonus`, `detectBonus`, `techLevelBonus`, `healRateMultiplier`.
   * **Commands** : `diceBonusForCommands` (Mapping auf `Influence`, `Chaos` etc.), `influenceDiceBonus` als generischer Zusatz.
   * **Movement** : `extraMovesPerTurn` & `moveCashCost` (Move-Auflösung erweitern).
   * **Recruitment** : `freeEliteGangUses` (einmalige Gang-Spawn-Logik + Upkeep-Sonderfall).
   * **Unique Items** : `uniqueItemPools` an `EquipmentService`-Pool hängen (optional Stub: Flag setzen, bis Items existieren).
5. **UI (App)**
   * Start/New Game: **Con-Picker** (Dropdown/Karten), Anzeige der wichtigsten Boni + Szenario-Sperren.
   * Player-Badge im HUD mit Con-Icon/Name; Tooltip zeigt aktive Modifikatoren.
   * Neutral-Option (kein Con): „Ohne Zugehörigkeit (keine Boni, keine Sperren)“.
6. **Save/Load**
   * `ISaveLoadService`: `SelectedConId` und ggf. `freeEliteGangUses (remaining)` persistieren.
7. **Tests (Core)**
   * Lade-Test: `cons.json` → `ConRef[]` vollständig und valide.
   * Economy-Test: Cash-Income Multiplier greift pro Site-Tag.
   * Scenario-Filter-Test: Exclusions werden korrekt angewandt.
   * Commands-Test: `diceBonusForCommands["influence"]` erhöht Erfolgswürfel.
   * Movement-Test: `extraMovesPerTurn` erlaubt zweite Bewegung ohne Cash bei `moveCashCost=0`.

## Akzeptanzkriterien

* New Game Wizard zeigt Con-Auswahl (inkl. neutral).
* Auswahl eines Cons filtert Szenarien korrekt.
* Mindestens 3 Modifikator-Arten sind wirksam nachweisbar (z. B. Itemkosten, TechBonus, Influence-Würfel).
* Save/Load erhält Con-Zugehörigkeit und verbleibende Einmal-Nutzung.
* Keine Regressions in bestehenden Phasen (Phase 1–2 laufen).

**Hinweise:**

* Halte Änderungen klein; nutze vorhandene Services (`EconomyService`, `CommandResolver`, `RecruitmentService`).
* Modifiers möglichst **additiv** anwenden (keine Hard-Forks).
* Tooltips/Labels aus `silvercity.cons.md` übernehmen.
