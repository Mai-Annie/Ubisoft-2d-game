# Bamboo Bandits — Project Summary

## Game Overview

**Genre:** 2D local co-op puzzle-platformer  
**Engine:** Unity (2D)  
**Platform:** PC (Windows/Mac)  
**Target Completion:** 4 months  

Two players control pandas on a shared keyboard (P1: WASD/Space, P2: Arrow Keys/Enter), stealing bamboo from the zoo before the zookeeper catches them. Bamboo comes in three sizes (small, medium, large) — larger pieces score more points but create coordination challenges through tight spaces.

**Core mechanic:** The more bamboo you carry, the less mobile you are.

---

## Development History

### Session 1 — Project Setup (Jan 28)
- Set up Git repository with Unity-specific `.gitignore`
- Learned proper commit practices (commit `Assets/` and `ProjectSettings/`, not `Library/` or `Temp/`)
- Fixed `SceneManager` namespace error (`using UnityEngine.SceneManagement`)
- Learned about `[SerializeField]` vs `public` fields
- Set up scene loading via `SceneManager.LoadScene()`
- Resolved GitHub auth issues (PAT / SSH)

### Session 2 — Art vs. Code Decision (Feb 10)
- Decided to **prototype mechanics with placeholder art first**
- Confirmed the game should stay **2D** — the co-op, coordination, and "clumsy panda" aesthetic are fundamentally suited to 2D readability
- Visual style direction: **bold flat colors with strong outlines** (Overcooked-like) for co-op clarity

### Session 3 — Input System Setup (Feb 11)
- Migrated from legacy `Input.GetKey()` to Unity's **new Input System**
- Created `PlayerInputActions` asset with a `Player` action map
- Configured `Move` (2D axis) and `Jump` (button/Spacebar) actions
- Generated C# wrapper class from the asset
- Implemented ground detection via collision tags for jump gating

### Session 4 — Two-Player Controls (Feb 22)
- Added **control schemes** (`PlayerOne`, `PlayerTwo`) to the Input Actions asset
- Used `InputBinding.MaskByGroup()` in `Awake()` to filter bindings per player
- Removed conflicting legacy `PlayerInput` component from the GameObject
- Both players now run from the same `PlayerMovement` script, differentiated by `isPlayerOne` bool

### Session 5 — Clumsiness Design (Feb 23)
- Conceptual groundwork for the clumsiness mechanic
- Clumsiness should be a **spectrum tied to bamboo load**, not a binary on/off
- Key dimensions to tune: responsiveness, stopping behavior, turning momentum, control precision
- Decision: **define the desired feel first**, then identify which physics parameters to adjust (typically 2–3)
- Implementation deferred pending the bamboo grab mechanic

### Session 6 — Bamboo Collection Mechanic (Mar 6)
- Designed and implemented the cooperative grab system
- **Architecture:**
  - Bamboo has two child GameObjects: `LeftEnd` and `RightEnd`, each with a trigger collider and `BambooEnd` script
  - Child scripts report up to the parent `Bamboo` script (established inter-script communication pattern)
- **Grab flow:** First panda to touch an end gets **anchored**; when second panda grabs the other end, both are **linked**
- **Key bug fixed:** Both `BambooEnd` triggers fired simultaneously because the panda's physics body slid into the sibling trigger — solved by having the first grabbed end set `sibling.isGrabbed = true` immediately

### Session 7 — Fixing the Bamboo Connection (Mar 16)
- Fixed compile error: `BambooEnd` was passing `this` (a `BambooEnd`) instead of `collision.GetComponent<PlayerMovement>()` to `SetGrabbedEnd()`
- Refactored `Bamboo.cs`: replaced the `grabbedEnd` counter with **explicit player references** (`leftEndPlayer`, `rightEndPlayer`) per mentor feedback
- Completed `LinkPlayers()` using `DistanceJoint2D` components connecting both players to the bamboo
- Implemented `UnlinkPlayers()`
- **Bugs resolved during testing:**
  - Pre-existing `DistanceJoint2D` in Inspector caused duplicate joints → removed it
  - Sibling-blocking logic in `BambooEnd` was preventing the second player from grabbing → removed
  - Bamboo `Rigidbody2D` constraints needed to be **frozen on first grab, unfrozen on second** to prevent physics drift
  - `FixedUpdate` in `PlayerMovement` was still applying velocity when anchored → fixed
