# Chaos Overlords – Research & Item Specification

## 1. Forschung & Kauf
- **Research Cost (RC):** Punkte, die eine Gang durch den Befehl *Research* abbauen muss. Jeder Research-Erfolg reduziert RC um 1.
- **Fabrication Cost (Cos):** Produktionskosten pro Exemplar, sobald ein Item erforscht ist.
- **Ablauf:**
  1. Item auswählen und erforschen, bis RC = 0.
  2. Danach beliebig viele Exemplare herstellen (Cos bezahlen).
  3. Discounts von Sites (z. B. Factory –30%) wirken auf Cos.

## 2. TechLevel & Nutzung
- Jede Gang hat ein **TechLevel**.
- Items haben eine **TechLevel-Anforderung**.
- Ein Item kann nur von Gangs genutzt werden, deren TechLevel >= Item-TechLevel ist.
- Overlord kann Items erforschen, die aktuell keine Gang nutzen kann. Sie sind für spätere, höherstufige Gangs verfügbar.

## 3. Warehouse (Item-Lager)
- Erforschtes + produziertes Equipment landet im zentralen **Warehouse** des Overlords.
- Von dort können Items per **Equip-Befehl** an Gangs zugewiesen werden.
- Über **Sell-Befehl** können Items wieder verkauft werden (Cash, idR. unter Herstellungskosten).

## 4. Commands im Zusammenhang mit Research & Items
- **Research:** RC abbauen, bis Item freigeschaltet.
- **Equip:** Item aus Warehouse an Gang ausrüsten (Gang muss TechLevel-Anforderung erfüllen, max. 3 Slots).
- **Sell:** Item aus Warehouse verkaufen (Rückerstattung, reduzierter Wert).
- **Move/Trade (optional):** Items zwischen Gangs verschieben (über Give-Befehl).

## 5. Strategische Implikationen
- Forschung = langfristige Investition, abhängig von Research-Fähigkeiten und Sites.
- Herstellung = kurzfristige Cash-Investition, abhängig von Discounts.
- Gang-TechLevel = taktische Limitierung für sofortige Einsatzfähigkeit.
- Warehouse = Lager für Vorrat, Vorbereitung auf künftige Kämpfe oder neue Gangs.
