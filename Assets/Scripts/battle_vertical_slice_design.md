# 战斗垂直切片设计文档：移动、普攻、死亡、行动条

## 0. 目标

本阶段目标不是完成完整 roguelike 战棋系统，而是做出一个能稳定跑通的最小战斗闭环：

```text
创建战斗
-> 行动条推进
-> 选出当前行动角色
-> 玩家/AI 执行移动或普攻
-> 结算伤害与死亡
-> 结束回合
-> 下一个角色行动
-> 一方全灭后战斗结束
```

这个切片要为后续技能、Buff、AI、MVVM UI 留出扩展点，但暂时不实现复杂系统。

核心原则：

```text
1. Model 只保存运行时状态，不依赖 MonoBehaviour，不直接操作 UI。
2. System 负责规则计算和状态修改。
3. Command 表示玩家或 AI 的行动意图。
4. BattleCommandService 负责验证和执行 Command。
5. BattleState 保存战斗状态，但不持有 System。
6. 事件先做轻量结构，方便后续 UI 和日志订阅。
```

---

## 1. 本阶段范围

### 1.1 必须实现

```text
角色运行时状态：
- InstanceId
- Name
- Team
- Position
- CurrentHealth
- IsDead
- ActionGauge
- TurnActionState

地图：
- 矩形 GridMap
- GridPosition
- 格子可走性
- 格子占用查询

行动条：
- 根据 Speed 推进行动条
- 选择行动条满的角色
- 稳定处理多个角色同时满条
- 未死亡角色参与行动条

移动：
- 检查是否当前行动角色
- 检查目标格子在地图内
- 检查目标格子可走且未被占用
- 检查移动距离不超过 RemainingMove
- 执行后扣除 RemainingMove

普攻：
- 检查是否当前行动角色
- 检查攻击者未死亡
- 检查目标未死亡
- 检查本回合未普攻
- 检查攻击距离
- 结算固定公式伤害
- 血量归零时死亡

回合：
- StartTurn
- EndTurn
- 重置本回合行动资源
- 结束后行动条归零

战斗结束：
- 玩家队伍全灭，失败
- 敌方队伍全灭，胜利
```

### 1.2 暂不实现

```text
技能系统
Buff 系统
复杂 AI
路径寻路
命中、闪避、暴击
护甲、抗性
动画播放
MVVM UI 绑定
ScriptableObject 配置
存档
撤销、回放
```

暂不实现不代表架构不考虑，只是本阶段不写代码。

---

## 2. 推荐目录结构

基于当前项目已有 `Core/Character`、`Core/Battle`、`MVVM`，本阶段建议逐步整理成：

```text
Core
├── Battle
│   ├── BattleState.cs
│   ├── BattlePhase.cs
│   ├── BattleResult.cs
│   ├── BattleLoopSystem.cs
│   ├── ActionGaugeSystem.cs
│   ├── BattleCommandService.cs
│   ├── Commands
│   │   ├── IBattleCommand.cs
│   │   ├── MoveCommand.cs
│   │   ├── BasicAttackCommand.cs
│   │   └── EndTurnCommand.cs
│   └── Events
│       ├── IBattleEvent.cs
│       ├── CharacterMovedEvent.cs
│       ├── CharacterDamagedEvent.cs
│       ├── CharacterDiedEvent.cs
│       ├── TurnStartedEvent.cs
│       ├── TurnEndedEvent.cs
│       └── BattleEndedEvent.cs
│
├── Character
│   ├── Model
│   │   ├── CharacterModel.cs
│   │   ├── TeamType.cs
│   │   └── TurnActionState.cs
│   └── Stat
│       ├── CharacterStats.cs
│       ├── BaseStat.cs
│       └── DerivedStat.cs
│
└── Grid
    ├── GridMap.cs
    ├── GridCell.cs
    ├── GridPosition.cs
    └── GridDistance.cs
```

如果想降低改动量，也可以先不移动现有文件，只新增缺失模块。

---

## 3. 核心数据模型

### 3.1 TeamType

```csharp
public enum TeamType
{
    Player,
    Enemy,
    Ally
}
```

`Ally` 先保留，当前切片可以只使用 `Player` 和 `Enemy`。

---

### 3.2 GridPosition

