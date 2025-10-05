# Chaos Overlords – Phase UIFramework (Desktop UI Shell & Interaction Layer)

> Datum: 2025-10-05  
> Status: Proposed (noch nicht gestartet)  
> Scope: Umsetzung des neuen Desktop UI-Konzepts (TopBar, ContextPanel, CommandPanel, ActionDock, Sheets/DialogService, Selection & Rule Engine) ohne neue Spielmechaniken. Keine Combat-/Police-/Stealth-Features. Fokus: Struktur, Interaktionsfluss, Barrierefreiheit, Testbarkeit.

---

## 1. Zielsetzung

Eine robuste, modulare UI-Shell, die:

1. Das neue Layout (TopBar, Map/Canvas, Right Panel = Context + Commands, Action Dock) technisch abbildet.
2. Aktionen ausschließlich über einen einheitlichen Dialog-/Sheet-Mechanismus startet (keine verstreuten Dialog-Services mehr).
3. Deterministisch bleibt: UI darf keine Hidden-State-Entscheidungen treffen – nur Projektion & Validierung.
4. Klare Trennung: Domain/Services (Core) vs. Darstellung/Enablement-Gründe (UI Rule Engine Adapter).
5. Erweiterbar für spätere Features (Combat, Police) ohne Refactor der Shell.

---

## 2. Gap-Analyse (Neu-Spezifikation vs. Ist-Code)

| Komponente / Konzept | Spez (Neu) | Aktueller Stand | Gap / Aktion |
|----------------------|-----------|-----------------|--------------|
| Top Bar (`CO.TopBarView`) | Runde, Spieler, Cash, Alerts, Quick-Access | Nicht vorhanden (nur TurnSections / FinanceHUDIndicator) | Neue View + VM, Aggregation aus `IGameSession`, `IFinancePreviewService`, Event-Badges. |
| Canvas / CityView | Pan/Zoom, Selection, 60 FPS Ziel | `MapView` = statisches 8×8 `UniformGrid` in `ScrollViewer` | Refactor zu eigenem `CityView` Host + Layering (Tiles, Tokens, Overlays); später Pan/Zoom (MatrixTransform). |
| SectorView (ehem. CellView) | Hit Testing, Status Badges | `SectorView.axaml` (statisch, kein Selected-State) | Auswahlzustand + Visual States + Click Command hinzufügen. |
| SelectionService | Zentraler Kontext (Sector/Gang/Item) | Nicht vorhanden (TurnViewModel hält verstreute Selection Properties) | Neues Interface + Implementation; Turn-/Map-VM subscriben. |
| ContextPanel | Tabs (Detail/Gangs/Items/Events/Finance) | Nicht vorhanden (Information verteilt in Turn-*Sections*) | Neue zusammengesetzte VM mit Tab-Child-VMs; liest via SelectionService + GameState. |
| CommandPanel | Kontextaktionen gruppiert | Nicht vorhanden (Queue/Timeline UI getrennt, keine Aktionsbuttons) | Implementierung + Binding an RuleEngine + Opening via DialogService. |
| Action Dock (Bottom) | Primärkommandos (1–6) | Nicht vorhanden | Neues Control + Hotkey Binding / CommandRelay. |
| DialogService / Sheets | Einheitliche Overlays, nicht-blockierend | Nur `CityFinancialDialogService` (spezifisch) | Generisches `IDialogService` + Host + Sheet-basierte ContentControls. FinanceDialog migrieren. |
| Action Rule Engine (`CO.ActionRuleEngine`) | Enablement, Reason, CostPreview | Domain kennt Commands, UI kein dediziertes Regelobjekt | Adapter: ließt Domain-State, produziert UI-spezifische Enablement DTOs (kein Hidden GameState). |
| Tooltips / Reasons | Einheitlich aus Rule Engine | Ad-hoc Strings (z. B. Previews in TurnViewModel) | Extrahieren in Rule Engine, TurnViewModel bereinigen. |
| Event Feed Panel Integration | Badge/Live-Region | `EventFeedViewModel` existiert, keine Einbettung ins neue Layout | Einbindung rechts (umschaltbar) / oder TopBar Badge → Sheet. |
| Keyboard Access | 1–6 für Actions, Tab-Struktur | Teilweise vorhanden (Commands im TurnViewModel) | KeyBinding Layer + Fokusreihenfolge definieren. |
| Theming / Styles | Konsistente Tokens (Spacing, Colors) | Sporadische Inline-Setters in `SectorView` | Theme Resource Dictionary + Design Tokens einführen. |
| Virtualization | Große Maps performant | Nicht erforderlich bei 8×8 (derzeit) | Schnittstellen vorbereiten (evtl. `ItemsRepeater` / OwnerDraw später). |

---

## 3. Architektur – Neue / Erweiterte Interfaces

```text
ChaosOverlords.Core (unverändert für Logik)
ChaosOverlords.App
  Services.UI
    ISelectionService
    IUiActionRuleEngine (liefert: IEnumerable<UiActionDescriptor>)
    IDialogService (Open/Close Sheets, async Ergebnisse)
    ISheetViewModel (Marker-Interface)
    IActionDockService (Hotkey Routing, optional)
```

