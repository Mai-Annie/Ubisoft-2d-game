# Bamboo Bandits

A 2D local co-op puzzle-platformer built in Unity. Two players share a keyboard and must cooperatively carry bamboo poles across obstacles to a delivery zone before a timer expires. Larger bamboo scores more points but slows both players down and demands tight coordination through narrow spaces.

**Engine:** Unity (2D) &nbsp;|&nbsp; **Platform:** PC (Windows/Mac) &nbsp;|&nbsp; **Players:** 2 (shared keyboard)

| Player | Move | Jump | Drop |
|---|---|---|---|
| P1 | WASD | Space | E |
| P2 | Arrow Keys | Enter | Right Ctrl |

---

## Technical Documentation

### Architecture

The project is organized into four distinct layers of responsibility:

| Layer | Scripts |
|---|---|
| Input & Player | `PlayerMovement.cs`, `PlayerInputActions` (generated) |
| Physics / Gameplay Object | `Bamboo.cs`, `BambooEnd.cs`, `DeliveryZone.cs` |
| Session Management | `GameManager.cs`, `HUDManager.cs` |
| Data / Persistence | `LevelData.cs`, `LevelDatabase.cs` |
| UI Entry Point | `MainMenuManager.cs`, `MainMenuButtonManager.cs` |

Each layer communicates downward (managers talk to gameplay objects; gameplay objects talk to players) or inward through well-defined public methods. No script reaches across layers to read another's private state.

---

### Challenge 1 — Two-Player Input on a Single Keyboard

#### The Problem

Unity's legacy `Input.GetKey()` system has no concept of player identity — any script can read any key at any time. Routing two independent players through the same keyboard, with the same underlying `PlayerMovement` script, required a different approach entirely.

#### Solution: Unity Input System with Binding Masks

The project uses Unity's new Input System. A single `PlayerInputActions` asset defines one action map (`Player`) with actions for `Move`, `Jump`, and `Drop`. Each action has bindings registered under two **control schemes**: `PlayerOne` (WASD / Space / E) and `PlayerTwo` (Arrow Keys / Enter / Right Ctrl).

At runtime, each `PlayerMovement` instance filters its bindings at the asset level:

```csharp
inputActions.bindingMask = isPlayerOne
    ? InputBinding.MaskByGroup("PlayerOne")
    : InputBinding.MaskByGroup("PlayerTwo");
```

`InputBinding.MaskByGroup()` tells the Input System to only resolve bindings belonging to the named control scheme. Both players run identical code; only the binding mask differs. This means adding a third player later requires only a new control scheme in the asset, not new scripts.

#### Event-Driven Actions vs Polling

Continuous actions like `Move` are polled each frame with `ReadValue<Vector2>()`. One-shot actions — `Jump` and `Drop` — use event callbacks registered in `OnEnable` and unsubscribed in `OnDisable`:

```csharp
void OnEnable()  { inputActions.Player.Jump.performed += OnJump; }
void OnDisable() { inputActions.Player.Jump.performed -= OnJump; }
```

This distinction matters: polling a jump button can miss a fast press between frames, while an event fires exactly once per physical key press regardless of frame rate.

---

### Challenge 2 — Cooperative Bamboo Grab System

This was the most complex problem in the project, involving physics state, multi-object coordination, and a subtle race condition.

#### State Machine

Each bamboo pole moves through three states based on who is holding it:

```
FREE ──(first player grabs)──► ANCHORED ──(second player grabs)──► LINKED
                                   │                                    │
                                   └──────────(either drops)───────────┘
                                              (returns to ANCHORED or FREE)
```

- **FREE**: `Rigidbody2D` unconstrained, solid collider enabled, no players attached.
- **ANCHORED**: `Rigidbody2D` fully frozen (`RigidbodyConstraints2D.FreezeAll`), one player locked in place. The bamboo can't drift from physics while waiting for the second player.
- **LINKED**: Both players connected to the bamboo via `DistanceJoint2D`. The `Rigidbody2D` is unfrozen; the bamboo moves with the players. Solid collider disabled so the pole doesn't collide with the environment as a rigid body while being carried.

`Bamboo.cs` owns this state machine. The transition logic lives in `OnEndGrabbed()`:

```csharp
public void OnEndGrabbed(PlayerMovement player)
{
    bool leftFree  = leftEndPlayer == null;
    bool rightFree = rightEndPlayer == null;

    if (leftFree && rightFree)
    {
        // First grab: anchor this player, freeze bamboo
        leftEndPlayer = player;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        player.SetAnchored(true);
        player.SetHeldBamboo(this);
    }
    else if (!leftFree && rightFree && leftEndPlayer != player)
    {
        // Second grab from right end: unanchor first player, link both
        rightEndPlayer = player;
        leftEndPlayer.SetAnchored(false);
        rb.constraints = RigidbodyConstraints2D.None;
        player.SetHeldBamboo(this);
        LinkPlayers();
    }
    // Symmetric case: first player grabbed the right end
    else if (leftFree && !rightFree && rightEndPlayer != player)
    { ... }
}
```

