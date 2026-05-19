# LANDFILL — Game Design Document
*Version 1.0 | May 2026*

---

## 1. Vision Statement

**Landfill** is an incremental idle game disguised as a falling-block puzzle. The player descends into an infinite, pre-filled pit one run at a time, clearing lines to earn currency and upgrade their toolkit between loops. The further down you dig, the harder the landfill fights back — but each run leaves you permanently stronger.

**Core Fantasy:** *You are a lone scavenger digging through an endless cursed landfill, trying to break through layers of compacted junk before the buried things crawl back out.*

**Pillars:**
1. **Satisfying puzzle feel** — the board must always feel solvable with good play.
2. **Meaningful progression** — every run should feel faster/stronger than the last.
3. **Emergent tension** — creatures and the timer create pressure that feels fair, not arbitrary.

---

## 2. Core Loop

```
RUN PHASE
  ↓
Place pieces → Clear lines → Descend deeper → Earn Scrap
  ↓
[Timer expires or board locks out]
  ↓
UPGRADE PHASE
  ↓
Convert Scrap → Parts → Spend on Skill Tree
  ↓
[New run begins — board resets, upgrades persist]
```

Each loop is a single **Dive**. The board is procedurally generated fresh each dive but gets harder as meta-progression depth increases.

---

## 3. Game Board & Core Mechanics

### 3.1 The Board

- **10 columns wide**, standard Tetris layout.
- No visible top ceiling — pieces spawn at the top.
- The board spawns **pre-filled from row 5 downward** — the player never sees an empty board.
- The player's active zone is the top 4–6 rows. Pieces spawn and fall from here.
- The board does **not scroll**. When a line is cleared, the **depth counter** advances by 1 and a new semi-random row is revealed at the bottom of the viewport.
- A **Depth Meter** on the side shows how far down the current dive has reached.
- Visible board height: **20 rows** (standard Tetris height). The viewport reveals rows as you descend.

### 3.2 Cell Types

| Type | Appearance | Behavior |
|---|---|---|
| Empty | Black | Placeable |
| Debris | Grey block | Standard fill, 1 clear to remove |
| Compacted Junk | Dark brown, cracks | 2 line clears to fully remove (first clear = cracked state) |
| Creature Cell | Animated | See Section 5 |

### 3.3 Piece Placement

- Standard SRS rotation system.
- Hard drop (Space) and soft drop (Down arrow).
- Ghost piece shows landing position.
- **No lock delay initially** — piece locks the moment it lands. Lock delay unlockable via skill tree.
- Gravity starts slow (1 cell per second). Upgradeable.

### 3.4 Line Clearing & Depth

- Clearing a full row awards **Scrap** and advances **Depth** by 1.
- Clearing multiple rows simultaneously awards bonus Scrap:

| Lines Cleared | Scrap Multiplier |
|---|---|
| 1 | 1× |
| 2 | 3× |
| 3 | 6× |
| 4 | 10× |

- Cleared rows cause all cells above to fall down (standard Tetris gravity).
- **Compacted Junk** on its first clear transitions to a cracked visual — it counts toward line completion but is not removed until the second line clear through that row.

### 3.5 Run End Conditions

1. **Timer expires** — run ends, go to Upgrade screen.
2. **Board lockout** — no valid placement exists for the incoming piece. Run ends immediately.

---

## 4. The Timer & Run Structure

- Each run starts with a **Dive Timer** (default: 90 seconds).
- A visual indicator — rising **Miasma** (toxic fog from below) — shows time pressure visually in addition to the countdown.
- When the Miasma reaches the active zone, it's a visual warning the timer is critical (below 15 seconds).
- **Depth Milestone Bonuses:** Every 10 rows descended, the timer refills by **+4 seconds**, rewarding consistent clearing.
- Timer upgrades are the primary early skill tree investment.

---

## 5. Creatures

Creatures are embedded in pre-filled rows. They activate when disturbed — a piece lands adjacent to them or a line near them is almost complete.

### 5.1 Worm
- **Cells:** 2–3 connected cells in an L or S shape.
- **Trigger:** When the row it occupies is 1 cell away from a clear.
- **Action:** Shifts 1 cell laterally, breaking the near-complete line.
- **Counter:** Isolate it or clear its entire row in a multi-line wipe.
- **Scrap Value:** 2× debris cells.

### 5.2 Goblin
- **Cells:** 1 cell, faint animated glow.
- **Trigger:** Passive timer — activates every 8–12 seconds.
- **Action:** Picks up a loose Debris cell from anywhere in the active board region and teleports it to a random empty cell, creating a gap.
- **Counter:** Clearing the Goblin's row removes it. Cannot survive a line clear.
- **Scrap Value:** 3× debris cells.

