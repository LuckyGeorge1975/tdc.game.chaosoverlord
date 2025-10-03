# chaosoverlord.uiux.controls.md

Dieses Dokument listet alle benötigten **Avalonia UserControls (Views)** für Chaos Overlords auf – ohne Code, als klare Baupläne für spätere AXAML-Implementierungen.

> Naming: `CO.*View` (UserControls), `CO.*Dialog` (modal). ViewModels analog `CO.*VM`. Commands als `ICommand`-Properties (z. B. `AttackCommand`).
> Interaction principle: Avoid context menus where possible. Primary command entry is the Command Bar on gangs and/or dedicated actions in the Right Panel.

---

## 0) Shell & Layout

### CO.ShellView
- **Zweck:** Host der gesamten App (Titel, Dock/Grid, Dialoghost).
- **Enthält:** Header (Spielstatus), `CO.TopMenuBar`, Arbeitsbereich (Map/Detail), `CO.RightPanelView`, `CO.FooterStatusView`.
- **Bindings (Input):** `CurrentTurn`, `GamePhase`, `SelectedSector`, `SelectedGang`, `IsModalOpen`.
- **Child Views:** `CO.TopMenuBar`, `CO.CityView`, `CO.RightPanelView`, `CO.FooterStatusView`.
- **Events/Outputs:** `NewGameRequested`, `SaveRequested`, `ExitRequested`.
- **Zustände:** Fullscreen/Windowed, Pause.

### CO.TopMenuBar (optional)
- **Zweck:** File / Options / Comm / Help (Win95-Style).
- **Bindings:** `MenuItems` (hierarchisch).
- **Outputs:** `MenuItemInvoked(MenuId)`.

---

## 1) Karten & Navigation

### CO.CityView
- **Zweck:** 8×8-Stadtkarte (Hauptansicht).
- **Enthält:** `CO.SectorGrid`, `CO.OverlordBar`, Legende/Tooltip-Layer.
- **Bindings:** `Sectors: ObservableCollection<SectorVM>`, `SelectedSector`, `HoverSector`.
- **Outputs:** `SectorClicked`, `SectorDoubleClicked`, `SectorContextRequested(Point, SectorVM)`.
- **Zustände:** Such-/Influence-Overlays ein/aus.

### CO.SectorGrid
- **Zweck:** Klickbarer 8×8-Renderer.
- **Bindings:** `Sectors`, `Selection`, `Highlights`.
- **Outputs:** `CellContextMenuRequested`, `CellSelected`.

### CO.SectorView
- **Zweck:** Detailansicht eines Sektors.
- **Enthält:** Big Art/Scene, `CO.SiteStrip`, `CO.GangStrip`, `CO.MiniMap3x3`, Back-Button.
- **Bindings:** `Sector: SectorVM`, `VisibleSites`, `VisibleGangs`, `MiniMapNeighborhood`.
- **Outputs:** `BackToCityRequested`, `GangCommandRequested`, `SiteSelected`.

### CO.MiniMap3x3
- **Zweck:** 3×3-Navigation.
- **Bindings:** `Neighborhood: ObservableCollection<SectorVM>`, `Center: SectorVM`.
- **Output:** `MiniCellClicked(SectorVM)`.

### CO.OverlordBar
- **Zweck:** Overlord-/Spielerportraits mit Status (WAIT, Active).
- **Bindings:** `Overlords: ObservableCollection<OverlordVM>`, `ActiveOverlord`.
- **Output:** `OverlordClicked(OverlordVM)`.

---

## 2) Kontext & Befehle

### CO.SectorContextMenu (deprecated)
- Hinweis: Kontextmenüs werden vermieden. Stattdessen werden Befehle über die Gang-spezifische Command Bar bzw. das Right Panel ausgelöst.
- Ersatz: Siehe `CO.GangCommandBar` und `CO.RightPanelView` (Actions).

### CO.GangCommandBar
- **Zweck:** Primäre Bedienoberfläche direkt am Gang (Manual-Konsistenz), ersetzt Kontextmenüs.
- **Bindings:** `AvailableCommands`, `IsRepeat`, `QueuedCommand`.
- **Outputs:** `CommandQueued(CommandId, RepeatMode)`.