`UiActionDescriptor` (DTO Vorschlag):
```csharp
public sealed record UiActionDescriptor(
    string Id,
    string Group,
    string DisplayName,
    string IconKey,
    bool   IsEnabled,
    string? DisabledReason,
    string? CostPreview,
    object? Payload);
```

---

## 4. Epics & Tasks

### Epic 1: Layout & Shell Refactor
1. `MainView` umbauen: Grid-Rows (TopBar, Content, ActionDock) + Right Panel Spalte.  
2. Placeholder Views für TopBar / ContextPanel / CommandPanel / ActionDock einhängen.  
3. Responsive Regeln (min/max Breite Right Panel) implementieren.  
**Acceptance:** App startet; alte TurnSections erreichbar über einen temporären Tab *Legacy*.

### Epic 2: Selection Service
1. Interface + Impl (`SelectionService`) mit Events (SelectionChanged).  
2. MapView click → `SelectSector(id)`; Gang-Auswahl später.  
3. TurnViewModel Migration: Entfernt direkte Sector/Gang Selections (nur Subscribes).  
**Acceptance:** Auswahl toggelt ContextPanel Inhalte; keine Null-Ref Fehler.

### Epic 3: ContextPanel + Tabs
1. Grund-VM (`ContextPanelViewModel`) + Enum Tabs.  
2. DetailTab: Site/Gang/Status Daten (read-only).  
3. GangsTab: Liste (später Auswahl → SelectionService.SetActiveGang).  
4. EventsTab: Filterbarer lokaler Feed (Adapter TurnEventLog).  
5. FinanceTab (optional flag).  
**Acceptance:** Tabs wechseln; leere / limited intel States; Bindings testbar.

### Epic 4: CommandPanel & Rule Engine
1. `IUiActionRuleEngine` Draft (liest GameState + Selection).  
2. Mapping Domain Commands → UiActionDescriptor (Attack, Influence, Move, Deploy, Withdraw, Buy, Sell, Research, Spy, Communicate – nicht implementierte Aktionen disabled mit Reason „Not Implemented“).  
3. CommandPanel View (Groups collapsible).  
4. Hot Reload bei Selection / TurnPhase Änderungen (MessageHub Subscription).  
**Acceptance:** Buttons zeigen korrekte Enable/Disable Gründe; Snapshot-Test.

### Epic 5: Action Dock
1. `ActionDockViewModel` spiegelt wichtigste 6 Actions (konfigurierbar).  
2. Hotkey Binding (Key1–Key6) → ExecuteActionDescriptor.  
3. Fokus-Indikatoren & Tooltips.  
**Acceptance:** Tastatur-Trigger löst identische Pfade wie Click aus.

### Epic 6: Dialog / Sheet Framework
1. `IDialogService` Interface (OpenSheetAsync<TVm, TResult>(payload)).  
2. DialogHost Control (Z-Overlay, ESC schließt wenn erlaubt).  
3. Migrate CityFinancialDialog → Sheet.  
4. Placeholder Sheets: AttackSheet (Mock), InfluenceSheet (Mock), MoveSheet (echte Parameter).  
**Acceptance:** Öffnen/Schließen ohne Memory-Leak (Dispose-Test), keine UI-Blockade.

### Epic 7: Enablement Gründe & Previews Vereinheitlichen
1. Entferne Control/Influence Preview Strings aus TurnViewModel → RuleEngine liefert `CostPreview`.  
2. Logging unverändert (Domain).  
**Acceptance:** TurnViewModel Code diff - Reduktion UI-spezifischer Strings ≥80 Zeilen.

### Epic 8: TopBar
1. ViewModel aggregiert: Aktiver Spieler, Runde, Cash, Alerts Count, Buttons (Finance, Settings, Events).  
2. Badge für neue TurnEvents (ungelesen).  
**Acceptance:** Cash & Runde aktualisieren sich deterministisch beim Phasenwechsel.

### Epic 9: Map Interaction Upgrade (Phase Scope: Selektion + Hover)
1. Visueller Selected-State in SectorView.  
2. (Optional) Basic Zoom (Ctrl+MouseWheel) – wenn unkritisch.  
3. Struktur vorbereiten für Token Layer (Gangs) ohne echte Icons.  
**Acceptance:** Auswahlpersistenz, keine Scroll-Jitter.

### Epic 10: Accessibility & Keyboard
1. ARIA-Rollen / AutomationProperties (Avalonia) für Tabs / Buttons.  
2. Focus Scope Reihenfolge definieren (TopBar → Map → Context → Commands → Dock).  
3. Screenreader Labels für Actions (Id + Reason).  
**Acceptance:** Automated accessibility probe script (simple traversal) findet alle Fokusziele.

### Epic 11: Performance & Virtualization Hooks
1. Abstraktion für Tile Presenter (Interface) – später Virtualization Strategy.  
2. Lazy Tooltip Inhalte (erst bauen bei Hover).  
**Acceptance:** Kein messbarer zusätzlicher Start-Up Lag (> +5%).

