# chaosoverlord.uiux.overview.md

## 📌 Übersicht Chaos Overlords – UI/UX Analyse

Dieses Dokument fasst die Module (Map, Gangs, Items, Sites, Combat, Events, Finance, Endgame, Menus) zusammen und hebt zentrale **Unstimmigkeiten** sowie **Verbesserungspotenziale** hervor.

---

## 1) UI/UX Module im Überblick

| Modul | Screenshots | Haupt-UI-Elemente | Manual-Abgleich | Bemerkungen |
|---|---|---|---|---|
| **Map & Sectors** | 3,4,7,37,38 | 8×8 Grid, Overlord Bar, Main Panel, Mini-Map | Entspricht City/Sector View | **Richtlinie:** Keine Kontextmenüs; Befehle über Command Bar/Right Panel |
| **Gangs** | 1,8,9,14,15,19,20 | Gang Info, Stats, Equip-Effekte, Kontextmenü-Commands | Manual-Stats/Skills/Tech Upkeep | Manual (Command Bar) vs UI (Rechtsklick) |
| **Items** | 11–13,29,30 | Purchase, Info, Give, Research, Sell | Slots, Tech, Research Costs | Keine Delta-Anzeige, viele Popups |
| **Sites** | 6,24–26,28 | Site Info, Resistance, Influence, Search | Influence nur bei Control | „Search Sites“ vs „Influence“ |
| **Combat** | 18–21,34 | Target, Combat Screen, Events | Combat-Phase, Modifikatoren | Auto-Combat, kein Log |
| **Events** | 22,28,31,32,35 | Last Turn Events | Turn-Feedback | Nur Schlagzeilen, keine Zahlen |
| **Finance** | 33 | City Financial | Upkeep→Income→Expenses→Adjustment | Summen statt Drilldown |
| **Endgame** | 36,39,40 | Rankings, Awards, Stats | Siegbedingungen | Keine Begründung „warum“ |
| **Menus** | 2 | File/Edit/Options/Comm/Help | Manual bestätigt | Meta-UI ohne Screenshots |

---

## 2) Zentrale Unstimmigkeiten
1) **Command-Vergabe:** Manual-Command Bar vs. frühere Kontextmenü-Idee. Projekt-Richtlinie: Keine Kontextmenüs; nutze Command Bar/Right Panel.
2) **Terminologie:** „Influence“ vs. „Search Sites“.
3) **Feedback-Tiefe:** Zahlen/Logs fehlen (Combat, Events, Finance).
4) **Finanzsystem:** wichtige Infos nicht im HUD, keine Warnungen.
5) **Endgame-Erklärung:** Siegbegründung fehlt; Symbol-Legende unklar.

---

## 3) UX-Kommentare
- **Starker Stil**, aber Nützlichkeit leidet.
- **Zweikanal-Bedienung** (Buttons für Infos, Rechtsklick für Aktionen) wird vermieden. Primär: Command Bar/Right Panel.
- **Hohe Lernkurve** ohne Manual.
- **Viele Klicks** für Kernaktionen.
- **Black-Box-Mechaniken** (Combat/Events).

---

## 4) Lösungsvorschläge (Redesign – zusammengefasst)
1) **Command-Struktur vereinheitlichen** (Command Bar/Right Panel als primäre Oberflächen; keine Kontextmenüs).
2) **Begriffe klären** (Search→Scan; Influence bleibt).
3) **Feedback verbessern** (Combat-Log, Event-Feed, Delta-Anzeigen).
4) **HUD-Indikatoren** (Finanzwarnungen, Map-Highlights).
5) **Flow optimieren** (Drag&Drop, direkte Map-Verlinkungen).


