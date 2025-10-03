# chaosoverlord.uiux.controls.mermaid.md

Dieses Dokument enthält **Mermaid-ClassDiagram Outlines** für alle geplanten Avalonia-Controls.
Die Notation verwendet einfache Typen/Listen und listet **Bindings** als Felder sowie **Outputs** als Methoden.

---

## 0) Shell & Layout

```mermaid
classDiagram
class CO_ShellView {
  +CurrentTurn:int
  +GamePhase:string
  +SelectedSector:SectorVM
  +SelectedGang:GangVM
  +IsModalOpen:bool
  +NewGameRequested()
  +SaveRequested()
  +ExitRequested()
}

class CO_TopMenuBar {
  +MenuItems:List~MenuItemVM~
  +MenuItemInvoked(id:string)
}

class CO_RightPanelView {
  +Actions:List~ActionVM~
  +Selection:ActionVM
  +ActionInvoked(id:string)
}

class CO_FooterStatusView {
  +Turn:int
  +Phase:string
  +HintMessage:string
  +WarningMessage:string
  +OpenHelp()
}

CO_ShellView --> CO_TopMenuBar
CO_ShellView --> CO_RightPanelView
CO_ShellView --> CO_FooterStatusView
```

---

## 1) Map & Navigation

```mermaid
classDiagram
class CO_CityView {
  +Sectors:List~SectorVM~
  +SelectedSector:SectorVM
  +HoverSector:SectorVM
  +SectorClicked(id:string)
  +SectorDoubleClicked(id:string)
  +SectorContextRequested(x:int,y:int,id:string)
}

class CO_SectorGrid {
  +Sectors:List~SectorVM~
  +Selection:SectorVM
  +Highlights:List~string~
  +CellSelected(id:string)
  +CellContextMenuRequested(x:int,y:int,id:string)
}

class CO_SectorView {
  +Sector:SectorVM
  +VisibleSites:List~SiteVM~
  +VisibleGangs:List~GangVM~
  +MiniMapNeighborhood:List~SectorVM~
  +BackToCityRequested()
  +GangCommandRequested(gangId:string,cmd:string)
  +SiteSelected(siteId:string)
}

class CO_MiniMap3x3 {
  +Neighborhood:List~SectorVM~
  +Center:SectorVM
  +MiniCellClicked(id:string)
}

class CO_OverlordBar {
  +Overlords:List~OverlordVM~
  +ActiveOverlord:OverlordVM
  +OverlordClicked(id:string)
}

CO_CityView --> CO_SectorGrid
CO_CityView --> CO_OverlordBar
CO_SectorView --> CO_MiniMap3x3
```

---

## 2) Kontext & Befehle

```mermaid
classDiagram
class CO_SectorContextMenu {
  +MenuEntries:List~MenuEntryVM~
  +CommandChosen(commandId:string)
}

class CO_GangCommandBar {
  +AvailableCommands:List~string~
  +IsRepeat:bool
  +QueuedCommand:string
  +CommandQueued(commandId:string,repeat:bool)
}
```

---

## 3) Gangs & Infos

```mermaid
classDiagram
class CO_GangInfoDialog {
  +Gang:GangVM
  +EffectiveStats:StatsVM
  +ItemEffects:List~StatDeltaVM~
  +Closed()
}

class CO_GangStrip {
  +Gangs:List~GangVM~
  +SelectedGang:GangVM
  +GangClicked(id:string)
  +GangContextRequested(x:int,y:int,id:string)
}

class CO_ItemSlotsPreview {
  +EquippedItems:ItemSlotsVM
  +PreviewDelta:List~StatDeltaVM~
  +OpenEquip()
  +OpenGive()
}
```

---

## 4) Items & Forschung

