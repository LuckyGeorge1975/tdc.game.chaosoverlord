# Chaos Overlords Task Workflow

Use this workflow for every new phase or task. Treat it as the default checklist before writing code, committing, or opening pull requests.

## Branching Strategy

1. **One branch per task.** No task shares a branch.
2. **One branch per phase.** Each phase has a dedicated integration branch.
3. **Starting a phase:**
   - Fetch latest `origin/main` and ensure local `main` is up to date.
   - Create the phase branch from `main` using the naming pattern `PX_ShortDescription` (e.g., `P2_TurnLogic`).
4. **Starting a task:**
   - Sync `main` again to avoid divergence.
   - Branch from the corresponding phase branch using the pattern `PX_TY_ShortDescription` (e.g., `P2_T8_TurnEventLog`).

## Execution Checklist

5. **Analyse the task.** Produce a concise implementation plan before coding.
6. **Implement the task.** Follow project architecture (MVVM, DI, determinism) and keep scope focused.
7. **Unit test.** Create or update tests that exercise the new behavior.
8. **Application smoke test.** Ensure the app starts without errors; this is part of the Definition of Done (DoD).
9. **Green tests required.** All unit tests must pass locally before committing.
10. **Remove dead code.** Delete classes or implementations that are no longer required. If the file was not originally created in this task, confirm with the team before removing it.
11. **Document changes.** Update relevant docs in `Planning/` or other locations. Note scope adjustments in `chaosoverlord.notes.md`. For changes affecting project planning, propose updates to the planning documents.
12. **Commit.** Only after completing the steps above, commit your work on the task branch.

## Additional Guidance

- Always keep branches small and focused on a single task.
- Rebase or merge regularly from the parent phase branch to minimize drift.
- When handing work off (PRs or reviews), reference this workflow to show each step is complete.