The `leftEndPlayer != player` guard is critical — it prevents the same player from grabbing both ends, which led to the race condition described below.

#### The Race Condition: Simultaneous Trigger Firing

During testing, a panda walking toward a bamboo end would occasionally trigger *both* `LeftEnd` and `RightEnd` colliders in the same frame. This happened because the panda's circular collider was wide enough to overlap the sibling trigger on entry.

The fix is in `Bamboo.OnEndGrabbed()` itself: the guard `leftEndPlayer != player` means the second call (from the sibling trigger, same player) fails the condition and does nothing. The state machine rejects self-double-grabs by checking player identity, not just slot availability — no shared flag between `BambooEnd` scripts needed.

#### Child → Parent Communication

`BambooEnd` scripts (on `LeftEnd` and `RightEnd` child objects) detect when a player enters their trigger and call up to the parent:

```csharp
bamboo.OnEndGrabbed(player);
```

`bamboo` is cached in `BambooEnd.Awake()` via `GetComponentInParent<Bamboo>()`. This parent-owned coordination is the right design because `Bamboo` is the only object with full visibility of both ends' state. `BambooEnd` handles detection; `Bamboo` handles state transitions.

---

### Challenge 3 — Physics-Based Cooperative Carrying

#### DistanceJoint2D for Carrying

When both players grab the bamboo, `LinkPlayers()` adds two `DistanceJoint2D` components to the bamboo GameObject at runtime — one connecting to each player's `Rigidbody2D`:

```csharp
leftJoint = gameObject.AddComponent<DistanceJoint2D>();
leftJoint.connectedBody = leftRb;
leftJoint.autoConfigureDistance = true;
```

`autoConfigureDistance = true` measures the actual gap between the bamboo and each player at link time and locks that distance as the joint maximum. Players can move freely within that radius but cannot pull the bamboo further away. This gives the bamboo authentic physical weight — it lags, swings, and exerts force back on both players — without requiring a custom physics solver.

Joints are destroyed (not disabled) on release, because disabled joints still exist in memory and Unity's physics engine continues tracking them.

#### Solid Collider Toggling

A bamboo pole has two colliders: a trigger (`BambooEnd` detection) and a non-trigger box collider (environmental collision). While being carried, the solid collider is disabled:

```csharp
if (solidCollider != null) solidCollider.enabled = false; // on LinkPlayers
if (solidCollider != null) solidCollider.enabled = true;  // on full release
```

Without this, the pole's `BoxCollider2D` would slam into walls as players tried to carry it through narrow passages — fighting the players instead of acting as the thing they're carrying.

---

### Challenge 4 — Clumsiness as a Continuous Mechanic

#### Design Requirement

The heavier the bamboo, the clumsier both players become. This needed to feel gradual and physical, not like a binary penalty. Three bamboo sizes map to three clumsiness levels:

```csharp
private static readonly float[] ClumsinessValues = { 0.2f, 0.45f, 0.7f };
```

Applied via enum index: `ClumsinessValues[(int)size]` — O(1) lookup, no switch statement.

#### Two Dimensions of Clumsiness

**1. Input Responsiveness** — how quickly the player's velocity follows their stick input:

```csharp
float responsiveness = Mathf.Lerp(20f, 4f, clumsinessModifier);
moveInput = Vector2.Lerp(moveInput, targetMoveInput, Time.deltaTime * responsiveness);
```

`moveInput` is a smoothed version of raw input. At zero clumsiness the lerp factor is 20 — input tracks the stick almost instantly. At maximum clumsiness the factor drops to 4 — direction changes take noticeably longer to register.

**2. Top Speed** — the ceiling on how fast you can move:

```csharp
float effectiveSpeed = speed * (1f - clumsinessModifier * 0.4f);
rb.linearVelocity = new Vector2(moveInput.x * effectiveSpeed, rb.linearVelocity.y);
```

At maximum clumsiness, top speed drops to 60% of baseline — a ceiling effect separate from the responsiveness lag.

The two dimensions compound: carrying large bamboo means you both *start moving slowly* and *can't move fast even once you're at speed*. This creates the lumbering, coordinated quality that is the game's core feel.

---

### Data Structures and Algorithms

#### Enum-Indexed Lookup Arrays

Point values and clumsiness modifiers are stored in `static readonly` arrays indexed by the `BambooSize` enum cast to `int`:

