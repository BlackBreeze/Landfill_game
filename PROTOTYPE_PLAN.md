# LANDFILL — Prototype Plan
*Goal: Reach a playable build that answers "is this fun?" as fast as possible.*

---

## Guiding Philosophy

The prototype answers **three questions** in order:
1. Does the pre-filled board + timer feel good to play? (Phase 1)
2. Do creatures add tension without adding frustration? (Phase 2)
3. Does the upgrade loop make you want to play again? (Phase 3)

Everything else is polish. Do not build what you cannot yet test.

---

## Phase 0 — Project Foundation
**Goal:** Unity project is structured and ready to build on.
**Time estimate:** 0.5–1 day

### Tasks
- [ ] Set up folder structure: `Assets/Scripts`, `Assets/Prefabs`, `Assets/Scenes`, `Assets/UI`, `Assets/Art`
- [ ] Create `GameScene` (the main play scene) and `UpgradeScene`
- [ ] Set camera to orthographic, sized to display a 10×20 board at a comfortable scale
- [ ] Define constants file (`GameConstants.cs`): board width (10), visible height (20), default timer (90s), base gravity speed
- [ ] Set up a simple `GameManager` singleton that owns scene transitions

### Deliverable
Empty scene with a camera correctly framed for a 10×20 board.

---

## Phase 1 — Core Board & Piece System
**Goal:** A fully playable Tetris loop on a pre-filled board with a timer.
**Time estimate:** 3–5 days

This is the heart of the prototype. Get this right before touching anything else.

### 1A — Board Representation
- [ ] `Board.cs` — 2D int array `[10, 20]` representing cell states (0 = empty, 1 = debris, 2 = compacted junk)
- [ ] `BoardRenderer.cs` — renders the board state using a `Tilemap` or sprite grid
- [ ] Board procedural fill: fill rows 5–20 with ~70% debris density using seeded RNG
- [ ] Visual distinction: empty cells = dark bg, debris = grey tile, compacted junk = brown cracked tile

### 1B — Piece System
- [ ] `TetrominoData.cs` — define all 7 tetrominoes as cell offset arrays in all 4 rotations (SRS)
- [ ] `PieceSpawner.cs` — 7-bag randomizer, spawns next piece at top-center
- [ ] `ActivePiece.cs` — holds current piece state (type, position, rotation)
- [ ] Input handling: Left/Right move, Up/Z rotate CW, X rotate CCW, Down soft drop, Space hard drop
- [ ] Ghost piece rendering (transparent version at landing position)
- [ ] Wall kicks (basic SRS)

### 1C — Collision & Locking
- [ ] `CollisionChecker.cs` — validates piece position against board bounds and filled cells
- [ ] Piece locks when it can no longer move down
- [ ] On lock: write piece cells into board array, trigger line clear check, spawn next piece
- [ ] Lockout detection: if new piece spawns overlapping a filled cell → run ends

### 1D — Line Clearing & Depth
- [ ] `LineClearSystem.cs` — scan all rows after each piece locks, detect full rows
- [ ] Full rows: animate clear (flash), remove, shift rows above down, reveal new row at bottom
- [ ] New row generation: random debris fill ~60–75% density (gets denser with depth)
- [ ] `DepthCounter` increments by 1 per cleared line
- [ ] Scrap awarded per clear (1 line = 10, 2 = 30, 3 = 60, 4 = 100)
- [ ] Compacted Junk: first clear sets to "cracked" state (still blocks), second clear removes

### 1E — Timer
- [ ] `DiveTimer.cs` — countdown from 90 seconds, displayed prominently in UI
- [ ] Timer stops on run end (lockout or expiry)
- [ ] Every 10 depth: +4 seconds added (depth milestone refill)
- [ ] UI: timer bar + numeric display, turns red below 15 seconds

### 1F — Run End & Basic Upgrade Screen
- [ ] On run end: show "Run Summary" overlay (depth reached, Scrap earned)
- [ ] "Continue" button loads `UpgradeScene` (or a placeholder panel)
- [ ] Upgrade screen stub: display Parts balance, "Start Dive" button, no actual upgrades yet
- [ ] `GameState.cs` — persistent singleton (DontDestroyOnLoad) holding Parts balance, depth record

### Prototype Checkpoint 1
**Can you:** Place pieces on a pre-filled board, clear lines, earn Scrap, watch the timer, and reach the upgrade screen stub?
**Fun test:** Play 5 runs. Does the pre-filled board feel like a puzzle or a wall? Adjust starting fill density if needed.

---

## Phase 2 — Creatures
**Goal:** Add Worm and Goblin. Validate that creatures add tension, not misery.
**Time estimate:** 2–3 days

### 2A — Creature Framework
- [ ] `CreatureBase.cs` — abstract class with: cell positions (list of Vector2Int), Activate(), OnLineNearComplete(), OnCleared(), ScrapValue
- [ ] `CreatureRegistry.cs` — tracks all active creatures on the board
- [ ] Board generation updated: creature spawn points seeded during procedural fill (1 per 8 rows default)
- [ ] Creature cells flagged in the board array (value 3+), visually distinct

### 2B — Worm
- [ ] `Worm.cs` extends `CreatureBase`
- [ ] Occupies 2–3 cells in an L or S shape (random per spawn)
- [ ] `BoardMonitor` checks: after each piece placement, scan rows — if any row is 1 cell from complete AND Worm occupies it → trigger Worm shift
- [ ] Worm shift: moves all its cells 1 column in a random valid direction
- [ ] Animation: brief flinch/slide anim before shifting
- [ ] Cleared when all its cells are in simultaneously cleared rows