- **Scene-level fixes:** unfroze bamboo Rigidbody2D constraints in Inspector, added non-trigger `BoxCollider2D` for platform collision, set Rigidbody2D Interpolate to reduce jitter
- **Core mechanic confirmed working** ✅
- Remaining visual issues (player climbing bamboo, jitter) are placeholder-related, deferred until real assets

---

## Current Script State

### `PlayerMovement.cs`
- Handles movement (WASD / Arrow Keys) via new Input System with binding masks
- `SetAnchored(bool)` — stops movement input when anchored
- Ground detection via `OnCollisionEnter2D` / `OnCollisionExit2D` with "Ground" tag
- `FixedUpdate` respects anchored state (no velocity applied when anchored)

### `BambooEnd.cs`
- Trigger on each bamboo end (LeftEnd, RightEnd child objects)
- Detects "Player" tag on trigger enter
- Sets own `isGrabbed = true`, then calls `Bamboo.SetGrabbedEnd(PlayerMovement)`

### `Bamboo.cs`
- Stores explicit references: `leftEndPlayer`, `rightEndPlayer`
- First grab: anchors that player
- Second grab: unanchors first player, calls `LinkPlayers()`
- `LinkPlayers()`: adds `DistanceJoint2D` on bamboo connected to each player
- `UnlinkPlayers()`: removes joints (TODO: fully implement drop mechanic)

---

## Mentor Feedback (Vincent Martineau, received Mar 15)

1. **Compile error:** `BambooEnd` was passing `this` instead of a `PlayerMovement` to `SetGrabbedEnd` → **fixed**
2. **Avoid chaining:** `transform.parent.GetComponent<Bamboo>().SetGrabbedEnd(this)` — prefer storing the component first and logging an error if null, for safer debugging
3. **Use explicit references instead of a counter** in `Bamboo.cs` — store who holds which end; more reliable and enables future mechanics (e.g., blocking a player until they re-grab) → **implemented**
4. **Level management:** Hard-coding `Level 1` in MenuManager is a missed opportunity. Suggest a data-driven approach: a small `LevelData` object per level (name, completion status, attempts, availability). `playGame()` iterates the list to find the first uncompleted level. Scales to 100 levels; reusable for save data and level select UI → **to do**

---

## What's Next

- [x] **Level data system** — LevelData and LevelDatabase ScriptableObjects in Assets/Data/; wired into all scenes
- [x] **Drop / unlink mechanic** — P1: E, P2: Right Ctrl; re-anchors remaining player, freezes bamboo
- [x] **Clumsiness** — sluggish input lerp + speed reduction tied to bamboo size; applied on LinkPlayers
- [x] **Bamboo sizes** — Small (10pts/0.2), Medium (25pts/0.45), Large (50pts/0.7)
- [x] **3 levels** — L1: small bamboo open room 60s; L2: medium bamboo one wall 90s; L3: large bamboo two walls 120s
- [x] **UI** — runtime HUD with timer, score, and message overlay (TMPro)
- [ ] **Real art assets** — bold flat style with strong outlines; replace placeholders

---

## Key Principles & Learnings

- **Prototype mechanics first**, polish art last — working gameplay matters more for internship portfolios
- **Define feel before touching physics** — envision the player experience, then find the 2–3 parameters that produce it
- **Child → Parent communication pattern** — `BambooEnd` reports up to `Bamboo`, which coordinates cross-object behavior
- **Explicit references > counters** — store *who* grabbed *what* for reliability and future extensibility
- **Null-check before chaining** — always store `GetComponent<>()` results and log errors if null
- **Data-driven design** — level lists, not hardcoded strings, for scalability
- **New Input System requires careful binding mask management** — remove legacy `PlayerInput` components to avoid conflicts