### Epic 12: Migration & Abschaltung Legacy Panels
1. Turn-*Section* Panels (Recruitment/Timeline/Queue) schrittweise integrieren:  
   - Timeline Badge / Dialog (statt permanenter Section)  
   - Recruitment als Sheet (HireSheet).  
2. Entferne alte Sections nach Paritätscheck.  
**Acceptance:** Feature-Parität dokumentiert; keine toten Abhängigkeiten nach Entfernen.

### Epic 13: Styling & Theming
1. `Theme.xaml` (Color Tokens, Spacing, FontSizes).  
2. Dark Mode (optional Flag).  
3. Konsolidierung Inline Styles → Ressourcen.  
**Acceptance:** StyleCop/Analyzer Regel – kein Inline Brush in Views (≥90% erfüllt).

### Epic 14: Testing & Tooling
1. Binding Smoke Tests für neue Views (Reflection + DataContext Mocks).  
2. Snapshot Tests für `UiActionDescriptor` Sets bei Beispiel-Szenarien (No Selection / Friendly / Enemy).  
3. Memory Leak Test (Open/Close 100 Sheets).  
4. Lint Rule: Keine direkte Game-Manipulation in ViewModels (Roslyn Analyzer optional später).  
**Acceptance:** Alle neuen Tests grün in CI; Coverage Zuwachs +5% UI Layer.

### Epic 15: Dokumentation & Developer Guide
1. README Abschnitt „UI Architecture“.  
2. Contribution Guide: Hinzufügen neuer Actions (Domain → Rule Engine → Button).  
**Acceptance:** Neuer Entwickler kann gemäß Guide eine Dummy-Action in < 30 Min implementieren (internes Ziel, beschreibend dokumentiert).

---

## 5. Sequenzempfehlung

1. Epics 1–2 (Shell + Selection)  
2. Epics 3–4 (Context + CommandPanel Basis)  
3. Epic 6 (Dialog/Sheets, früh für Integration)  
4. Epic 5 (Action Dock – nutzt Rule Engine)  
5. Epic 8 (TopBar)  
6. Epic 7 (Refactoring Previews)  
7. Epics 9–10 (Interaction + Accessibility)  
8. Epic 12 (Migration Legacy)  
9. Epics 11, 13–15 (Optimierung, Tests, Docs)

Parallel: Styling (13) kann früh starten, Testing (14) begleitet jede Epic.

---

## 6. Akzeptanzkriterien Gesamtphase

- Neue UI-Shell nutzbar: Aktionen (mind. Move, Influence) startbar über CommandPanel & ActionDock Sheets.  
- Keine alten TurnSections im Hauptlayout (nur Dialog/Sheets).  
- Auswahl einer SectorView aktualisiert ContextPanel in <150 ms.  
- Alle Actions zeigen konsistente Enablement-Tooltips.  
- Binding-Log: 0 Fehler beim Start + beim Ausführen einer Beispielrunde.  
- Unit-/Snapshot-Tests für Rule Engine & Bindings vorhanden.  
- Dokumentation erklärt Hinzufügen einer neuen Aktion.  

---

## 7. Risiken & Mitigation

| Risiko | Auswirkung | Mitigation |
|--------|------------|------------|
| Over-Engineering der Dialog/Sheet-Infrastruktur | Verzögerter Nutzen | Start: minimal (ContentControl + Overlay); Animations später. |
| Zu große PRs beim Shell-Refactor | Review-Verzögerungen | Epics 1–2 in kleineren Merges (Layout → Selection → Context). |
| Vermischung Domain-Logik in UI | Testbarkeit sinkt | Code Review Checklist (kein GameState-Mutation in ViewModels). |
| Performance (später größere Maps) | Ruckeln / Scroll Lag | Architektur-Hook für Virtualization früh anlegen, jetzt nur einfache Implementierung. |
| Inkonsistente Enablement-Gründe | Spieler-Verwirrung | Single Source: `IUiActionRuleEngine` + Tests für Beispiel-Kontexte. |
| Accessibility vernachlässigt bis spät | Hoher Nacharbeitsaufwand | Epic 10 früh schedulen; einfache Focus/Label Checks automatisieren. |

---

## 8. Aufwandsschätzung (grobe T-Shirt Größen)

| Epic | Größe |
|------|-------|
| 1 | M |
| 2 | S |
| 3 | M |
| 4 | M |
| 5 | S |
| 6 | M |
| 7 | S |
| 8 | S |
| 9 | S |
| 10 | S |
| 11 | S |
| 12 | M |
| 13 | S |
| 14 | M |
| 15 | S |

Mischung ergibt ~5–6 Wochen Netto (part-time) oder 2–3 Wochen fokussiert.

---

## 9. Nächste Sofortschritte (wenn Phase genehmigt)

1. Branch `phase-uiframework` anlegen.  
2. Epic 1 Task Breakdown Tickets erstellen.  
3. Minimaler `ISelectionService` + Autowire in MapViewModel Click Handler.  
4. Skeleton `ContextPanelView` + Binding Smoke Test.  
5. Erste `UiActionDescriptor` Liste (nur Move/Influence/Research aktiviert).  

---

© 2025 Chaos Overlords – UI Framework Planning