---

## 3) Gangs & Infos

### CO.GangInfoDialog
- **Zweck:** Overlay „GANG INFORMATION“.
- **Enthält:** Portrait, Name, Flavor, Force/Upkeep/Tech, Stats (Combat/Defense/Stealth/Detect), Command-Skills (Chaos/Control/Heal/Influence/Research), Combat-Skills (Strength/Blade/Ranged/Fighting/MA), `CO.ItemSlotsPreview`, Close.
- **Bindings:** `Gang: GangVM`, `EffectiveStats`, `ItemEffects`.
- **Outputs:** `Closed`.

### CO.GangStrip
- **Zweck:** Zeigt alle Gangs im Sektor (mit Force-Bar & Status).
- **Bindings:** `Gangs: ObservableCollection<GangVM>`, `SelectedGang`.
- **Output:** `GangClicked(GangVM)`, `GangContextRequested(Point, GangVM)`.

### CO.ItemSlotsPreview
- **Zweck:** 3 Slots (Weapon / Armor / Misc) + Delta-Anzeigen (+X/−Y) bei Vorschau.
- **Bindings:** `EquippedItems`, `PreviewDelta`.
- **Output:** `OpenEquip`, `OpenGive`.

---

## 4) Items & Forschung

### CO.EquipmentPurchaseDialog
- **Zweck:** „EQUIPMENT TO PURCHASE“ inkl. Filter & Detailpanel.
- **Enthält:** Typ-Filter (All/Armor/Weapons/Misc), Itemliste, `CO.ItemDetailPanel`, OK/Cancel.
- **Bindings:** `Items: ObservableCollection<ItemVM>`, `SelectedItem`, `CanAfford`, `FactoryDiscount`.
- **Outputs:** `Purchase(ItemVM)`, `Closed`.

### CO.ItemDetailPanel
- **Zweck:** Name, Typ, Beschreibung, Kosten, Tech Level, Modifikatoren (kompakt, mit Delta-Vorschau).
- **Bindings:** `Item: ItemVM`, `DeltaFor(GangVM?)`.

### CO.EquipmentGiveDialog
- **Zweck:** „EQUIPMENT TO GIVE“ – Inventar → Zielgang.
- **Enthält:** Inventar-Liste, Zielgang-Liste mit Slots, Vorschau.
- **Bindings:** `Inventory`, `TargetGangs`, `SelectedItem`, `SelectedTargetGang`, `TechEligible`.
- **Outputs:** `Give(ItemVM, GangVM)`, `Closed`.

### CO.EquipmentResearchDialog
- **Zweck:** „EQUIPMENT TO RESEARCH“ – Fortschritt & Freischaltung.
- **Bindings:** `Researchables`, `Progress(%)`, `ScienceBonus`, `TurnsRemaining`.
- **Outputs:** `QueueResearch(ItemVM)`, `Closed`.

### CO.EquipmentSellDialog
- **Zweck:** „EQUIPMENT TO SELL“ – Auswahl & Bestätigung.
- **Bindings:** `SellableItems`, `EstimatedProceeds`.
- **Outputs:** `Sell(ItemVM[])`, `Closed`.

---

## 5) Sites & Einfluss

### CO.SiteStrip
- **Zweck:** Site-Symbole eines Sektors (unten links), inkl. Status.
- **Bindings:** `Sites: ObservableCollection<SiteVM>` (Icon, Name, Resistance, Influenced?).
- **Outputs:** `SiteClicked(SiteVM)`, `InfluenceRequested(SiteVM)`.

### CO.SiteInfoDialog
- **Zweck:** „SITE INFORMATION“ – z. B. Factory (Resistance 10, -30% Equip).
- **Bindings:** `Site: SiteVM`, `Resistance`, `Bonuses`.
- **Outputs:** `Closed`.

