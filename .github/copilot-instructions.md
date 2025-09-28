# Copilot Instructions – Chaos Overlords

Diese Datei gibt Copilot Kontext für Chat & Code-Reviews. Bitte prägnant bleiben, objektiv bewerten, konkrete Vorschläge machen.

## Projektüberblick
- **Ziel:** Modernes Remaster von *Chaos Overlords* mit minimaler, klarer Architektur (MVVM), datengetriebenen Referenzen und reproduzierbarer Rundenlogik.
- **Tech:** .NET 9, Avalonia UI, MVVM (CommunityToolkit.Mvvm), DI via `ServiceCollection`.
- **Datenquellen:** JSON-Dateien (Gangs, Items, Sites) → Data-Layer → Core-Modelle.
- **Nicht-Ziel:** Kein Over-Engineering, keine vorzeitige Feature-Explosion.

## Architektur-Leitplanken
- **Layering:** `App` (UI), `Core` (Domain + Services), `Data` (Datenzugriff), `Tests`.
- **MVVM:** Keine UI-Logik in Services/Models. ViewModels nur Bindings/Commands.
- **DI:** Alle Services als Interfaces registrieren (Testbarkeit).
- **Determinismus:** Zufall nur über zentralen `IRngService` mit Seed im `GameState`.
- **Logging:** Runden-/Ereignis-Log als erste Fehlerquelle vor Debugging.

## Akzeptanzkriterien – Phase 1 (Bootstrapping)
- App startet ohne Fehler (Release/Debug).
- **New Game** erzeugt `GameState` (HQ, Startcash, Startgang).
- **8×8 Placeholder-Map** wird angezeigt.
- JSON-Referenzdaten werden erfolgreich geladen (Gangs/Items/Sites).
- DI ist vollständig (IDataService, IScenarioService, …).
- **Nicht Teil von P1:** Kampf, Verstecken, Crackdown, Odds.

## Akzeptanzkriterien – Phase 2 (Rundenlogik „Happy Path“)
- **TurnViewModel & State-Machine**: Upkeep → Command → Execution → Hire → Elimination.
- **Command-Subphasen-Visualisierung**: Timeline mit Slots (Instant, Combat, Transaction, Chaos, Movement, Control).
- **RngService (Seeded)** + **Turn Event-Log**: Gleicher Seed ⇒ gleiche Ergebnisse.
- **EconomyService**: Upkeep (Gang-Upkeep), +1/Sektor, Site-Cash ±; Log-Einträge korrekt.
- **RecruitmentService**: 3er-Pool, Hire/Ablehnen, Kostenabzug, persistenter Pool.
- **Command Queue (Skeleton)**: Move/Control/Chaos (Chaos nur Projektion, noch keine Auszahlung/Crackdown).
- **Finance Preview** (City & Sector): Vorschau für nächste Runde inkl. Chaos(Estimate).
- **Map/Sector-Erweiterung**: Sektorklassen & Basis-Toleranz/Income (read-only, konfigurierbar).
- **Nicht Teil von P2:** Kampf/Stealth/Polizei/Crackdown-Mechanik, komplexe Odds.

## Code-Review-Richtlinien (für Copilot)
Bitte antworte kurz, nachvollziehbar, mit konkreten Änderungen:
1. **Architektur:** Verletzt der PR MVVM/Layering/DI? → Benenne Datei/Zeile + kurze Fix-Idee.
2. **Determinismus:** Direkte `new Random()`-Verwendung? → auf `IRngService` verweisen.
3. **Datenfluss:** Werden JSON-Refs zentral geladen? Keine UI-Parsing-Logik in Views/VMs.
4. **Services:** Fehlen Interfaces/Registrierungen? Zeige `Program.cs`/Composition Root.
5. **Tests:** Fehlen Unit-Tests für Economy/Hire/RNG? Fordere minimalen Smoke-Test an.
6. **UI/Accessibility:** Timeline/Preview ohne Binding-Fehler? Tooltips/Keyboard-Fokus vorhanden?
7. **Scope-Disziplin:** PR enthält Phase-3/5 Features? → Bitte auslagern oder Feature-Flag.

**Tonfall im Review:** Sachlich, knapp, lösungsorientiert. Max. 5 Kernpunkte, danach „Weitere Hinweise“ bündeln.

## Stil & Qualität
- **Naming:** Interfaces `I*`, ViewModels `*ViewModel`, Services `*Service`.
- **Async:** UI-blockierende Operationen vermeiden; ConfigureAwait in Libs, nicht im UI.
- **Immutables:** Wo möglich readonly/record für Referenzdaten.
- **Nullability:** `<Nullable>enable</Nullable>`; Guard-Clauses statt stiller Defaults.
- **Bindings:** Fehlerfrei (Output „Binding errors“ = ❌).

## Beispiel-Prompts (Copilot Chat)
- *„Erkläre den Datenfluss: Wer lädt die JSON-Referenzen und wo werden sie in DI registriert?”*
- *„Zeig mir alle Stellen, an denen `Random` direkt instanziiert wird und schlage einen Fix über `IRngService` vor.”*
- *„Welche Dateien betreffen die Finance Preview und wie prüfen wir die Summen in Tests?”*
- *„Liste alle Commands im Command-Resolver (Phase 2 Scope) und markiere nicht erlaubte (Kampf/Stealth).“*

## Sicherheits-/Wartungshinweise
- Keine Secrets im Repo; App-Settings ohne geheime Werte commiten.
- Dependabot & CodeQL-Alerts beachten; kleine PRs bevorzugen.
- Branch-Schutz: CI + CodeQL müssen grün sein (required checks).