### 5.3 Slime Mold *(Unlocked: Depth Milestone 50)*
- **Cells:** 1 cell seed.
- **Trigger:** Passive timer — spreads to one orthogonally adjacent empty cell every 15 seconds.
- **Action:** Fills empty cells with soft debris (standard Debris). Cannot spread diagonally.
- **Counter:** Clearing any cell it occupies removes the entire connected colony.
- **Scrap Value:** 1.5× per cell removed.

### 5.4 Stone Giant *(Unlocked: Depth Milestone 150)*
- **Cells:** 2×2 block.
- **Trigger:** When one of its two rows is cleared without the other.
- **Action:** Regenerates the cleared row after 5 seconds.
- **Counter:** Both rows must be cleared simultaneously.
- **Scrap Value:** 8× per cell (32 Scrap total).

### 5.5 Creature Density Scaling

| Meta-Progress Depth | Creatures per Rows |
|---|---|
| 0–50 | 1 per 8 rows |
| 50–150 | 1 per 6 rows |
| 150–300 | 1 per 4 rows |
| 300+ | 1 per 3 rows |

New creature types gate behind dive depth milestones, introducing threats gradually.

---

## 6. Piece Progression

Players begin with standard Tetrominoes. Smaller pieces are power unlocks — harder to use but more precise.

| Tier | Piece Type | Pool Change |
|---|---|---|
| Default | Tetrominoes (7 pieces) | Full standard bag |
| Unlock 1 | + Pentominoes (18 pieces) | Added to weighted bag |
| Unlock 2 | + Trominoes (2 pieces: L, I) | Added to weighted bag |
| Unlock 3 | + Dominoes (1×2 piece) | Added to weighted bag |
| Unlock 4 | + Monomino (1×1) | Rare drop only, not in bag |

Unlocking a tier does **not** replace the previous tier — it adds to the weighted pool. Players can tune weights in the skill tree.

Pentominoes award a **+10% Scrap bonus** on placement (rewarding adaptation to harder pieces).

---

## 7. The Extraction Ability

> *"You're not just filling — you're excavating."*

### 7.1 Mechanics

- The player holds a limited number of **Extraction Charges** (default: 2 per run).
- Activating Extraction mode (press E) changes the current piece into a **negative silhouette**.
- Placing the piece in Extraction mode **removes** existing filled cells in that shape instead of adding to them.
- After placing, Extraction mode deactivates and the player uses one charge.

### 7.2 Rules & Restrictions

- Can only remove **Debris** cells. Cannot remove:
  - Compacted Junk
  - Creature cells
  - Cells that would create a floating island (i.e., cells above must have support below — unless the "Falling Debris" upgrade is purchased)
- **Falling Debris Upgrade:** Removes the support restriction. Unsupported cells fall, potentially triggering accidental line clears. High risk, high reward.
- Charges refill by **+1 per Depth Milestone** (every 10 rows descended).

### 7.3 Design Rationale

Scarcity (2 charges base) makes Extraction a decision, not a solution. Its primary uses are:
1. **Surgical gap creation** for an upcoming I-piece or multi-clear setup.
2. **Creature isolation** — dig around a Worm without triggering it.
3. **Emergency undo** — remove a badly placed piece if it was the last one dropped (only works if no new piece has been placed since).

---

## 8. Currency & Economy

### 8.1 Scrap (Run Currency)
- Earned during runs by clearing lines and removing creatures.
- **Does not persist** between runs as raw Scrap.
- At the Upgrade Screen, Scrap converts to **Parts** at a base rate of 10:1.

### 8.2 Parts (Persistent Meta-Currency)
- Spent in the Skill Tree between runs.
- Conversion rate from Scrap → Parts improves with skill tree upgrades.

### 8.3 Relics (Late-Game Currency)
- Rare drops from Stone Giants and first-time depth milestone clears.
- Spent in the second tier of the Skill Tree (Relic Tree).
- Cannot be farmed — only from milestone first clears.

---

## 9. Skill Tree

The player earns **1 Skill Point per run** plus bonus points for depth milestones. Skill Points are spent at the Upgrade Screen.

### Branch A: The Clock (Time & Efficiency)
| Node | Cost | Effect |
|---|---|---|
| Extra Time I | 2 | Dive Timer +15s |
| Extra Time II | 4 | Dive Timer +15s |
| Milestone Refill+ | 3 | Depth milestone refill +2s |
| Slow Gravity | 2 | Piece gravity –20% |
| Lock Delay | 4 | 0.3s lock delay before piece sets |
| **Capstone: Second Wind** | 8 | Once per run: when timer expires, gain +10s if Depth ≥ 20 |

