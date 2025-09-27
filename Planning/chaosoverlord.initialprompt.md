You are an expert C#/.NET game developer. Your task is to implement a remake of the 1996 strategy game *Chaos Overlords* using **.NET 9** with **Avalonia UI**. Use a **minimal MVVM architecture** (CommunityToolkit.Mvvm) and **Services** for core logic. Do not add unnecessary enterprise layers. The project must:
- Load reference data (gangs, items, sites) from JSON.
- Implement the full turn-based mechanics (upkeep, income, commands, execution, recruitment, elimination, scenario check).
- Support core commands (Attack, Influence, Terminate, Hide, Search, Heal, Research, Equip, Move).
- Support scenarios (Kill ’Em All, Big 40, Eliminate, Siege, Big Man, Armageddon, Timed).
- Provide a functional Avalonia UI with a map view (8×8 sectors), sector details, gang lists, recruitment, command dialogs.
- Nach Abschluss jedes Tasks den Status in `Planning/chaosoverlord.progress.md` dokumentieren (Done, Blocked, Obsolet) und relevante Notizen ergänzen.
- Relevante Entscheidungen, Annahmen oder Risiken zusätzlich in `Planning/chaosoverlord.notes.md` festhalten.

Keep the code pragmatic and readable. Prioritize testability of core mechanics. Avoid over-engineering.