```csharp
public readonly struct GridPosition : IEquatable<GridPosition>
{
    public int X { get; }
    public int Y { get; }

    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

需要支持：

```text
Equals
GetHashCode
==
!=
ToString
```

原因：后续要用它做 Dictionary key、占用查询、日志输出。

---

### 3.3 GridDistance

本阶段统一使用曼哈顿距离：

```csharp
public static class GridDistance
{
    public static int Manhattan(GridPosition a, GridPosition b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
```

移动距离和近战攻击距离都先使用曼哈顿距离。

---

### 3.4 GridCell

```csharp
public class GridCell
{
    public GridPosition Position { get; }
    public bool IsWalkable { get; set; } = true;
}
```

本阶段格子只判断可走性。障碍、地形消耗、高低差后续再加。

---

### 3.5 GridMap

```csharp
public class GridMap
{
    public int Width { get; }
    public int Height { get; }

    public bool IsInside(GridPosition position);
    public GridCell GetCell(GridPosition position);
    public bool IsWalkable(GridPosition position);
}
```

职责：

```text
1. 判断坐标是否在地图内。
2. 查询格子数据。
3. 判断格子是否可走。
```

不建议让 `GridMap` 保存角色占用，因为角色列表属于 `BattleState`。占用判断由 `BattleState` 结合 `GridMap` 完成。

---

### 3.6 CharacterModel

当前已有 `CharacterModel`，本阶段建议扩展为：

```csharp
public class CharacterModel
{
    public string InstanceId { get; }
    public string Name { get; private set; }
    public TeamType Team { get; private set; }
    public GridPosition Position { get; private set; }
    public CharacterStats Stats { get; }

    public int AttackRange { get; private set; }
    public int ActionGauge { get; private set; }
    public bool IsDead { get; private set; }

    public TurnActionState TurnState { get; }

    public int MaxHealth => Stats.BaseStats[BaseStatType.MaxHealth].Value;
    public int CurrentHealth => Stats.DerivedStats[DerivedStatType.CurrentHealth].Value;
    public int Strength => Stats.BaseStats[BaseStatType.Strength].Value;
    public int Speed => Stats.BaseStats[BaseStatType.Speed].Value;
    public int MovementRange => Stats.BaseStats[BaseStatType.MovementRange].Value;
}
```

推荐提供少量安全方法：

```csharp
public void SetPosition(GridPosition position);
public void AddActionGauge(int amount);
public void ResetActionGauge();
public void TakeDamage(int amount);
public void MarkDead();
```

注意：

```text
1. CharacterModel 可以修改自己的字段，但不要判断战斗规则。
2. 例如“目标是否在攻击范围内”不应该写在 CharacterModel。
3. “受到伤害后是否死亡”的最终调用可以由 CombatSystem / BattleCommandService 触发。
```

---

### 3.7 TurnActionState

```csharp
public class TurnActionState
{
    public int RemainingMove { get; private set; }
    public bool HasUsedBasicAttack { get; private set; }

    public void Reset(CharacterModel character)
    {
        RemainingMove = character.MovementRange;
        HasUsedBasicAttack = false;
    }

    public void ConsumeMove(int amount)
    {
        RemainingMove -= amount;
    }

    public void MarkBasicAttackUsed()
    {
        HasUsedBasicAttack = true;
    }
}
```

本阶段移动力简单扣除曼哈顿距离。之后接入寻路时，可以改为路径消耗。

---

## 4. BattleState

`BattleState` 是战斗运行时状态容器。

```csharp
public class BattleState
{
    public GridMap Grid { get; }
    public IReadOnlyList<CharacterModel> Characters => characters;

    public string CurrentActorId { get; private set; }
    public int TurnIndex { get; private set; }
    public int ActionGaugeThreshold { get; } = 1000;
    public BattlePhase Phase { get; private set; }

    public BattleResult Result { get; private set; }

    public CharacterModel GetCharacter(string instanceId);
    public CharacterModel GetCharacterAt(GridPosition position);
    public bool IsCellOccupied(GridPosition position);
    public bool IsCellAvailable(GridPosition position);
    public IEnumerable<CharacterModel> GetAliveCharacters();
    public IEnumerable<CharacterModel> GetAliveCharacters(TeamType team);
}
```

### 4.1 BattleState 不做什么

`BattleState` 不负责：

```text
1. 推进行动条。
2. 验证移动是否合法。
3. 计算伤害。
4. 执行 Command。
5. 发布复杂业务逻辑。
```

这些属于 System 和 Service。

### 4.2 BattlePhase

```csharp
public enum BattlePhase
{
    Init,
    WaitingForGauge,
    WaitingForCommand,
    ResolvingCommand,
    BattleEnd
}
```

本阶段不拆 `TurnStart`、`TurnEnd` 等更细阶段，先保持简单。

### 4.3 BattleResult

```csharp
public enum BattleResult
{
    None,
    PlayerWin,
    EnemyWin
}
```

---

## 5. 行动条系统

### 5.1 规则

```text
1. 每个未死亡角色根据 Speed 积累行动条。
2. 行动条达到 threshold 的角色可以行动。
3. 如果当前已有角色满条，不再继续推进。
4. 如果多个角色满条，使用稳定排序选一个。
5. 角色回合结束后，行动条归零。
```

### 5.2 ActionGaugeSystem

```csharp
public class ActionGaugeSystem
{
    public void AdvanceUntilReady(BattleState state);
    public CharacterModel PickNextActor(BattleState state);
}
```

### 5.3 推进算法

推荐逻辑：

```text
1. 先检查是否已有 ready 角色。
2. 如果有，直接返回。
3. 找出所有 alive 且 Speed > 0 的角色。
4. 如果没有可推进角色，战斗进入异常或结束处理。
5. 计算每个角色距离满条所需时间。
6. 使用最小时间一次性推进所有角色。
```

伪代码：

```csharp
public void AdvanceUntilReady(BattleState state)
{
    if (HasReadyActor(state))
        return;

    var candidates = state.GetAliveCharacters()
        .Where(c => c.Speed > 0)
        .ToList();

    if (candidates.Count == 0)
        throw new InvalidOperationException("No living character can gain action gauge.");

    float minTime = float.MaxValue;

    foreach (var character in candidates)
    {
        int remaining = state.ActionGaugeThreshold - character.ActionGauge;
        float time = remaining / (float)character.Speed;
        minTime = Math.Min(minTime, time);
    }

    foreach (var character in candidates)
    {
        int gain = Math.Max(1, (int)Math.Ceiling(character.Speed * minTime));
        character.AddActionGauge(gain);
    }
}
```

### 5.4 多人满条排序

稳定规则：

```text
1. ActionGauge 高者优先。
2. Speed 高者优先。
3. TeamType 优先级：Player > Assist > Enemy。
4. InstanceId 字典序作为最终稳定排序。
```

这个规则会让测试结果可预测。

---

## 6. Command 系统

### 6.1 IBattleCommand

```csharp
public interface IBattleCommand
{
    CommandResult Validate(BattleState state);
    CommandResult Execute(BattleState state);
}
```

本阶段 `Execute` 可以直接修改 `BattleState` 和 `CharacterModel`。后续接技能、Buff 时再引入 `BattleContext`。

### 6.2 CommandResult

```csharp
public class CommandResult
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    public IReadOnlyList<IBattleEvent> Events { get; }
}
```

可以提供工厂方法：

```csharp
public static CommandResult Success(params IBattleEvent[] events);
public static CommandResult Failure(string errorMessage);
```

### 6.3 BattleCommandService

```csharp
public class BattleCommandService
{
    public CommandResult TryExecute(BattleState state, IBattleCommand command)
    {
        var validateResult = command.Validate(state);
        if (!validateResult.IsSuccess)
            return validateResult;

        state.SetPhase(BattlePhase.ResolvingCommand);
        var executeResult = command.Execute(state);
        state.SetPhase(BattlePhase.WaitingForCommand);
        return executeResult;
    }
}
```

职责：

```text
1. 统一验证。
2. 统一执行。
3. 统一管理 BattlePhase。
4. 统一返回事件和错误。
```

---

## 7. MoveCommand

### 7.1 数据

```csharp
public class MoveCommand : IBattleCommand
{
    public string ActorId { get; }
    public GridPosition TargetPosition { get; }
}
```

### 7.2 验证规则

```text
1. BattlePhase 必须是 WaitingForCommand。
2. ActorId 必须等于 BattleState.CurrentActorId。
3. 行动者存在。
4. 行动者未死亡。
5. 目标格子在地图内。
6. 目标格子可走。
7. 目标格子未被其他角色占用。
8. 曼哈顿距离 <= RemainingMove。
```

### 7.3 执行规则

```text
1. 记录 oldPosition。
2. 计算移动消耗。
3. 设置角色 Position。
4. 扣除 RemainingMove。
5. 返回 CharacterMovedEvent。
```

### 7.4 事件

```csharp
public record CharacterMovedEvent(
    string CharacterId,
    GridPosition From,
    GridPosition To
) : IBattleEvent;
```

---

## 8. BasicAttackCommand

### 8.1 数据

```csharp
public class BasicAttackCommand : IBattleCommand
{
    public string AttackerId { get; }
    public string TargetId { get; }
}
```

### 8.2 验证规则

```text
1. BattlePhase 必须是 WaitingForCommand。
2. AttackerId 必须等于 BattleState.CurrentActorId。
3. 攻击者存在。
4. 目标存在。
5. 攻击者未死亡。
6. 目标未死亡。
7. 攻击者和目标不是同队。
8. 本回合还没有使用普攻。
9. 曼哈顿距离 <= AttackRange。
```

### 8.3 伤害公式

本阶段使用最简单公式：

```text
Damage = max(1, Attacker.Strength)
```

暂不考虑防御、暴击、命中、元素、Buff。

### 8.4 执行规则

```text
1. 计算 damage。
2. 目标扣血。
3. 攻击者标记 HasUsedBasicAttack。
4. 返回 CharacterDamagedEvent。
5. 如果目标 CurrentHealth <= 0：
   - 标记 IsDead。
   - 目标行动条归零。
   - 追加 CharacterDiedEvent。
```

### 8.5 事件

```csharp
public record CharacterDamagedEvent(
    string SourceId,
    string TargetId,
    int Amount
) : IBattleEvent;

public record CharacterDiedEvent(
    string CharacterId
) : IBattleEvent;
```

---

## 9. EndTurnCommand

### 9.1 数据

```csharp
public class EndTurnCommand : IBattleCommand
{
    public string ActorId { get; }
}
```

### 9.2 验证规则

```text
1. ActorId 必须等于 BattleState.CurrentActorId。
2. 行动者存在。
3. 行动者未死亡。
4. BattlePhase 必须是 WaitingForCommand。
```

### 9.3 执行规则

```text
1. 发布 TurnEndedEvent。
2. 当前行动者行动条归零。
3. 清空 CurrentActorId。
4. TurnIndex + 1。
5. 检查战斗是否结束。
6. 如果未结束，Phase = WaitingForGauge。
```

---

## 10. BattleLoopSystem

### 10.1 职责

`BattleLoopSystem` 负责推进战斗状态机。

```csharp
public class BattleLoopSystem
{
    public IReadOnlyList<IBattleEvent> StartBattle(BattleState state);
    public IReadOnlyList<IBattleEvent> AdvanceToNextTurn(BattleState state);
    public BattleResult CheckBattleResult(BattleState state);
}
```

### 10.2 StartBattle

```text
1. 设置 Phase = WaitingForGauge。
2. 检查战斗双方是否都有存活角色。
3. 调用 AdvanceToNextTurn。
```

### 10.3 AdvanceToNextTurn

```text
1. 如果战斗已结束，直接返回。
2. Phase = WaitingForGauge。
3. ActionGaugeSystem.AdvanceUntilReady。
4. ActionGaugeSystem.PickNextActor。
5. 设置 CurrentActorId。
6. 重置该角色 TurnActionState。
7. Phase = WaitingForCommand。
8. 发布 TurnStartedEvent。
```

### 10.4 回合结束后的衔接

本阶段推荐由外层调用：

```text
EndTurnCommand 执行成功
-> BattleLoopSystem.CheckBattleResult
-> 如果未结束，BattleLoopSystem.AdvanceToNextTurn
```

也可以让 `BattleCommandService` 在执行 `EndTurnCommand` 后自动推进下一回合。早期为了清晰，建议外层显式调用。

---

## 11. 死亡与战斗结束

### 11.1 死亡规则

```text
1. CurrentHealth 降到 0。
2. IsDead = true。
3. ActionGauge = 0。
4. 死亡角色不能移动、不能攻击、不能被选为当前行动角色。
5. 死亡角色仍保留在 Characters 列表中。
```

不要从列表删除死亡角色。这样 UI、日志、复活、回放都更容易处理。

### 11.2 战斗结束规则

```text
PlayerWin:
- Enemy 队伍没有任何存活角色。

EnemyWin:
- Player 队伍没有任何存活角色。
```

如果双方同时全灭，本阶段可以先判定 `EnemyWin` 或额外加 `Draw`。建议暂时避免设计同时全灭，等技能/Buff 阶段再补。

---

## 12. 事件系统

本阶段不需要完整 EventBus，只需要 `CommandResult.Events` 和 `BattleLoopSystem` 返回事件。

### 12.1 IBattleEvent

```csharp
public interface IBattleEvent
{
}
```

### 12.2 事件列表

```text
TurnStartedEvent
TurnEndedEvent
CharacterMovedEvent
CharacterDamagedEvent
CharacterDiedEvent
BattleEndedEvent
```

### 12.3 用途

```text
1. 控制台日志。
2. 单元测试断言。
3. 后续 ViewModel 刷新 UI。
4. 后续动画系统播放表现。
```

---

## 13. 最小运行示例

目标测试场景：

```text
地图：5 x 5

Player:
- hero
- Position = (0, 0)
- MaxHealth = 30
- Strength = 10
- Speed = 10
- MovementRange = 3
- AttackRange = 1

Enemy:
- slime
- Position = (2, 0)
- MaxHealth = 20
- Strength = 5
- Speed = 5
- MovementRange = 2
- AttackRange = 1
```

预期流程：

```text
1. StartBattle。
2. 行动条推进，hero 先行动。
3. hero Move 到 (1, 0)。
4. hero BasicAttack slime，slime HP 20 -> 10。
5. hero EndTurn。
6. slime 行动。
7. slime BasicAttack hero，hero HP 30 -> 25。
8. slime EndTurn。
9. hero 再次行动。
10. hero BasicAttack slime，slime HP 10 -> 0，slime 死亡。
11. 检查 Enemy 全灭，BattleResult = PlayerWin。
```

这个场景跑通，就说明垂直切片成立。

---

## 14. 测试建议

如果项目暂时没有测试框架，可以先写普通 C# 测试入口或 Unity EditMode Tests。建议至少覆盖：

```text
ActionGaugeSystem:
- Speed 高的角色先行动。
- 多个角色同时满条时排序稳定。
- 死亡角色不会获得行动权。

MoveCommand:
- 可以移动到合法格子。
- 不能移动到地图外。
- 不能移动到不可走格。
- 不能移动到被占用格。
- 不能超过 RemainingMove。

BasicAttackCommand:
- 可以攻击范围内敌人。
- 不能攻击友军。
- 不能攻击范围外目标。
- 每回合只能普攻一次。
- 伤害能杀死目标。

BattleLoopSystem:
- StartBattle 后进入 WaitingForCommand。
- EndTurn 后能推进到下一个角色。
- 敌人全灭后 PlayerWin。
- 玩家全灭后 EnemyWin。
```

---

## 15. 实现顺序

推荐按下面顺序写代码：

```text
1. GridPosition / GridDistance / GridCell / GridMap
2. TeamType / TurnActionState
3. 扩展 CharacterModel
4. BattlePhase / BattleResult / BattleState
5. IBattleEvent 和基础事件
6. CommandResult / IBattleCommand
7. MoveCommand
8. BasicAttackCommand
9. EndTurnCommand
10. ActionGaugeSystem
11. BattleLoopSystem
12. BattleCommandService
13. 写一个最小运行场景验证完整流程
```

原因：

```text
先有地图和角色状态，Command 才能验证规则。
先有 Command，BattleLoop 才能形成回合闭环。
最后写运行场景，可以避免过早被 Unity 表现层影响。
```

---

## 16. 后续扩展点

这个切片完成后，下一阶段可以自然扩展：

```text
技能：
- CastSkillCommand
- SkillInstance
- SkillSystem
- RangePattern
- AreaPattern
- DamageEffect

Buff：
- BuffInstance
- BuffSystem
- IOnTurnStart
- IBeforeDamageTaken
- IStatModifierProvider

AI：
- IActorController
- AutoAIController
- BattleActionPlan

UI：
- BattleViewModel
- CharacterPanelViewModel
- GridCellViewModel
- BattleLogViewModel
```

本阶段的关键是保证这些扩展不会推翻已经完成的核心闭环。

---

## 17. 验收标准

代码完成后，至少满足：

```text
1. 不依赖 Unity 场景，也能用纯 C# 创建一场战斗。
2. 战斗可以自动推进到第一个行动角色。
3. 当前行动角色可以移动。
4. 当前行动角色可以普攻。
5. 普攻能造成伤害并杀死目标。
6. 死亡角色不会继续行动。
7. 一方全灭后 BattleResult 正确。
8. 每个关键行为都会产生事件。
9. ViewModel 和 MonoBehaviour 不参与任何战斗规则。
```

达到这些标准后，再开始做技能和 Buff 会更稳。