### Branch B: The Toolkit (Pieces & Extraction)
| Node | Cost | Effect |
|---|---|---|
| Pentomino Pool | 3 | Unlocks Pentominoes in bag |
| Extraction+ | 3 | Extraction charges +1 |
| Tromino Pool | 5 | Unlocks Trominoes in bag |
| Falling Debris | 4 | Extraction can create unsupported cells (they fall) |
| Piece Tuner | 3 | Unlock bag weight sliders in Upgrade Screen |
| **Capstone: Phase Shift** | 10 | Once per run: choose any piece from the next 5 in queue |

### Branch C: The Landfill (Board Interaction)
| Node | Cost | Effect |
|---|---|---|
| Lighter Fill | 2 | Compacted Junk spawn rate –10% |
| Creature Sense I | 3 | Creatures revealed 2 rows before entering viewport |
| Lighter Fill II | 4 | Compacted Junk spawn rate –10% |
| Sparse Creatures | 3 | Creature spawn rate –5% |
| Scrap Surge | 3 | Depth milestone Scrap bonus +25% |
| **Capstone: Salvager's Eye** | 8 | Creatures always visible from top of viewport, never hidden |

### Relic Tree (Late Game — Requires Relics)
| Node | Cost | Effect |
|---|---|---|
| Wide Board | 3 Relics | Board width +1 column |
| Shallow Fill | 2 Relics | Starting fill level –5% (more empty rows at top) |
| Worm Weakness | 2 Relics | Worms freeze 3s when a line adjacent to them clears |
| Conversion Boost | 3 Relics | Scrap → Parts rate +50% |
| Second Board | 5 Relics | Experimental: second parallel board, pieces alternate (see Open Questions) |

---

## 10. Upgrade Screen (UI)

- Triggered automatically when a run ends.
- Shows: Run summary (Depth reached, Scrap earned, creatures killed), Scrap → Parts conversion, Skill Tree.
- Player can inspect each node for its full description before spending.
- A **"Start Dive"** button begins the next run.
- No time pressure in the Upgrade Screen — it's a calm, deliberate space.

---

## 11. Progression Arc

| Phase | Depth Milestone | New Content |
|---|---|---|
| Early | 0–50 rows | Tetrominoes, Worms, Goblins, Clock branch |
| Mid | 50–150 rows | Pentominoes, Slime Mold, Extraction ability, Toolkit branch |
| Late | 150–300 rows | Trominoes/Dominoes, Stone Giants, Relic Tree, Landfill branch |
| Endgame | 300–500 rows | Procedural creature combos, dense Compacted Junk |
| Prestige | 500 rows | Reset skill tree, Part savings → permanent multiplier |

### Prestige
At dive depth 500 cumulative (across all runs), the player may **Prestige**:
- Skill tree resets.
- Accumulated Parts convert to a **Prestige Multiplier** (permanent, stacks each prestige).
- Difficulty resets to early phase feel.
- Board visuals shift to a deeper stratum aesthetic.

---

## 12. Atmosphere & Presentation

- **Visual Style:** Gritty pixel art. Strata deepen visually — shallow layers show garbage/junk, mid layers show compacted earth and metal, deep layers reveal ruins and stranger things.
- **Creatures** are hand-animated with warning animations before acting (Worm flinches, Goblin eyes dart).
- **Soundtrack:** Ambient industrial drone. Gains urgency when timer drops below 20 seconds.
- **UI During Play:** Minimal — Depth Meter, Timer, Scrap counter, Extraction charge icons. All else in the Upgrade Screen.
- **Screen Flash:** Line clears trigger a brief white flash on the cleared rows. Multi-clears add screen shake proportional to count.

---

## 13. Open Design Questions

1. **Extraction mode: current piece or dedicated cursor?** Cursor = more precise but removes piece constraint. Playtesting required.
2. **Creature aggression vs. density:** Few high-disruption creatures vs. many low-disruption ones. Current design favors fewer, higher-value targets.
3. **Second Board (Relic capstone):** High complexity — consider as a separate game mode rather than passive unlock.
4. **Idle component:** Does anything happen during the Upgrade Screen? Passive Scrap trickle could deepen incremental feel without adding scope.
5. **Removal ability naming:** "Extraction" is functional but may need a more evocative name tied to the landfill theme (e.g., "Excavate", "Scavenge").

---

*GDD maintained alongside the Unity project. Update version number on significant design changes.*
