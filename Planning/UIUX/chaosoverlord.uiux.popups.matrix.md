# Chaos Overlords – Popup-Matrix (Aktionen • Close-Verhalten • ContentView)

> Einheitliche Regeln (für alle Zeilen gültig):
>
> - **Action Row (unten):** `Primary` – `Secondary` – `Close/Abort`  
> - **Execute/Primary:** führt die Hauptaktion aus und schließt **automatisch** (*Auto-Close*).  
> - **Close:** schließt ohne Aktion; **persistiert** nur dann, wenn das Popup persistierbare Eingaben enthält (z. B. Settings).  
> - **Abort:** schließt **ohne** Persistenz und **ohne** Aktion.  
> - **ContentView:** Name der View, die im Popup-Content angezeigt wird (keine Implementierung, nur Bezeichner).  

| ID | Name | Kategorie | Primary (Execute) | Secondary | Close / Abort | **On Close (nach Klick)** | **ContentView** | Hinweise |
|---:|------|-----------|-------------------|-----------|---------------|---------------------------|-----------------|----------|
| P1 | EventsPopup | Info | Mark as Read | Scroll to Oldest | Close | **Close:** keine Persistenz (nur Anzeige). **Abort:** n/a. | `EventsListView` | Chronologische Liste, scrollfähig. |
| P2 | FinanceOverview | Info | Export Report | Filter Turn | Close | **Close:** keine Persistenz. **Abort:** n/a. | `FinanceOverviewView` | KPIs, Tabellen/Charts, Turn-Filter. |
| P3 | GangOverview | Info | Manage Gang (öffnet Gang-Mgmt) | View Stats | Close | **Close:** keine Persistenz. **Abort:** n/a. | `GangOverviewView` | Liste aller Gangs, Status/Ausrüstung. |
| P4 | WarehousePopup | Info | Sell Item (öffnet Verkauf) | Transfer | Close | **Close:** keine Persistenz. **Abort:** n/a. | `WarehouseView` | Lagerbestand, Filter, Sort. |
| P5 | AttackSheet | Action | Execute Attack | Preview | Abort | **Execute:** Auto-Close (GameState aktualisiert). **Close:** keine Persistenz. **Abort:** verwerfen. | `AttackSheetView` | Zielauswahl, Odds, evtl. Warnungen. |
| P6 | InfluenceSheet | Action | Apply Influence | Show Cost | Abort | **Execute:** Auto-Close. **Close:** keine Persistenz. **Abort:** verwerfen. | `InfluenceSheetView` | Site-/Kosten-Auswahl. |
| P7 | ResearchSheet | Action | Start Research | Show Info | Abort | **Execute:** Auto-Close. **Close:** keine Persistenz. **Abort:** verwerfen. | `ResearchSheetView` | Item/Tech Auswahl, Dauer/Kosten. |
| P8 | PurchaseSheet | Action | Confirm Purchase | Compare | Close | **Execute:** Auto-Close. **Close:** keine Persistenz. **Abort:** n/a. | `PurchaseSheetView` | Kaufdialog mit Bestand/Preis. |
| P9 | GiveSheet | Action | Give Item | Select Target | Abort | **Execute:** Auto-Close. **Close:** keine Persistenz. **Abort:** verwerfen. | `GiveSheetView` | Item-an-Gang Übergabe. |
| P10 | SellSheet | Action | Sell Item | Show Price | Close | **Execute:** Auto-Close. **Close:** keine Persistenz. **Abort:** n/a. | `SellSheetView` | Verkauf aus Warehouse. |
| P11 | HireSheet | Action | Hire Gang | Show Cost | Abort | **Execute:** Auto-Close. **Close:** keine Persistenz. **Abort:** verwerfen. | `HireSheetView` | Gang-Rekrutierung. |
| P12 | MoveSheet | Action | Confirm Move | Highlight Target | Abort | **Execute:** Auto-Close. **Close:** keine Persistenz. **Abort:** verwerfen. | `MoveSheetView` | Pfad/Ziel-Auswahl, Reichweite. |
| P13 | PlayerInfoPopup | Info | — | — | Close | **Close:** persistiert **nur** lokale Profil-Eingaben (falls vorhanden), sonst keine Persistenz. **Abort:** verwirft. | `PlayerInfoView` | Anzeige/Profil ggf. editierbar. |
| P14 | EventFeedExpanded | Info | Filter Events | Export | Close | **Close:** keine Persistenz. **Abort:** n/a. | `EventHistoryView` | Vollständige Historie, Filter. |
| P15 | HelpPopup | System | Open Manual | Show Controls | Close | **Close:** keine Persistenz. **Abort:** n/a. | `HelpView` | Hilfe/Referenz, Links, Shortcuts. |
| P16 | SettingsPopup | System | Save Settings | Reset | Close | **Primary (Save):** Auto-Close + Persistenz. **Close:** persistiert **Einstellungen** (persistChanges=true). **Abort:** verwirft alle Änderungen. | `SettingsView` | Tabs für Audio/Video/Difficulty. |
| P17 | ScenarioPopup | System | Start Scenario | Preview | Abort | **Primary:** Auto-Close + Persistenz (Neustart/Load). **Close:** keine Persistenz. **Abort:** verwerfen. | `ScenarioSelectionView` | Kampagnen/Startoptionen. |
| P18 | QuitConfirmPopup | System | Confirm Quit | Save & Quit | Abort | **Primary:** Auto-Close (Quit). **Secondary:** Save & Quit (persist+quit). **Abort:** bleibt im Spiel. | `QuitConfirmView` | Sicherheitsabfrage. |
| P19 | ConfirmPopup | System | Yes | No | Abort | **Yes:** Auto-Close (Bestätigung). **No/Abort:** schließen ohne Persistenz. | `ConfirmView` | Generische Rückfrage. |
| P20 | ErrorPopup | System | Retry | Copy Log | Close | **Retry:** bleibt offen bis Erfolg/Close. **Close:** schließt (ohne Persistenz). **Abort:** n/a. | `ErrorView` | Fehlermeldung, Details, Aktionen. |

---

## Einheitliche Schließlogik (für Copilot als Leitlinie)

- **Execute/Primary:**  
  - führt die zugehörige Spielaktion aus  
  - bei Erfolg **Auto-Close**  
  - GameState wird **aktualisiert** (Persistenz durch Aktion selbst)
- **Close:**  
  - **ohne Aktion**  
  - **persistiert** nur, wenn das Popup **persistierbare Eingaben** enthält (z. B. Settings)  
  - ansonsten **keine** Persistenz
- **Abort:**  
  - **ohne Aktion**, **ohne Persistenz**, Eingaben verwerfen

---

## ContentView-Konventionen

- ContentViews sind **spezifische UI-Views**, die im generischen Popup-Container gerendert werden.  
- **Namensschema:** `{Feature}View` (z. B. `AttackSheetView`, `SettingsView`).  
- Jede ContentView implementiert **ihre eigene Präsentation** (Listen, Formulare, Charts),  
  **ohne** die generische Popup-Hülle zu verändern (Header + Action Row kommen vom Container).

---

© 2025 Chaos Overlords UI/UX – Popup Matrix Specification