### CO.SiteToInfluenceDialog
- **Zweck:** Zielwahl für Influence (Bild + OK/Cancel).
- **Bindings:** `CandidateSites`, `SelectedSite`, `CanInfluence`.
- **Outputs:** `ConfirmInfluence(SiteVM)`, `Closed`.

### CO.SearchSitesDialog  *(Umbenennungsvorschlag: „Scan Sites“)*
- **Zweck:** Gebäude-Suche/Filter (Gym, Museum, Corporate Tower, Research Lab, Hospital …).
- **Bindings:** `Categories`, `SelectedCategories`, `MatchCount`.
- **Outputs:** `ApplySearchFilter`, `Closed`.

---

## 6) Kampf

### CO.TargetAcquisitionDialog
- **Zweck:** Zielauswahl für Attack.
- **Bindings:** `Targets: ObservableCollection<GangVM>`, `SelectedTarget`, `EstimatedOdds`.
- **Outputs:** `ConfirmTarget(GangVM)`, `Closed`.

### CO.CombatDialog
- **Zweck:** Kampf-Screen (Auto-Resolution).
- **Enthält:** Links: Spieler-Gang (Portrait, Slots, Force). Rechts: Gegner-Gang. Optional: „Preview Odds“ & `CO.CombatLogPanel`.
- **Bindings:** `Attacker`, `Defender`, `ItemsView`, `ExpectedOutcome`, `AutoResolve`.
- **Outputs:** `Closed`.

### CO.CombatLogPanel *(optional)*
- **Zweck:** Rundenweises Log (Treffer, Mods, Force-Delta).
- **Bindings:** `Entries`.
- **Outputs:** –.

---

## 7) Events & Feedback

### CO.LastTurnEventsDialog
- **Zweck:** Event-Popups (Sector Control Attained/Lost, Cooperation, Research Completed, Overlord Eliminated, Police Crackdown …).
- **Bindings:** `Events: ObservableCollection<EventVM>`, `CurrentIndex`.
- **Outputs:** `OpenEventDetail(EventVM)`, `JumpToRelatedSector(SectorId)`, `Closed`.

### CO.EventFeedPanel *(empfohlen)*
- **Zweck:** Nicht-blockierender seitlicher Feed (ersetzt/ergänzt Popups).
- **Bindings:** `EventLog`.
- **Outputs:** `EventClicked(EventVM)`.

---

## 8) Finanzen

### CO.CityFinancialDialog
- **Zweck:** „CITY FINANCIAL“ – Einnahmen/Ausgabenübersicht.
- **Bindings:** `GangUpkeep`, `Contracts`, `Equipment`, `Officials`, `SectorTax`, `SiteProtection`, `Chaos`, `NetAdjustment`.
- **Outputs:** `Closed`.

### CO.FinanceHUDIndicator *(empfohlen)*
- **Zweck:** Permanenter +/- Income Indikator im HUD.
- **Bindings:** `ProjectedIncome`, `ProjectedExpense`, `Net`.
- **Outputs:** `OpenFinancialDialog`.

---

## 9) Endgame & Ranking

### CO.RankingView
- **Zweck:** Balkenvergleich der Overlords.
- **Bindings:** `RankingEntries`, `MetricSelection`.
- **Outputs:** `OverlordSelected`.

### CO.AwardsView
- **Zweck:** Awards mit Symbolen (Faust, Totenkopf, Dollar, …).
- **Bindings:** `Awards: ObservableCollection<AwardVM>`.
- **Outputs:** –.

### CO.EndgameStatsView
- **Zweck:** Tabelle (Cash Earned/Spent, Damage, Casualties, Overthrows).
- **Bindings:** `StatsRows`, `SortState`.
- **Outputs:** `ExportRequested`.

---

## 10) Right Panel & Footer

### CO.RightPanelView
- **Zweck:** Hauptmenü-Buttons / Info-Pane (Events, Comlink, Combat Summary, Financial, City, Detail, Sector, Gangs, Ranking, Search, Done).
- **Bindings:** `Actions: ObservableCollection<ActionVM>`, `Selection`.
- **Outputs:** `ActionInvoked(ActionId)`.

