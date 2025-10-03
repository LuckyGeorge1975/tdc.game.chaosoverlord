# Chaos Overlords – Mechanics Specification (Consolidated)

## 1. Spielfeld & Startbedingungen
- **Karte:** 8×8 City-Grid (64 Blöcke, Schachbrett a1..h8).
- **Spieler:** bis zu 6 Chaos Overlords.
- **Start:** Jeder Spieler startet mit einem HQ und einer Standard-Gang.
- **Globale Variablen:** Score, Cash, Sectors, Tolerance, Support.

---

## 2. Entitäten

### 2.1 Gangs (90)
- Attribute: HiringCost, UpkeepCost, Combat, Defense, TechLevel, Stealth, Detect, Chaos, Control, Heal, Influence, Research, Strength, BladeMelee, Ranged, Fighting, MartialArts.
- 3 Item-Slots (Weapon, Armor, Misc).
- Upkeep wird pro Runde fällig.
- Verlust der „Right Hand“-Gang = Niederlage in Szenario *Eliminate*.

### 2.2 Items (53)
- Attribute: Type (Melee, Ranged, Armor, Misc), ResearchCost (RC), FabricationCost (Cos), TechLevel, Stat-Modifikationen.
- Items müssen zuerst erforscht (RC = 0), dann produziert (Cos) werden.
- Discounts durch Sites (z. B. Factory –30%).
- Produzierte Items landen im **Warehouse** → können ausgerüstet oder verkauft werden.

### 2.3 Sites (22)
- Attribute: Resistance, Support, Tolerance, Cash, Combat, Defense, Stealth, Detect, Chaos, Control, Heal, Influence, Research, Strength, BladeMelee, Ranged, Fighting, MartialArts, EquipmentDiscountPercent, EnablesResearchThroughTechLevel.
- Beispiele: Casino (Cash), Hospital (Heal), Factory (Discount), Research Lab (TechLevel 10), Science Center (TechLevel 8).

---

## 3. Rundenablauf
1. **Upkeep/Income** – Gang-Upkeep abziehen, Site/Block-Einnahmen addieren, Police-Check vorbereiten.
2. **Command Phase** – Jede Gang erhält einen Befehl (Attack, Influence, Research, usw.).
3. **Execution Phase** – Befehle werden in Reihenfolge abgehandelt.
4. **Hire Phase** – Rekrutierung: 3 Kandidaten, 1 anheuerbar oder abgelehnt.
5. **Player Elimination** – Spieler ohne Gangs oder HQ verlieren.

---

## 4. Commands
- **Attack:** Angriff auf Gang im gleichen Block (Combat vs. Defense). Schaden = Differenz Erfolge, Stärke sinkt.
- **Influence:** Versuche Site/Block zu übernehmen (Influence+Control vs. Resistance). Erfolgreich → Block flippt.
- **Terminate:** Eliminierung einer gegnerischen Gang (stärkerer Kampf).
- **Hide:** Gang versteckt sich (Stealth vs. Detect).
- **Search:** Suche nach versteckten Gangs (Detect vs. Stealth).
- **Heal:** Stärke regenerieren (Heal + Boni durch Sites/Items).
- **Research:** Abbau ResearchCost eines Items (Force×Research-Skill Würfe, nur „6“ = Erfolg). Forschung kumulativ über Gangs.
- **Equip:** Item aus Warehouse einer Gang geben (Gang muss TechLevel erfüllen).
- **Sell:** Item aus Warehouse verkaufen (Cash zurück, Abschlag).
- **Give:** Item oder Cash an andere Gang übertragen.
- **Move:** Gang in anderen Sektor verlegen.

---

## 5. Würfelmechanik
- **Grundprinzip:** Anzahl Würfel = Skillwert × Force.  
- **Erfolg:** normal = 5–6; Research = nur 6.  
- **Ergebnis:** Erfolge Angreifer vs. Verteidiger → Differenz bestimmt Erfolg oder Schaden.
- **Modifikatoren:** Items, Sites, Force-Boni.

---

## 6. Research-System
- **RC (Research Cost):** Punkte, die durch Erfolge reduziert werden.  
- **Cos (Fabrication Cost):** Kosten pro Exemplar nach Freischaltung.  
- **TechLevel:** bestimmt, ob eine Gang ein Item ausrüsten darf.  
- **Sites:** Research Lab (TL 10), Science Center (TL 8), Factory (Discounts).  
- **Warehouse:** Zentrallager pro Overlord für produzierte Items.  
- **Forschung ist Overlord-weit kumulativ.**

---

## 7. Polizei & Ordnung
- **Chaos-Werte** erhöhen Risiko von Crackdowns.  
- **Tolerance** = Schwelle für Aktionen.  
- **Support** beeinflusst Influence und Polizei-Reaktionen.  
- **Crackdowns:** Razzien, Site-Schließungen, Cash-Verlust, Gang-Verluste.

---

## 8. Szenarien
### Ziel-Szenarien
- **Kill ’Em All** – alle Gegner eliminieren.
- **Big 40** – zuerst 40 Sektoren.
- **Siege** – alle gegnerischen HQs einnehmen.
- **Eliminate** – Verlust der „Right Hand“-Gang = Niederlage.
- **Big Man** – Kontrolle über Mittelsektoren (d4/d5/e4/e5) generiert Punkte → 40 Punkte = Sieg.
- **Armageddon** – Erobere alle 64 Sektoren; Start mit 500 Cash, alle Techs freigeschaltet.

### Zeit-Szenarien
- Ende nach X Runden; Sieger = höchster Wert in Score, Cash, Sectors, Support.

---

## 9. Offene Punkte
- Exakte Erfolgsformeln (z. B. Einflussnahme) – aktuell Annahme: Standard-Würfelregel (5–6 Erfolg).  
- Exakte Crackdown-Trigger (Wahrscheinlichkeiten, Ereignistabelle).  
- Einkommensformel pro Block (Basis unklar, vermutlich 1–2 Cash).  
- KI-Heuristiken.