### 2C — Goblin
- [ ] `Goblin.cs` extends `CreatureBase`
- [ ] Occupies 1 cell
- [ ] Passive timer: every 8–12 seconds (randomized), picks a random Debris cell in the active viewport and moves it to a random empty cell
- [ ] Only activates if the Goblin cell itself has not been cleared
- [ ] Animation: cell briefly highlights before the debris cell teleports
- [ ] Cleared when its row is cleared

### 2D — Creature Scrap Rewards
- [ ] When a creature is cleared, bonus Scrap added (Worm 2×, Goblin 3×)
- [ ] Brief visual popup showing bonus amount

### Prototype Checkpoint 2
**Can you:** Encounter Worms breaking near-complete lines and Goblins shuffling the board?
**Fun test:** Play 10 runs with creatures. Ask: Are creatures interesting obstacles or just annoying? Tune spawn rate and timing here.

---

## Phase 3 — Upgrade Loop
**Goal:** Meaningful choices between runs. Validate the meta-progression hook.
**Time estimate:** 2–3 days

### 3A — Currency Persistence
- [ ] On run end, Scrap converts to Parts at 10:1 rate
- [ ] Parts persist in `GameState` (PlayerPrefs or serialized JSON save)
- [ ] Parts balance displayed in Upgrade Screen

### 3B — Skill Tree (Prototype Subset)
Implement only these nodes for the prototype — enough to feel meaningful without taking weeks:

**Clock Branch:**
- [ ] Extra Time I (+15s) — Cost: 2 pts
- [ ] Extra Time II (+15s) — Cost: 4 pts
- [ ] Milestone Refill+ (+2s per milestone) — Cost: 3 pts

**Toolkit Branch:**
- [ ] Extraction Charge +1 — Cost: 3 pts
- [ ] Lock Delay (0.3s) — Cost: 4 pts

**Landfill Branch:**
- [ ] Lighter Fill (Compacted Junk –10%) — Cost: 2 pts
- [ ] Sparse Creatures (Creature rate –5%) — Cost: 3 pts

- [ ] `SkillTree.cs` — stores purchased nodes, applies effects to `GameState`
- [ ] Upgrade Screen UI: 7 nodes displayed as simple buttons with name, cost, description, purchased state
- [ ] Disable nodes player cannot afford

### 3C — Extraction Ability
- [ ] `ExtractionSystem.cs` — tracks charge count, activated with E key
- [ ] Piece outline turns to negative silhouette in extraction mode
- [ ] On placement: remove Debris cells under piece footprint (ignore non-Debris)
- [ ] Deduct 1 charge, deactivate mode
- [ ] Charge counter shown in UI (icon × count)
- [ ] Charges refill +1 per depth milestone (10 rows)

### Prototype Checkpoint 3
**Can you:** Finish a run, spend Parts on upgrades, and feel meaningfully stronger on the next run?
**Fun test:** Play 20 runs total. Ask: Do you want to keep playing? Does each run feel like progress? This is the core question.

---

## Phase 4 — Polish Pass (Prototype Only)
**Goal:** Make the prototype presentable enough for honest playtesting.
**Time estimate:** 1–2 days

- [ ] Line clear screen flash + shake (magnitude scales with line count)
- [ ] Piece lock animation (brief thud/impact)
- [ ] Timer urgency: Miasma visual rising from below when timer < 20s, music speed-up
- [ ] Run summary screen shows: depth, lines cleared, creatures killed, Scrap earned, best depth (all-time)
- [ ] Basic sound effects: piece move, piece lock, line clear, creature activate, timer warning
- [ ] Depth milestone notification: "+4s" popup on milestone reach
- [ ] Prevent input during animations (lock brief input window after line clear)

---

## What the Prototype Intentionally EXCLUDES

These are post-prototype features. Do not build them during the prototype phase:

- Pentominoes, Trominoes, Dominoes (piece progression)
- Slime Mold, Stone Giant (advanced creatures)
- Relic currency and Relic Tree
- Prestige system
- Visually distinct board strata / art direction
- Music (placeholder sfx only)
- Full 20-node skill tree
- Capstone abilities (Second Wind, Phase Shift, Salvager's Eye)
- Save/load system beyond PlayerPrefs

---

## Prototype Success Criteria

After Phase 3, ask these questions:

| Question | Good Sign | Bad Sign |
|---|---|---|
| Do runs feel tense? | Timer causes real urgency | Timer feels irrelevant or oppressive |
| Do creatures feel fair? | Creatures create interesting decisions | Creatures feel random and punishing |
| Is the board satisfying? | Pre-fill creates fun puzzles | Pre-fill feels like instant lockout |
| Does the loop hold? | You want to start the next run | Each run feels like a chore |
| Is Extraction fun? | Extraction creates clever moments | Extraction feels like undo, not strategy |

If any Bad Signs emerge, fix them before adding content. A broken core loop cannot be fixed with more content.

---

## Recommended First Session

Start with **Phase 0 + Phase 1A + 1B**. Get a piece falling on an empty board before touching anything else. Then add the pre-fill. The instinct to add creatures or upgrades early is the main risk — resist it.
