# SilverCity Overlords – Fraktionen & Diplomatie

> Fraktionen sind Unterwelt‑Ausrichtungen, die kleiner sind als Cons, dafür aber **situativ** eingreifen und **Diplomatie** ermöglichen. Ein Spieler kann **genau eine Fraktion** wählen (oder neutral bleiben) – zusätzlich zur Con‑Zugehörigkeit.

## Kernprinzipien
- **Leichte Boni/Mali** (typisch ±1 Würfel oder 10–25 % Multiplikatoren); sie sollen den Spielstil würzen, nicht dominieren.
- **Ortssensitiv**: Einige Effekte gelten nur in **The Dump** (Ex‑Kreuzberg) oder außerhalb.
- **Diplomatie‑Hooks**: Jede Fraktion besitzt Startsympathien/Antagonismen, die den **Ruf** zwischen Spielern beeinflussen.

---

## Fraktionsübersicht

| ID | Name | Kurzprofil | Kernboni | Mali | Diplomatische Tendenzen |
|---:|------|------------|----------|------|-------------------------|
| chrombund | **Chrombund** | Cyberware‑Syndikat, Teilehandel | +1 Combat **&** +1 Defense auf *ausgerüstete* Gangs; Waffen/Armor −10 % | −1 Stealth; Upkeep +10 % wenn ≥2 Items | +5 Datenkommune, −10 Duftbund |
| duftbund_flux | **Duftbund „Flux“** | Kult um „Duft“ & Rituale | +1 Würfel **Influence** & **Heal**; 10 % Chance auf +1 Support nach Influence | −1 Detect; 5 % Trance (Gang verliert Aktion) | −10 Chrombund, +5 Dump‑Wächter |
| datenkommune | **Datenkommune** | Grid‑Kollektiv & Datenbroker | +1 Tech, +1 Detect; **Research** +20 % effizienter | −1 Combat; Chaos an Grid‑Sites +5 % Crackdown | +5 Chrombund, −5 Schwarzlicht |
| schiebergilde | **Schiebergilde** | Logistik/Schmuggel, U‑Bahnen | 1 **kostenloser Move**/Runde (auto‑zugewiesen); **Give/Handel** kostet 0 Cash | −1 Influence; −10 % Cash aus Casino/BTL | +5 Schwarzlicht, −5 Wardens |
| schwarzlicht | **Schwarzlicht‑Makler** | Hehler, Paktierer, Fixer | **Bribe** −25 %; +1 Würfel **Control** bei ≥2 Sites im Sektor | −1 Chaos; **Snitch** doppelte Rufschäden | +5 Schiebergilde, −5 Datenkommune |
| dump_wardens | **Dump‑Wächterrat** | Lokalpatrioten von *The Dump* | In **The Dump**: +1 Stealth, +1 Control, Crackdown‑Stufe −1; **Heal** +1 mit Klinik | Außerhalb: −1 Support‑Gewinn; −1 Influence gegen Con‑Sites | +5 Duftbund, −5 Schiebergilde |
| eisenfaust | **Eisenfaust‑Kartell** | Söldnerringe & Kiez‑Boxclubs | +2 Combat beim **Attack**‑Befehl; **Retaliatory** erleidet −1 Schaden (min 0) | −1 Research; Itemkosten +10 % | −5 Datenkommune, +5 Wardens |
| nachtigallen | **Nachtigallen‑Orden** | Spione, Boten, Meuchel | +1 Stealth; **Hide** gewinnt +2 % Unentdecktheits‑Chance pro Stealth‑Punkt | −1 Defense | −5 Eisenfaust, +5 Schwarzlicht |
| rotorkollektiv | **Rotorkollektiv** | Drohnen, Rigger, Rotor‑Schrauber | **Detect** +2 in Sektoren mit Fabrik/Depot; 1×/10 Runden „Drohnen‑Sweep“ (Reveal) | −1 Influence; **Move** in engen Vierteln +1 Cash | +5 Datenkommune, −5 Wardens |

---

## Mechanische Details
- **Kostenmodifikatoren**: Multiplikativ (z. B. 0,9 für −10 %). Effekte stapeln *multiplikativ* mit Con‑Boni.
- **Würfelboni**: Additiv auf Befehlswürfe (z. B. Influence +1 → ein zusätzlicher W6 gemäß Checks).
- **Ortssensitivität**: „In The Dump“ meint Sektoren mit Bezirksflag `dump=true`.
- **Trance**: Runde‑Event. 5 % pro Runde; verhindert genau **eine** Aktion der betroffenen Gang in jener Runde (wird vor Execution gezeigt).
- **Drohnen‑Sweep** (Rotorkollektiv): Markiere 1 angrenzenden Sektor‑Block; deckt versteckte/undetektiere Gangs kurz auf (kein Auto‑Targeting, nur Sicht).

---

# Diplomatie‑MVP

## Ruf (−100 … +100)
- Start: 0 zu allen, außer **+10** zu Spielern mit derselben Fraktion, **−10** zu ausgewiesenen Antagonisten.
- Sichtbar im **Diplomatie‑Panel** (Farbcodierung, Tooltips, letzte Ereignisse).

## Abkommen (MVP‑Umfang)
1. **Nichtangriffspakt (NAP)**  
   *Bedingung:* Ruf ≥ +10. *Dauer:* 10 Runden.  
   *Effekt:* −25 % Angriffswahrscheinlichkeit in Grenzsektoren; −1 Chaos von beiden Parteien gegeneinander.  
   *Bruch:* −40 Ruf bilateral; global −10 „schlechter Ruf“; 5 Runden **Unglaubwürdig** (−1 Influence).

2. **Handelsabkommen**  
   *Bedingung:* Ruf ≥ +15. *Dauer:* 8 Runden.  
   *Effekt:* Zwischen den Parteien: Itemkosten −10 %; Give/Transfer +1 Kapazität.  
   *Bruch:* −25 Ruf bilateral; Sofortverlust aller Handelsrabatte.

3. **Gemeinsame Operation**  
   *Bedingung:* Ruf ≥ +20. *Dauer:* 1 markierter Zielsektor/5 Runden.  
   *Effekt (im Zielsektor):* Beide Parteien erhalten +1 Combat **und** +1 Control; **Chaos** von beiden Seiten zählt nur halb zur Crackdown‑Schwelle.  
   *Erfolg (Kontrolle erlangt):* +15 Ruf beidseitig; *Misserfolg:* −5 Ruf.

## Ruf‑Drift & Nebenwirkungen
- **Grenzkonflikt**: Fortlaufende Kämpfe in denselben angrenzenden Sektoren → −1 Ruf/ Runde zwischen den Akteuren.  
- **Hilfeleistung**: Heilung/Unterstützung in fremden Sektoren (Event) → +5 Ruf (20 %).  
- **Snitch** (mit Schwarzlicht‑Malus): doppelter Rufabzug gegen alle Zeugen‑Fraktionen.

---

## UI‑Hinweise
- **New Game**: Fraktions‑Picker (Karte/Badge, Tooltips mit Boni/Mali & Startbeziehungen).  
- **HUD**: Fraktions‑Badge neben Con‑Icon; Tooltip listet aktive Effekte.  
- **Diplomatie‑Panel**: Tabelle Spieler↔Ruf, Buttons „Pakt vorschlagen“, „Handel“, „Gemeinsame Op.“

---

> Siehe begleitende `factions.json` für die maschinenlesbare Definitionen (IDs, Modifier, Bias, Abkommen).

