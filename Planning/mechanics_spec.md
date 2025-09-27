# Chaos Overlords – Mechanics Specification (Final Draft)

Dieses Dokument fasst die vollständigen Spielmechaniken von *Chaos Overlords (1996)* zusammen. Basis: vollständiges Manual (OCR/MD), extrahierte Tabellen (Gangs, Items, Sites), FAQ, Reviews, Wikipedia/Strategium.

---

## 1. Spielfeld & Startbedingungen
- **Karte:** 8×8 City-Grid (64 Blöcke, bezeichnet wie Schachfelder).
- **Spielerzahl:** bis zu 6 Chaos Overlords.
- **Start:** Jeder Spieler erhält 1 Hauptquartier-Block und eine Standard-Gang.
- **Globale Variablen pro Spieler:** Score, Cash, Sectors (kontrollierte Blöcke), Tolerance, Support.

---

## 2. Entitäten

### 2.1 Gangs (90)
- Attribute: HiringCost, UpkeepCost, Combat, Defense, TechLevel, Stealth, Detect, Chaos, Control, Heal, Influence, Research, Strength, BladeMelee, Ranged, Fighting, MartialArts.
- Pro Gang: max. 3 Item-Slots (Weapon, Armor, Misc).
- Upkeep wird jede Runde bezahlt.
- Eliminierung der „Right Hand“-Gang = Niederlage im Szenario *Eliminate*.

### 2.2 Items (53)
- Attribute: Type (1=Melee, 2=Ranged, 3=Armor, 4=Misc), ResearchCost, FabricationCost, TechLevel.
- Stat-Modifikationen für Combat, Defense, Stealth usw.
- Produktion erfordert Forschung (ResearchCost), danach Bau (FabricationCost).
- Sites (z. B. Factory) gewähren Discounts.

### 2.3 Sites (22)
- Attribute: Resistance, Support, Tolerance, Cash, Combat, Defense, Stealth, Detect, Chaos, Control, Heal, Influence, Research, Strength, BladeMelee, Ranged, Fighting, MartialArts, EquipmentDiscountPercent, EnablesResearchThroughTechLevel.
- Beispiele: Casino/Offices → Cash; Hospital/Clinic → Heal; Factory → Discounts; Research Lab/Science Center → Tech-Level-Cap.

---

## 3. Rundenablauf
1. **Upkeep/Income Phase**
   - Cash-Updates: Abzug Gang-Upkeep, Einnahme aus Sites/Blocks.
   - Polizei-/Chaos-Check.
2. **Command Phase**
   - Jeder Spieler erteilt seinen Gangs Befehle (eine Aktion pro Gang).
3. **Execution Phase**
   - Befehle werden in Zugreihenfolge aufgelöst.
4. **Hire Phase**
   - Gang-Rekrutierung aus Pool (3 Optionen, eine anheuerbar oder ablehnbar).
5. **Player Elimination Check**
   - Spieler ohne Gangs oder HQ verlieren.

---

## 4. Commands (Aktionen pro Gang)
- **Attack** – Direktes Gefecht gegen Gang im selben Block.
- **Bribe** – Bestechung (z. B. Polizei oder andere Gangs).
- **Chaos** – Aktionen, die Unruhe erzeugen und Crackdowns provozieren.
- **Control** – Gebietskontrolle verstärken.
- **Equip** – Items kaufen/herstellen und ausrüsten.
- **Give** – Items oder Geld an andere Gangs weitergeben.
- **Heal** – Stärke regenerieren.
- **Hide** – Gang versteckt sich (Stealth vs. Detect).
- **Influence** – Einflussnahme auf Sites oder Blöcke.
- **Move** – Gang in einen anderen Block versetzen.
- **Research** – Forschungspunkte generieren, TechLevel steigern.
- **Sell** – Items oder Besitz verkaufen.
- **Snitch** – Informationen an Polizei verraten.
- **Terminate** – Versuch, eine Gang vollständig zu eliminieren.

---

## 5. Würfelmechanik
- Pro Attribut wird eine Anzahl Würfel geworfen (entspricht dem Skillwert).
- Erfolge: typischerweise Würfe von 5–6 (oder nur 6, je nach Kontext).
- Modifikatoren: Force-Einsatz, Items, Sites, gegnerische Werte.
- Ergebnis: Erfolgswerte vs. Widerstand/Defense → Handlung gelingt oder scheitert.

*Deduktion: Genaue Erfolgsgrenzen sind im Manual teils vage. Wahrscheinlich nutzen alle Skills die gleiche Grundregel (5–6 = Erfolg), mit Abweichungen für spezielle Aktionen.*

---

## 6. Kampfmodell
- Angreifer: Combat + Item + Stil-Boni.
- Verteidiger: Defense + evtl. Site-Boni.
- Schadensmodell: Verlust an Strength.
- Gang kann verwundet (temporär) oder eliminiert (permanent) werden.

*Deduktion: Schaden = Anzahl Erfolge Angreifer minus Anzahl Erfolge Verteidiger.*

---

## 7. Polizei & Ordnung
- Chaoswerte summieren sich → Risiko für Crackdown.
- Tolerance = Schwelle, ab der Polizei aktiv wird.
- Support beeinflusst Erfolg von Influence und Reaktion auf Polizei.
- Polizeiaktionen: Razzien, Schließungen, Verhaftungen, Geldstrafen.

*Deduktion: Crackdown-Check vermutlich am Ende jeder Runde: Chaos – Tolerance > Schwelle → Event.*

---

## 8. Forschung
- Research-Befehl generiert Fortschritt.
- TechLevel = Zugang zu neuen Items.
- Sites mit TL-Erweiterung (Research Lab, Science Center) heben Limit.
- Forschungskosten = ResearchCost pro Item, danach FabricationCost.

*Deduktion: Fortschritt = Summe aller Research-Würfe einer Gang, modifiziert durch Sites.*

---

## 9. Szenarien
### Ziel-Szenarien
- **Kill ’Em All** – alle Gegner eliminieren.
- **Big 40** – zuerst 40 Sektoren.
- **Siege** – alle gegnerischen HQs einnehmen.
- **Eliminate** – Verlust der Right-Hand-Gang = Niederlage.
- **Big Man** – Halte Mittelsektoren, sammle Punkte.
- **Armageddon** – Erobere alle 64 Sektoren; Start mit 500 Cash + alle Techs.

### Zeit-Szenarien
- Laufzeit begrenzt; Sieger durch höchste Werte (Score, Cash, Sectors, Support).

---

## 10. Offene Punkte
- **Exakte Erfolgsformeln:** z. B. wie viele 5/6-Erfolge nötig sind.
- **Bewegung:** freie Relokation oder nur angrenzend? *Manual impliziert frei, aber nicht eindeutig.*
- **Polizeimechanik:** exakte Wahrscheinlichkeit für Crackdowns.
- **Forschungsfortschritt:** genaue Punkteberechnung.
- **AI:** Entscheidungsheuristiken der Gegner.

---

## 11. Nächste Schritte
1. Validierung aller Formeln durch Spiel-Logs oder Longplays.
2. Testweise Implementierung mit JSON-Daten (Gangs, Items, Sites).
3. Feinjustierung der fehlenden Mechaniken.

