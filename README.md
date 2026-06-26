# 吞噬球v0.1版本
- 实现了游戏基本逻辑以及完整流程
- 还有很多地方需要优化与细节需要添加，后续有空会完善，争取推出正式版


# 吞噬球v0.2版本

## 概述

对原始单一脚本架构进行重大重构，采用模块化的事件驱动架构，新增完整的存档/读档系统，并用自定义史莱姆外观替换原始球体。

---

## 架构

### 事件系统 (`Core/EventBus.cs`)
静态事件总线，替代跨脚本直接调用。共五个事件通道：

| 事件 | 发布者 | 订阅者 |
|---|---|---|
| `OnEat(GameObject, EatableType, Vector3, Bounds)` | EatingSystem | EffectManager, SoundManager, SaveManager |
| `OnPlayerSizeChanged(float size, float radius)` | GrowthSystem | CountdownUI, SaveManager |
| `OnGameStateChanged(GameState prev, GameState curr)` | GameStateManager | GameOverUI, SaveManager |
| `OnCountdownEnded()` | CountdownUI | GameStateManager |
| `OnScoreChanged(int score)` | GameOverUI | SaveManager |

```csharp
// 发布事件
EventBus.PublishEat(eatenObject, type, position, bounds);

// 订阅事件
EventBus.OnEat += HandleEat;
```

### 游戏状态机 (`Core/GameStateManager.cs`)
单例，管理四种游戏状态及其合法转换：

```
OpeningCutscene ──→ Playing ──→ GameOver
                     ↕
                   Paused
```

- 根据状态设置 `Time.timeScale` 和鼠标锁定
- 非法状态转换会被记录并忽略

### 存档系统 (`Core/SaveManager.cs`, `Core/SaveData.cs`)

| 按键 | 功能 |
|---|---|
| **F5** | 保存当前游戏状态（玩家位置/大小、剩余可吞噬物体、倒计时、水面高度） |
| **F9** | 读取已保存的游戏状态 |
| **自动读档** | 游戏启动时，若存在 `devour_save.json`，延迟1帧后自动恢复状态 |

存档数据字段：
- `playerPosX/Y/Z`、`playerSize`、`playerRadius`
- `remainingTime`、`waterLevelY`
- `remainingEatableIds`（基于层级路径的稳定对象标识）
- `highScore`、`totalObjectsEaten`、`totalPlayTime`（持久化统计）

游戏结束时自动删除存档文件。存在存档时跳过开场动画。

### Player 脚本拆分

**重构前：** `PlayerEater`（上帝类 — 200+ 行，管理所有逻辑）

**重构后：**

| 脚本 | 职责 |
|---|---|
| `PlayerEater.cs` | 协调器：连接 GrowthSystem 和 EatingSystem，订阅 OnEat 事件处理特效和音效，自动添加 SlimePlayerBridge |
| `Player/GrowthSystem.cs` | 管理 Size、Radius、localScale、Y 坐标、相机高度缩放。发布 `OnPlayerSizeChanged` 事件 |
| `Player/EatingSystem.cs` | `[RequireComponent(GrowthSystem)]`。在 Update 中遍历 `EatableManager.All`，检测距离与 `GrowthSystem.Radius`，触发 Grow() |

### 标签/类型映射 (`EatableTypeExtensions.cs`)
tag ↔ EatableType 转换的唯一数据源：
```csharp
EatableType type = EatableTypeExtensions.TagToType("Human");
string tag = EatableTypeExtensions.ToTag(EatableType.Human);
```

---

## 修改的脚本汇总

| 文件 | 改动内容 |
|---|---|
| `PlayerController.cs` | 状态守卫：`FixedUpdate`/`Update` 在非 Playing 状态下拦截。鼠标锁定回退逻辑 |
| `CountdownUI.cs` | 使用 `OnCountdownEnded` 事件。暴露 `GetRemainingTime()`/`SetRemainingTime()` |
| `GameOverUI.cs` | 订阅 `OnGameStateChanged`。读取 `GrowthSystem.Size` 作为分数 |
| `OpeningCutscene.cs` | 新增 `ForceSkip()` 供读档使用。`Start()` 中检测存档直接跳过动画 |
| `PauseMenu.cs` | 使用 `GameStateManager.SetState(Paused/Playing)` |
| `AirWall.cs` | `IsPlaying()` 守卫。修复 static bool → 实例字段 |
| `CitySink.cs` | 使用 `GameStateManager` 触发 GameOver。暴露 `WaterLevelY` 属性 |
| `EffectManager.cs` | 新增 `EatableType` 重载。修复 bounds→Vector3 错误 |
| `SoundManager.cs` | 新增 `PlayEatSound(EatableType)` 重载 |
| `Eatable.cs` | `AutoDetectType()` 委托给 `EatableTypeExtensions.TagToType()` |

---

## Slime 文件夹依赖 （后续升级方向）

`Assets/Slime/` 文件夹包含原始 PBF 流体模拟源码作为参考（当前简化的史莱姆外观方案不使用）：

| 文件 | 用途 |
|---|---|
| `Slime_PBF.cs` | PBF 模拟 + Marching Cubes + 渲染 |
| `Jobs_Simulation_PBF.cs` | Burst 编译的 SPH 核函数、密度、位置修正 |
| `Jobs_Reconstruction.cs` | 表面重建、密度网格、各向异性平滑 |
| `LMarchingCubes.cs` | Marching Cubes 网格生成 |
| `Jobs_Effects.cs` | 连通分量分析、气泡粒子系统 |
| `Eigen.cs` | 各向异性滤波的特征值分解 |
| `Slime.mat` / `Bubbles.mat` / `Particle.mat` / `face.mat` | 材质 |
| `face.obj` / `eyes.png` | 面部模型和贴图 |

`ProjectSettings/ProjectSettings.asset`：`allowUnsafeCode: 1`（`LMarchingCubes.cs` 需要）

---

## 场景配置

需要手动在场景中添加两个单例 GameObject（场景默认不存在）：

1. **GameStateManager** — 挂载 `GameStateManager` 组件
2. **SaveManager** — 挂载 `SaveManager` 组件

两者均在 `Awake()` 中设置 `DontDestroyOnLoad`。

