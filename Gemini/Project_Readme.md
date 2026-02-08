# Bubble Touch 60 - Project Status & Documentation

## 1. Project Overview
- **Title**: Bubble Touch 60
- **Genre**: 2D Arcade / Puzzle
- **Goal**: Touch bubbles in ascending numerical order (1 -> 2 -> 3...) within 60 seconds.
- **Key Mechanics**: Combo system, Time Attack, Dynamic Difficulty.

## 2. Core Systems

### A. Bubble Manager (`BubbleManager.cs`)
Handles bubble spawning, positioning, and difficulty scaling.

- **Grid System**: 
  - Size: 6 columns x 7 rows
  - Cell Size: 1.5 x 1.5
  - **Visible Area**: Rows Y=1 to Y=6 are used. Row Y=0 is excluded to prevent visibility issues at the bottom of the screen.
  - **Origin**: `(-4.5, -3.5)` - Centered horizontally, shifted up vertically.

- **Spawning Logic (Critical Fixes)**:
  1. **Priority Spawning**: Always checks if the `CurrentTargetNumber` exists on screen. If missing, it is spawned immediately. This prevents "skipping" or game-locking scenarios.
  2. **Sequential Spawning**: Bubbles are spawned one by one with a `0.1s` delay using a Coroutine (`UpdateBubbleCountRoutine`) to avoid visual clutter and lag spikes.
  3. **Continuity**: If the target exists, spawns `MaxNumberOnScreen + 1` to ensure a continuous stream of numbers.

- **Position Management**:
  - Uses `occupiedCells` (HashSet) to track filled grid slots.
  - **Fix**: Bubbles now store their assigned `GridPosition`. When popped, this stored value is used to clear `occupiedCells`, eliminating floating-point errors and "memory leaks" where grid cells remained marked as occupied indefinitely.

- **Difficulty (Survival Mode)**:
  - Base Bubble Count: 5
  - **Decrease Rule**: Count decreases by 1 for every 15 combos (`decreaseThreshold`), increasing difficulty by reducing available options/time buffer.

### B. Game Manager (`GameManager.cs`)
- Manages Game State (Menu, Playing, GameOver).
- Tracks Score (Target Number), Combo, and Remaining Time.
- Handles Global Events: `OnGameStart`, `OnGameEnd`, `OnTargetNumberChanged`.

### C. Bubble (`Bubble.cs`)
- Handles interaction (Touch/Click).
- Validates input against `GameManager`.
- **New Feature**: Stores `Vector2Int GridPosition` for accurate pool return.

## 3. Current Configuration (Tuning)

| Setting | Value | Description |
| :--- | :--- | :--- |
| **Grid Size** | `(6, 7)` | Expanded width to 6 for better screen coverage. |
| **Grid Origin** | `(-4.5, -3.5)` | Adjusted for new width and to lift bubbles off the bottom bezel. |
| **Spawn Logic** | `y = 1` start | Loops start from y=1 instead of y=0 in `GetRandomEmptyPosition`. |
| **Start Bubbles** | `5` | Initial number of bubbles on screen. |
| **Combo Threshold** | `15` | Bubbles decrease every 15 combos. |

## 4. Troubleshooting History

### Issue 1: Game Stuck (Next number not appearing)
- **Cause**: Random spawning didn't guarantee the *next* required number appeared.
- **Fix**: Implemented logic to strictly check for `CurrentTargetNumber` absence and spawn it with top priority.

### Issue 2: Spawning Stops (Grid Full error when empty)
- **Cause**: `WorldToGrid` calculation on bubble removal had floating-point errors, failing to clear `occupiedCells`.
- **Fix**: Added `GridPosition` property to `Bubble` class to cache the exact grid coordinate.

### Issue 3: Bottom Bubbles Invisible
- **Cause**: Grid Y origin was too low, and `y=0` row was obscured by screen edges/UI.
- **Fix**: Raised `gridOrigin.y` to `-3.5` and modified spawn loop to start from `y=1`.

### Issue 4: Narrow Gameplay Area
- **Cause**: 5 columns left too much empty space on sides.
- **Fix**: Increased `Grid Size X` to 6 and adjusted `Origin X` to `-4.5` for centering.

## 5. Debugging
- **Log File**: `Assets/Scripts/log.txt`
- Detailed logs for Spawning, Grid Status, and Target Tracking are recorded here.

---
*Last Updated: 2026-02-08*
