# DevourBall v0.2 — Refactor & Slime Visual

## Overview

Major refactoring of the original monolithic codebase into a modular, event-driven architecture with a full save/load system and a custom slime visual replacing the original ball.

---

## Architecture

### Event System (`Core/EventBus.cs`)
Static event bus replacing direct cross-script calls. Five event channels:

| Event | Publisher | Subscribers |
|---|---|---|
| `OnEat(GameObject, EatableType, Vector3, Bounds)` | EatingSystem | EffectManager, SoundManager, SaveManager |
| `OnPlayerSizeChanged(float size, float radius)` | GrowthSystem | CountdownUI, SaveManager |
| `OnGameStateChanged(GameState prev, GameState curr)` | GameStateManager | GameOverUI, SaveManager |
| `OnCountdownEnded()` | CountdownUI | GameStateManager |
| `OnScoreChanged(int score)` | GameOverUI | SaveManager |

```csharp
// Publish
EventBus.PublishEat(eatenObject, type, position, bounds);

// Subscribe
EventBus.OnEat += HandleEat;
```

### Game State Machine (`Core/GameStateManager.cs`)
Singleton managing four states with validated transitions:

```
OpeningCutscene ──→ Playing ──→ GameOver
                     ↕
                   Paused
```

- Applies `Time.timeScale` and cursor lock per state
- Invalid transitions logged and ignored

### Save System (`Core/SaveManager.cs`, `Core/SaveData.cs`)

| Key | Function |
|---|---|
| **F5** | Save current game state (player pos/size, remaining eatables, countdown, water level) |
| **F9** | Load saved game state |
| **Auto-load** | On game start, if `devour_save.json` exists, restores state after 1 frame |

Saved data fields:
- `playerPosX/Y/Z`, `playerSize`, `playerRadius`
- `remainingTime`, `waterLevelY`
- `remainingEatableIds` (hierarchy path for stable object identification)
- `highScore`, `totalObjectsEaten`, `totalPlayTime` (persistent stats)

Game Over auto-deletes the save file. Opening cutscene is skipped when a save exists.

### Player Scripts Split

**Before:** `PlayerEater` (God class — 200+ lines, managed everything)

**After:**

| Script | Responsibility |
|---|---|
| `PlayerEater.cs` | Coordinator: wires GrowthSystem + EatingSystem, subscribes to OnEat for effects/sounds, auto-adds SlimePlayerBridge |
| `Player/GrowthSystem.cs` | Size, Radius, localScale, Y position, camera height scaling. Publishes `OnPlayerSizeChanged` |
| `Player/EatingSystem.cs` | `[RequireComponent(GrowthSystem)]`. Iterates `EatableManager.All` in Update, checks distance vs `GrowthSystem.Radius`, triggers Grow() |

### Tag/Type Mapping (`EatableTypeExtensions.cs`)
Single source of truth for tag↔EatableType conversion:
```csharp
EatableType type = EatableTypeExtensions.TagToType("Human");
string tag = EatableTypeExtensions.ToTag(EatableType.Human);
```

---

## Slime Visual (`SlimePlayerBridge.cs`)

Replaces the original ball sphere with a custom slime-shaped mesh.

### Features
- **Custom mesh**: Lathe-generated oblate spheroid profile — wide skirt at base, rounded dome top, Y-axis symmetric
- **Material**: URP/Lit with emission for vibrant green slime look regardless of scene lighting
- **Growth linkage**: Scales by `GrowthSystem.Radius` — visual matches eatable detection range exactly
- **Bounce animation**: Sinusoidal scale oscillation (+5%) for a jiggly feel
- **Always upright**: Visual child GameObject counteracts Rigidbody rotation
- **Auto-cleanup**: Destroys any previous PBF simulation children on start
- **Editor auto-fill**: If `slimeMaterial` is unassigned, searches `Assets/Slime/` at startup

### Mesh Profile
```
Bottom (y=-1.0):  r=0.80  — flat base
      y=-0.75:   r=0.93  — bulge
      y=-0.40:   r=1.00  — max width (skirt)
      y= 0.00:   r=0.85  — equator
      y= 0.30:   r=0.60  — mid dome
      y= 0.60:   r=0.42  — upper dome
      y= 1.00:   r=0.30  — rounded crown
```

---

## Modified Scripts Summary

| File | Changes |
|---|---|
| `PlayerController.cs` | State guard: `FixedUpdate`/`Update` blocked when not Playing. Cursor lock fallback |
| `CountdownUI.cs` | Uses `OnCountdownEnded` event. Exposes `GetRemainingTime()`/`SetRemainingTime()` |
| `GameOverUI.cs` | Subscribes to `OnGameStateChanged`. Reads `GrowthSystem.Size` for score |
| `OpeningCutscene.cs` | `ForceSkip()` for save load. Save detection in `Start()` skips cutscene entirely |
| `PauseMenu.cs` | Uses `GameStateManager.SetState(Paused/Playing)` |
| `AirWall.cs` | `IsPlaying()` guard. Fixed static bool → instance field |
| `CitySink.cs` | `GameStateManager` for GameOver trigger. Exposes `WaterLevelY` property |
| `EffectManager.cs` | `EatableType` overloads. Fixed bounds→Vector3 bug |
| `SoundManager.cs` | `PlayEatSound(EatableType)` overload |
| `Eatable.cs` | `AutoDetectType()` delegates to `EatableTypeExtensions.TagToType()` |

---

## Dependencies (Slime Folder)

The `Assets/Slime/` folder contains the original PBF fluid simulation source files as reference (not used at runtime by the simplified slime visual):

| File | Purpose |
|---|---|
| `Slime_PBF.cs` | PBF simulation + Marching Cubes + rendering |
| `Jobs_Simulation_PBF.cs` | Burst-compiled SPH kernels, density, position correction |
| `Jobs_Reconstruction.cs` | Surface reconstruction, density grid, anisotropic smoothing |
| `LMarchingCubes.cs` | Marching Cubes mesh generation |
| `Jobs_Effects.cs` | Connected component analysis, bubble particle system |
| `Eigen.cs` | Eigenvalue decomposition for anisotropic filtering |
| `Slime.mat` / `Bubbles.mat` / `Particle.mat` / `face.mat` | Materials |
| `face.obj` / `eyes.png` | Face model and texture |

`ProjectSettings/ProjectSettings.asset`: `allowUnsafeCode: 1` (required by `LMarchingCubes.cs`)

---

## Scene Setup

Two manually-added singleton GameObjects required (not in scene by default):

1. **GameStateManager** — `GameStateManager` component
2. **SaveManager** — `SaveManager` component

Both have `DontDestroyOnLoad` in `Awake()`.