### CO.FooterStatusView
- **Zweck:** Turn-/Phase-Status, Hinweise/Fehlermeldungen.
- **Bindings:** `Turn`, `Phase`, `HintMessage`, `WarningMessage`.
- **Outputs:** `OpenHelp`.

---

## 11) Dialog-Infrastruktur

### CO.ConfirmDialog
- **Zweck:** generisches „OK/Cancel“ (Sell, Terminate, …).
- **Bindings:** `Title`, `Message`, `Icon`, `ConfirmText`, `CancelText`.
- **Outputs:** `Confirmed`, `Cancelled`.

### CO.HelpDialog
- **Zweck:** In-Game Hilfe/FAQ (optional).
- **Bindings:** `MarkdownContent`, `SearchQuery`.
- **Outputs:** –.

---

## 12) ViewModel-Daten (Kurzabriss, ohne Code)

- **SectorVM:** `Id`, `OwnerColor`, `HasControl`, `Sites: SiteVM[]`, `Gangs: GangVM[]`, `Markers`.
- **GangVM:** `Id`, `Name`, `Portrait`, `Force`, `Upkeep`, `Tech`, `Stats`, `Skills`, `EquippedItems`, `Can(CommandId)`.
- **SiteVM:** `Id`, `Name`, `Category`, `Resistance`, `Influenced`, `Bonuses`.
- **ItemVM:** `Id`, `Name`, `Type(Weapon/Armor/Misc)`, `Tech`, `Cost`, `Mods`, `Researched`.
- **OverlordVM:** `Id`, `Name`, `Portrait`, `Status`.
- **EventVM:** `Type`, `Title`, `Body`, `RelatedSectorId`, `RelatedSiteId`, `Severity`.
- **ActionVM/MenuEntryVM:** `Id`, `Label`, `Icon`, `IsEnabled`, `Tooltip`.

**Commands (Auszug):** `AttackCommand`, `InfluenceCommand`, `MoveCommand`, `EquipCommand`, `GiveCommand`, `ResearchCommand`, `SellCommand`, `HealCommand`, `HideCommand`, `ChaosCommand`, `ControlCommand`, `SnitchCommand`, `TerminateCommand`.

---

## 13) Konsistenz-Hinweise

- **Kontextmenü vs. Command-Bar:** Beide Oberflächen spiegeln denselben Befehl; wähle eine als Primär-Entry und halte sie synchron.
- **Search vs. Influence:** `CO.SearchSitesDialog` im UI als **Scan Sites** benennen; Influence bleibt Übernahme.
- **Transparenz:** Delta-Werte in `CO.ItemDetailPanel`; optional `CO.CombatLogPanel`; `CO.FinanceHUDIndicator` im Footer.
- **Events:** Zusätzlich `CO.EventFeedPanel` als nicht-blockierendes Log (Events klickbar → Map/Sector).

---

## 14) Priorisierte Bau-Reihenfolge

1) `CO.ShellView`, `CO.CityView`, `CO.SectorGrid`, `CO.RightPanelView`, `CO.FooterStatusView`
2) `CO.SectorView`, `CO.SiteStrip`, `CO.GangStrip`, `CO.MiniMap3x3`
3) `CO.SectorContextMenu` (+ optional `CO.GangCommandBar`)
4) `CO.GangInfoDialog`, `CO.ItemSlotsPreview`
5) `CO.EquipmentPurchaseDialog`, `CO.ItemDetailPanel`, `CO.EquipmentGiveDialog`, `CO.EquipmentResearchDialog`, `CO.EquipmentSellDialog`
6) `CO.SiteInfoDialog`, `CO.SiteToInfluenceDialog`, `CO.SearchSitesDialog`
7) `CO.TargetAcquisitionDialog`, `CO.CombatDialog` (+ `CO.CombatLogPanel`)
8) `CO.CityFinancialDialog`, `CO.EventFeedPanel` / `CO.LastTurnEventsDialog`
9) `CO.RankingView`, `CO.AwardsView`, `CO.EndgameStatsView`
10) `CO.TopMenuBar`, `CO.ConfirmDialog`, `CO.HelpDialog`
