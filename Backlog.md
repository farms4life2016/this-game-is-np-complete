# Backlog: This Game is NP-Complete

This is a lightweight Jira-style backlog in markdown.

## Workflow

- `todo`: not started
- `doing`: currently in progress
- `done`: completed and verified
- `blocked`: needs a decision or dependency

## Definition of Done (DoD)

- Code compiles in Unity and via `dotnet build Assembly-CSharp.csproj`
- Feature behavior matches `GameDesign.md`
- Edge cases have tests (or clearly documented manual test cases)
- No debug-only shortcuts left in production paths

## Epic A: Core Puzzle Simulation

- [x] `CORE-01` (`done`) Replace ad-hoc model logic with deterministic turn simulation.
- [x] `CORE-02` (`done`) Implement cardinal movement (`Up/Down/Left/Right`) with infinite wall boundary behavior.
- [x] `CORE-03` (`done`) Apply `Add`/`Multiply` only on tile entry or when staying still.
- [x] `CORE-04` (`done`) Merge blocks ending on same tile into one block with summed value.
- [x] `CORE-05` (`done`) Implement win/loss detection when one block remains.
- [x] `CORE-06` (`done`) Add core tests for movement, merge, modifier, and game-over rules.
- [ ] `CORE-07` (`todo`) Add deep-copy/snapshot support for undo.

## Epic B: Puzzle Data and Import

- [ ] `DATA-01` (`todo`) Define puzzle JSON schema (`width`, `height`, `target`, `tiles`, `blocks`).
- [ ] `DATA-02` (`todo`) Implement JSON parse + validation (bounds, wall placement, duplicates).
- [ ] `DATA-03` (`todo`) Add sample puzzles in `Assets` for smoke testing.
- [ ] `DATA-04` (`todo`) Display clear user-facing errors for invalid puzzle files.

## Epic C: Gameplay Vertical Slice

- [ ] `PLAY-01` (`todo`) Build `GameplayController` to call simulation on input.
- [ ] `PLAY-02` (`todo`) Render board + blocks from puzzle state.
- [ ] `PLAY-03` (`todo`) Animate movement and merges.
- [ ] `PLAY-04` (`todo`) Implement undo and restart.
- [ ] `PLAY-05` (`todo`) Add target/current status UI and win/loss modal.

## Epic D: Menus and UX

- [ ] `UI-01` (`todo`) Main menu flow: Play, Reduction (stub), Settings (stub), Credits.
- [ ] `UI-02` (`todo`) Level select with completion indicator.
- [ ] `UI-03` (`todo`) Input mappings: Arrow keys + WASD.
- [ ] `UI-04` (`todo`) Camera pan and zoom.

## Epic E: Quality and Portfolio Readiness

- [ ] `QUAL-01` (`todo`) Add regression tests for known tricky scenarios.
- [ ] `QUAL-02` (`todo`) Add structured logs for simulation turns in development builds.
- [ ] `QUAL-03` (`todo`) Update README with architecture and run instructions.
- [ ] `QUAL-04` (`todo`) Create short technical notes for employer-facing portfolio context.

## Sprint Plan (Recommended)

### Sprint 1 (Engine First)

- `CORE-01` to `CORE-06`
- Output: tested simulation core independent of UI

### Sprint 2 (Playable Loop)

- `CORE-07`, `DATA-01` to `DATA-03`, `PLAY-01` to `PLAY-05`
- Output: playable puzzle scene from JSON with undo/restart

### Sprint 3 (Usability)

- `UI-01` to `UI-04`, `DATA-04`, `QUAL-01`
- Output: usable game shell with robust input and feedback

### Sprint 4 (Polish + Portfolio)

- `QUAL-02` to `QUAL-04`
- Output: portfolio-quality repo and demonstration package