```csharp
public enum BambooSize { Small, Medium, Large }  // 0, 1, 2

private static readonly int[]   PointValues      = { 10, 25, 50 };
private static readonly float[] ClumsinessValues = { 0.2f, 0.45f, 0.7f };

public int   PointValue         => PointValues[(int)size];
public float ClumsinessModifier => ClumsinessValues[(int)size];
```

This avoids a switch statement and is naturally extensible — adding a new size means adding an enum value and a corresponding entry in each array, with no branching logic to update. `static readonly` allocates the array once per type across all bamboo instances in the scene.

#### ScriptableObject Asset Graph

Level data is stored in `LevelData` ScriptableObjects referenced by a `LevelDatabase` asset. `LevelDatabase` exposes three traversal methods — `GetLevelByScene`, `GetFirstAvailableLevel`, and `GetNextLevel` — all O(n) linear scans. With three levels this is correct; if the game grew to 50+ levels a `Dictionary<string, LevelData>` built in `Awake()` would be the appropriate upgrade.

ScriptableObjects survive scene transitions naturally as Unity assets — no `DontDestroyOnLoad`, no serialization, no save-file parsing. Writing to their fields at runtime updates the in-memory asset, persisting until the application quits. This makes them appropriate for session data (score, completion, attempts) while keeping the implementation minimal.

`LevelData` encapsulates its own mutation logic:

```csharp
public void RecordAttempt() => attempts++;

public void RecordCompletion(int score)
{
    isCompleted = true;
    if (score > highScore) highScore = score;
}
```

Callers (`GameManager`, `MainMenuManager`) invoke these methods rather than writing to fields directly, keeping data transition logic with the data.

#### Singleton Pattern with Lifecycle Safety

`GameManager` and `HUDManager` use singletons scoped to the current scene. Both implement `OnDestroy` cleanup:

```csharp
private void OnDestroy()
{
    if (Instance == this) Instance = null;
}
```

The `Instance == this` guard matters: if a duplicate enters the scene and `Awake` destroys it, `OnDestroy` fires on the duplicate. Without the guard, the duplicate's `OnDestroy` would null out the valid instance. This pattern makes the singleton robust to scene-loading order and prefab instantiation.

---

### What Makes It Enjoyable — From a Programming Perspective

#### Emergent Difficulty from Physics

The game's difficulty is not scripted — it emerges from the physics simulation. The `DistanceJoint2D` gives the bamboo real inertia: it lags, swings, and exerts force back on players. Two players at different heights find the bamboo hanging at an angle. Running in opposite directions pulls it taut. These interactions aren't coded — they're physics consequences of a well-constructed constraint graph.

#### Tension in the Drop Mechanic

Either player can drop their end at any time. `ReleasePlayer()` handles the asymmetric case: the dropping player is freed and their clumsiness cleared, but the remaining player is immediately re-anchored:

```csharp
PlayerMovement remaining = leftEndPlayer ?? rightEndPlayer;
if (remaining != null)
{
    rb.constraints = RigidbodyConstraints2D.FreezeAll;
    remaining.SetAnchored(true);
}
```

A mistimed drop traps your partner and freezes the bamboo mid-level. Players must communicate before dropping — the social friction is a direct product of how state is managed in code.

#### Scoring That Rewards Speed

```csharp
int timeBonus = Mathf.RoundToInt(timeRemaining);
score += pointValue + timeBonus;
```

Every second wasted is a point lost. Combined with clumsiness, players face a genuine risk/reward decision: take small bamboo and score quickly, or take large bamboo and score more if they can coordinate well enough. This tradeoff emerges entirely from the scoring formula and the physics — not from explicit difficulty settings.

---

### Key Technical Decisions

| Decision | Alternative Considered | Reason |
|---|---|---|
| Input System binding masks | Separate script per player with hardcoded keys | Single script handles both players; adding players only requires a new control scheme |
| Player identity check to resolve race condition | Shared `isBlocked` flag between `BambooEnd` siblings | Flag requires cross-sibling state; identity check keeps logic in the owning parent |
| `DistanceJoint2D` added at runtime | Pre-placed joint in Inspector | Runtime creation sets distance from actual player positions; pre-placed joint has a fixed, wrong distance |
| Solid collider disabled while carried | Physics layer change | Layer changes affect all collisions; disabling the specific collider is scoped and reversible |
| Enum-indexed arrays for bamboo data | Switch statement or dictionary | O(1) with no branching; extensible by appending entries |
| ScriptableObjects for level data | PlayerPrefs or JSON save file | No serialization code needed; Unity Editor doubles as a data editor |
| `OnDestroy` singleton cleanup | Singleton without cleanup | Scene reloads leave stale `Instance` references, causing `NullReferenceException` on next scene's first access |