```mermaid
classDiagram
class CO_EquipmentPurchaseDialog {
  +Items:List~ItemVM~
  +SelectedItem:ItemVM
  +CanAfford:bool
  +FactoryDiscount:int
  +Purchase(id:string)
  +Closed()
}

class CO_ItemDetailPanel {
  +Item:ItemVM
  +DeltaFor(gangId:string)
}

class CO_EquipmentGiveDialog {
  +Inventory:List~ItemVM~
  +TargetGangs:List~GangVM~
  +SelectedItem:ItemVM
  +SelectedTargetGang:GangVM
  +TechEligible:bool
  +Give(itemId:string,gangId:string)
  +Closed()
}

class CO_EquipmentResearchDialog {
  +Researchables:List~ItemVM~
  +ScienceBonus:int
  +TurnsRemaining:int
  +Progress:int
  +QueueResearch(itemId:string)
  +Closed()
}

class CO_EquipmentSellDialog {
  +SellableItems:List~ItemVM~
  +EstimatedProceeds:int
  +Sell(itemIds:List~string~)
  +Closed()
}
```

---

## 5) Sites & Einfluss

```mermaid
classDiagram
class CO_SiteStrip {
  +Sites:List~SiteVM~
  +SiteClicked(id:string)
  +InfluenceRequested(id:string)
}

class CO_SiteInfoDialog {
  +Site:SiteVM
  +Resistance:int
  +Bonuses:List~BonusVM~
  +Closed()
}

class CO_SiteToInfluenceDialog {
  +CandidateSites:List~SiteVM~
  +SelectedSite:SiteVM
  +CanInfluence:bool
  +ConfirmInfluence(id:string)
  +Closed()
}

class CO_SearchSitesDialog {
  +Categories:List~string~
  +SelectedCategories:List~string~
  +MatchCount:int
  +ApplySearchFilter()
  +Closed()
}
```

---

## 6) Kampf

```mermaid
classDiagram
class CO_TargetAcquisitionDialog {
  +Targets:List~GangVM~
  +SelectedTarget:GangVM
  +EstimatedOdds:int
  +ConfirmTarget(id:string)
  +Closed()
}

class CO_CombatDialog {
  +Attacker:GangVM
  +Defender:GangVM
  +ItemsView:ItemSlotsVM
  +ExpectedOutcome:string
  +AutoResolve:bool
  +Closed()
}

class CO_CombatLogPanel {
  +Entries:List~CombatLogEntryVM~
}
```

---

## 7) Events & Finance

```mermaid
classDiagram
class CO_LastTurnEventsDialog {
  +Events:List~EventVM~
  +CurrentIndex:int
  +OpenEventDetail(id:string)
  +JumpToRelatedSector(id:string)
  +Closed()
}

class CO_EventFeedPanel {
  +EventLog:List~EventVM~
  +EventClicked(id:string)
}

class CO_CityFinancialDialog {
  +GangUpkeep:int
  +Contracts:int
  +Equipment:int
  +Officials:int
  +SectorTax:int
  +SiteProtection:int
  +Chaos:int
  +NetAdjustment:int
  +Closed()
}

class CO_FinanceHUDIndicator {
  +ProjectedIncome:int
  +ProjectedExpense:int
  +Net:int
  +OpenFinancialDialog()
}
```

---

## 8) Endgame & Ranking

```mermaid
classDiagram
class CO_RankingView {
  +RankingEntries:List~RankingEntryVM~
  +MetricSelection:string
  +OverlordSelected(id:string)
}

class CO_AwardsView {
  +Awards:List~AwardVM~
}

class CO_EndgameStatsView {
  +StatsRows:List~EndgameRowVM~
  +SortState:string
  +ExportRequested()
}
```

---

## 9) Dialog Infrastruktur

```mermaid
classDiagram
class CO_ConfirmDialog {
  +Title:string
  +Message:string
  +Icon:string
  +ConfirmText:string
  +CancelText:string
  +Confirmed()
  +Cancelled()
}

class CO_HelpDialog {
  +MarkdownContent:string
  +SearchQuery:string
}
```
