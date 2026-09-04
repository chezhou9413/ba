# BA 地图共享 COST 系统交付使用说明

适用项目：`bluearchive-newcentury`
适用版本：RimWorld 1.6
核心命名空间：`BANWlLib.CostSystem`

## 交付概览

本系统已经接入学生技能、地图任务和什亭之匣入口 UI。交付版本包含以下能力：

- 每张地图独立的共享 COST 池，普通上限 10 点，特殊任务上限 20 点。
- 已征召学生驱动的自动回复、团队回复率加减算、直接回复和无人征召清零。
- 技能基础费用、固定减费、百分比减费、限次或限时减费，以及最多透支 5 点的过载机制。
- 技能按钮实际费用、禁用原因和 Tooltip 明细。
- 外圈/内圈进度、负数橙色显示、小数显示、满格柔光和数字弹跳。
- COST 轮盘独立拖动、随游戏存档保存位置，以及模组设置页一键复位。
- 69 个学生种类的 112 个实际技能已完成费用绑定。

接手开发者通常只需要修改 XML 配置。只有增加新的计算规则、扣费入口或 UI 表现时才需要修改 C# 或 Unity 资源。

## 交付文件

| 职责 | 项目内位置 |
|---|---|
| COST 核心、技能、状态和 UI 运行时代码 | `Source/BANWlLib/CostSystem/` |
| C# 项目编译入口 | `Source/BANWlLib/BANWlLib.csproj` |
| 已编译模组程序集 | `1.6/Assemblies/BANWlLib.dll` |
| 112 个技能费用补丁 | `1.6/Patches/CostSystem/StudentAbilityCosts.xml` |
| 可直接引用的示例状态 | `1.6/Defs/HediffDefs/CostSystem/CostSystem_Hediffs.xml` |
| 20 点任务规则 | `1.6/Defs/MissionDef/MissionNode/MissionNode.xml` |
| 游戏加载的 UI 资源包 | `1.6/AssetBundles/bamainui.ab` |
| Unity 独立预制体 | `E:/mygame/BAUIMIAN/Assets/Scenes/Resources/UI/CostUI.prefab` |
| Unity 环形进度 Shader | `E:/mygame/BAUIMIAN/Assets/Shaders/UI/Cost/CostRingProgress.shader` |

`mainUI.prefab` 没有被 COST 系统修改。运行时会单独加载 `CostUI.prefab`，再由模组 DLL 挂载 Presenter 和拖动控制器。

## 快速接入

### 给新技能配置 COST

把费用组件加入 `AbilityDef/comps`。同一个技能只能存在一个费用组件：

```xml
<li Class="BANWlLib.CostSystem.CompProperties_AbilityCost">
  <cost>5</cost>
</li>
```

配置完成后不需要在技能代码中手动扣费。`Ability.Activate` 的两个目标重载已经统一执行最终检查和原子扣费。手动再次调用 `BACostPoolService.TrySpend` 会造成重复扣费。

### 给学生施加 COST 状态

状态通过普通 Hediff 接口添加。直接回复状态只会对地图上已征召、存活的玩家学生生效：

```csharp
HediffDef def = DefDatabase<HediffDef>.GetNamed("BANW_CostGrant3_8");
pawn.health.AddHediff(def);
```

回复率、减费和过载状态也使用相同添加方式。技能成功施放后，参与本次计算的所有限次减费状态会各消费一次；费用不足或目标校验阶段未进入实际施放时不会消费次数。

### 配置 20 点任务

在 `BaMissionNode` 中配置：

```xml
<costRules>
  <maximumCost>20</maximumCost>
  <recoveryMultiplier>2</recoveryMultiplier>
</costRules>
```

当前交付版本只允许 `maximumCost` 为 10 或 20。未配置时自动采用 10 点和 1 倍回复。

## 模块职责

| 类型 | 职责 |
|---|---|
| `MapComponent_BACostPool` | 保存单张地图的十分位 COST、回复余数和无人征召计时 |
| `BACostPoolService` | 对外提供查询、直接回复、费用计算、支付检查和原子扣费 |
| `BACostStatusUtility` | 识别有效学生并汇总回复率、减费和过载状态 |
| `CompAbilityCost` | 在技能按钮阶段阻止无法支付的技能 |
| `AbilityCostCalculator` | 按固定减费、百分比乘算、向上取整计算实际费用 |
| `AbilityCostPatches` | 在技能真正发动前再次检查，并确保一次施放只扣费一次 |
| `HediffComp_CostRecoveryRate` | 向共享池提供团队回复率偏移 |
| `HediffComp_CostGrant` | 获得状态时立即回复一次 COST 并移除自身 |
| `HediffComp_CostDiscount` | 筛选目标技能、保存剩余次数并在成功施放后消费 |
| `HediffComp_CostOverdraft` | 提供施法者允许的负 COST 下限 |
| `CostUiPresenter` | 把当前地图共享池同步到环形 Shader 和数字显示 |
| `CostUiDragController` | 拖动轮盘、读写存档坐标并执行复位 |

## 存档数据

| 保存位置 | 字段 | 含义 |
|---|---|---|
| `MapComponent_BACostPool` | `currentCostTenths` | 当前 COST 的整数十分位 |
| `MapComponent_BACostPool` | `recoveryRemainderTenths` | 尚未结算成 0.1 点的回复余数 |
| `MapComponent_BACostPool` | `noDraftedStudentTicks` | 当前地图连续无人征召 tick 数 |
| `DisableCriticalComp` | `costUiPosX`、`costUiPosY` | 轮盘相对什亭之匣入口的位置 |
| `HediffComp_CostDiscount` | `remainingUses` | 限次减费剩余次数 |
| `HediffComp_CostGrant` | `applied` | 防止一次性直接回复在读档后重复结算 |

本系统不提供旧版本字段迁移。新字段缺失时使用声明的默认值，不会改写其他地图或其他存档的 COST。

## 系统规则

- 每张地图各自持有一个 `MapComponent_BACostPool`，不同地图的 COST、回复余数和无人征召计时互不影响。
- COST 以整数十分位保存。界面中的 `3.8` 实际保存为 `38`，不会使用浮点数作为存档主值。
- 普通地图上限为 10 点，每 180 tick 回复 1 点。
- `BaseDefenseNode2` 任务地图上限为 20 点，任务回复倍率为 2。
- 回复倍率为 `任务倍率 × Max(0, 1 + 已征召学生回复率偏移总和)`。
- 只有存活、已生成、玩家阵营、已征召的 `BANW_KivotosStudent` 才参与 COST 系统。
- 地图连续 180 tick 没有符合条件的学生时，当前 COST 和未结算回复进度清零。
- 技能实际费用依次执行固定减费、百分比乘算、向上取整：`Ceil(Max(0, 基础费用 - 固定减费总和) × 各百分比剩余倍率)`。
- 普通学生不能在 COST 为负值时施放 COST 技能。持有过载状态的学生可以把共享池扣到配置的负值下限，默认最低为 -5 点。

## 公开代码接口

命名空间为 `BANWlLib.CostSystem`。

```csharp
MapComponent_BACostPool pool = BACostPoolService.GetPool(map);
float current = BACostPoolService.GetCurrentCost(map);
float granted = BACostPoolService.Grant(map, 3.8f);
BACostRules rules = BACostPoolService.ResolveRules(map);

AbilityCostCalculation calculation = BACostPoolService.GetEffectiveCost(ability);
bool canSpend = BACostPoolService.CanSpend(ability, out string reason);
bool spent = BACostPoolService.TrySpend(ability, out reason);
```

`TrySpend` 会同时扣除共享池并消费所有参与本次计算的限次减费状态。正常技能施放已经由 Harmony 补丁调用该接口，其他代码不要在调用 `Ability.Activate` 前重复扣费。

## XML 配置

### 任务规则

不配置时采用 10 点上限和 1 倍回复。

```xml
<costRules>
  <maximumCost>20</maximumCost>
  <recoveryMultiplier>2</recoveryMultiplier>
</costRules>
```

### 技能基础费用

```xml
<comps>
  <li Class="BANWlLib.CostSystem.CompProperties_AbilityCost">
    <cost>5</cost>
  </li>
</comps>
```

### 回复率状态

`rateOffset` 采用小数：`0.2` 表示增加 20%，`-0.2` 表示降低 20%。状态持有者只有在符合征召条件时才参与地图合计。

```xml
<li Class="BANWlLib.CostSystem.HediffCompProperties_CostRecoveryRate">
  <rateOffset>0.2</rateOffset>
</li>
```

### 直接回复状态

状态获得时结算一次并自动移除，`amount` 最多保留一位小数，结果受当前地图上限约束。

```xml
<li Class="BANWlLib.CostSystem.HediffCompProperties_CostGrant">
  <amount>3.8</amount>
</li>
```

### 减费状态

`affectedAbilities` 留空时作用于持有者全部 COST 技能。`maxUses=-1` 表示不限次数，只等待 Hediff 的其他移除条件；正整数表示成功施放多少次后移除。和 `HediffCompProperties_Disappears` 同时使用时，次数或持续时间任一先到都会移除状态。

```xml
<li Class="BANWlLib.CostSystem.HediffCompProperties_CostDiscount">
  <affectedAbilities>
    <li>BAWN_Hina_EX</li>
    <li>BAWN_Hina_EXX</li>
  </affectedAbilities>
  <flatReduction>1</flatReduction>
  <percentageReduction>0.5</percentageReduction>
  <maxUses>2</maxUses>
</li>
<li Class="HediffCompProperties_Disappears">
  <disappearsAfterTicks>1800</disappearsAfterTicks>
  <showRemainingTime>true</showRemainingTime>
</li>
```

例如基础费用 5、固定减费 1、百分比减费 50% 时，结果为 `(5 - 1) × 50% = 2`；只有 50% 减费时结果为 `Ceil(5 × 50%) = 3`。

### 过载状态

```xml
<li Class="BANWlLib.CostSystem.HediffCompProperties_CostOverdraft">
  <overdraftLimit>5</overdraftLimit>
</li>
```

项目已经提供以下可直接引用的示例 Def：

- `BANW_CostRecoveryRatePlus20`
- `BANW_CostRecoveryRateMinus20`
- `BANW_CostGrant3_8`
- `BANW_CostDiscount50Once`
- `BANW_CostDiscount50Twice`
- `BANW_CostDiscount50Timed`
- `BANW_CostDiscountFlat1Once`
- `BANW_CostOverdraft5`

## UI 行为

- `CostUI.prefab` 是独立纯 UGUI 预制体，运行时由模组 DLL 挂载 `CostUiPresenter`，不依赖 AssetBundle 内的 `Assembly-CSharp` 脚本。
- `CostRoot` 是整体移动和缩放节点。实例挂在 `OpenUi/MainButtom` 上方约 115 像素，缩放为 0.25，并随入口按钮拖动。
- 鼠标左键拖动轮盘可单独调整它相对入口按钮的位置；坐标写入当前游戏存档，读档后自动恢复。
- 模组设置页的“重置COST轮盘位置”会把保存坐标和当前轮盘同时恢复到入口按钮正上方。
- 普通模式仅显示外圈 10 格；20 点模式先填外圈 1—10，再填内圈 11—20。
- 负值时隐藏内圈图片但保留中央数字，外圈反向显示最多 5 格债务，整体切换为橙色。
- 环形进度使用约 0.15 秒平滑过渡。一次回复跨越多个整数格时，数字只弹跳一次，所有新满格同时开始 1 秒白色柔光。
- 数字支持 `5`、`3.8`、`-2`、`-2.4`，整数不显示无意义的小数位。

## 112 个技能费用表

“Ability1 已确认”表示沿用对应学生图鉴第一技能中可确认的 2—7 点费用；“暂定 3 点”表示原文本为占位值或无法确认，当前统一设为 3 点。EX/EXX 变体保持同一学生的相同初始费用。

| 学生种类 | AbilityDef | COST | 来源 |
|---|---|---:|---|
| BANW_Airy | BAWN_Airy_EX | 5 | Ability1 已确认 |
| BANW_Airy | BAWN_Airy_EXX | 5 | Ability1 已确认 |
| BANW_Akane | BAWN_Akane_EX | 2 | Ability1 已确认 |
| BANW_Akane | BAWN_Akane_EXX | 2 | Ability1 已确认 |
| BANW_Akari | BAWN_Akari_EXX | 4 | Ability1 已确认 |
| BANW_Ako | BAWN_Ako_EX | 3 | Ability1 已确认 |
| BANW_Ako | BAWN_Ako_EXX | 3 | Ability1 已确认 |
| BANW_Aoba | BAWN_Aoba_EX | 3 | Ability1 已确认 |
| BANW_Aoba | BAWN_Aoba_EXX | 3 | Ability1 已确认 |
| BANW_Asuna | BAWN_Asuna_EX | 2 | Ability1 已确认 |
| BANW_Asuna | BAWN_Asuna_EXX | 2 | Ability1 已确认 |
| BANW_Atsuko | BAWN_Atsuko_EXX | 3 | 暂定 3 点 |
| BANW_Ayane | BAWN_Ayane_EXX | 4 | Ability1 已确认 |
| BANW_Azusa | BAWN_Azusa_EX | 5 | Ability1 已确认 |
| BANW_Azusa | BAWN_Azusa_EXX | 5 | Ability1 已确认 |
| BANW_Chiaki | BAWN_Chiaki_EX | 6 | Ability1 已确认 |
| BANW_Chiaki | BAWN_Chiaki_EXX | 6 | Ability1 已确认 |
| BANW_Chinatsu | BAWN_Chinatsu_EX | 4 | Ability1 已确认 |
| BANW_Chinatsu | BAWN_Chinatsu_EXX | 4 | Ability1 已确认 |
| BANW_Chise | BAWN_Chise_EX | 3 | 暂定 3 点 |
| BANW_Hanae | BAWN_Hanae_EX | 4 | Ability1 已确认 |
| BANW_Hanae | BAWN_Hanae_EXX | 4 | Ability1 已确认 |
| BANW_Hanako | BAWN_Hanako_EX | 5 | Ability1 已确认 |
| BANW_Hanako | BAWN_Hanako_EXX | 5 | Ability1 已确认 |
| BANW_Hanako_A | BAWN_Hanako_A_EXX | 2 | Ability1 已确认 |
| BANW_Hare | BAWN_Hare_EX | 4 | Ability1 已确认 |
| BANW_Hare | BAWN_Hare_EXX | 4 | Ability1 已确认 |
| BANW_Haruka | BAWN_Haruka_EX | 3 | 暂定 3 点 |
| BANW_Haruna | BAWN_Haruna_EX | 3 | 暂定 3 点 |
| BANW_Hasumi | BAWN_Hasumi_EX | 5 | Ability1 已确认 |
| BANW_Hasumi | BAWN_Hasumi_EXX | 5 | Ability1 已确认 |
| BANW_Hifumi | BAWN_Hifumi_EX | 3 | 暂定 3 点 |
| BANW_Hikali | BAWN_Hikali_EX | 2 | Ability1 已确认 |
| BANW_Hikali | BAWN_Hikali_EXX | 2 | Ability1 已确认 |
| BANW_Himari | BAWN_Himari_EX | 3 | Ability1 已确认 |
| BANW_Himari | BAWN_Himari_EXX | 3 | Ability1 已确认 |
| BANW_Hina | BAWN_Hina_EX | 7 | Ability1 已确认 |
| BANW_Hina | BAWN_Hina_EXX | 7 | Ability1 已确认 |
| BANW_Hina_B | BAWN_Hina_B_EX | 6 | Ability1 已确认 |
| BANW_Hina_B | BAWN_Hina_B_EXX | 6 | Ability1 已确认 |
| BANW_Hiyori | BAWN_Hiyori_EX | 3 | Ability1 已确认 |
| BANW_Hiyori | BAWN_Hiyori_EXX | 3 | Ability1 已确认 |
| BANW_Hoshiro | BAWN_Hoshiro_Treat | 3 | 暂定 3 点 |
| BANW_Hoshiro_A | BAWN_Hoshiro_A_EX | 5 | Ability1 已确认 |
| BANW_Hoshiro_A | BAWN_Hoshiro_A_EXX | 5 | Ability1 已确认 |
| BANW_Ibuki | BAWN_Ibuki_EX | 3 | Ability1 已确认 |
| BANW_Ibuki | BAWN_Ibuki_EXX | 3 | Ability1 已确认 |
| BANW_Ichika | BAWN_Ichika_EX | 3 | 暂定 3 点 |
| BANW_Iori | BAWN_Iori_EX | 3 | 暂定 3 点 |
| BANW_Izumi | BAWN_Izumi_EX | 3 | Ability1 已确认 |
| BANW_Izumi | BAWN_Izumi_EXX | 3 | Ability1 已确认 |
| BANW_Juri | BAWN_Juri_EX | 3 | 暂定 3 点 |
| BANW_Kanoe | BAWN_Kanoe_EXX | 3 | Ability1 已确认 |
| BANW_Karlin | BAWN_Karlin_EX | 4 | Ability1 已确认 |
| BANW_Karlin | BAWN_Karlin_EXX | 4 | Ability1 已确认 |
| BANW_Kasumi | BAWN_Kasumi_EX | 4 | Ability1 已确认 |
| BANW_Kasumi | BAWN_Kasumi_EXX | 4 | Ability1 已确认 |
| BANW_Kazusa | BAWN_Kazusa_EX | 4 | Ability1 已确认 |
| BANW_Kazusa | BAWN_Kazusa_EXX | 4 | Ability1 已确认 |
| BANW_Kei | BAWN_Kei_EXX | 2 | Ability1 已确认 |
| BANW_Kokona | BAWN_Kokona_EXX | 2 | Ability1 已确认 |
| BANW_Kotama | BAWN_Kotama_EX | 3 | Ability1 已确认 |
| BANW_Kotama | BAWN_Kotama_EXX | 3 | Ability1 已确认 |
| BANW_Koyuki | BAWN_Koyuki_EX | 3 | 暂定 3 点 |
| BANW_Maki | BAWN_Maki_EX | 5 | Ability1 已确认 |
| BANW_Maki | BAWN_Maki_EXX | 5 | Ability1 已确认 |
| BANW_Mari | BAWN_Mari_EX | 2 | Ability1 已确认 |
| BANW_Mari | BAWN_Mari_EXX | 2 | Ability1 已确认 |
| BANW_Mashiro | BAWN_Mashiro_EX | 3 | Ability1 已确认 |
| BANW_Mashiro | BAWN_Mashiro_EXX | 3 | Ability1 已确认 |
| BANW_Michiru | BAWN_Michiru_EX | 3 | 暂定 3 点 |
| BANW_Mika | BAWN_Mika_EX | 6 | Ability1 已确认 |
| BANW_Mika | BAWN_Mika_EXX | 6 | Ability1 已确认 |
| BANW_Mika_A | BAWN_Mika_A_EX | 4 | Ability1 已确认 |
| BANW_Mika_A | BAWN_Mika_A_EXX | 4 | Ability1 已确认 |
| BANW_Momiji | BAWN_Momiji_EX | 4 | Ability1 已确认 |
| BANW_Momiji | BAWN_Momiji_EXX | 4 | Ability1 已确认 |
| BANW_Momoi | BAWN_Momoi_EX | 3 | Ability1 已确认 |
| BANW_Momoi | BAWN_Momoi_EXX | 3 | Ability1 已确认 |
| BANW_Nagisa | BAWN_Nagisa_EXX | 3 | 暂定 3 点 |
| BANW_Nagisa_A | BAWN_Nagisa_A_EXX | 3 | Ability1 已确认 |
| BANW_Natsu | BAWN_Natsu_EX | 3 | Ability1 已确认 |
| BANW_Natsu | BAWN_Natsu_EXX | 3 | Ability1 已确认 |
| BANW_Nero | BAWN_Nero_EX | 2 | Ability1 已确认 |
| BANW_Nero | BAWN_Nero_EXX | 2 | Ability1 已确认 |
| BANW_Noa | BAWN_Noa_EX | 3 | Ability1 已确认 |
| BANW_Noa | BAWN_Noa_EXX | 3 | Ability1 已确认 |
| BANW_Nonomi | BAWN_Nonomi_EX | 2 | Ability1 已确认 |
| BANW_Nozomi | BAWN_Nozomi_EX | 3 | 暂定 3 点 |
| BANW_Nozomi | BAWN_Nozomi_EXX | 3 | 暂定 3 点 |
| BANW_Pina | BAWN_Pina_EX | 3 | 暂定 3 点 |
| BANW_Reisa | BAWN_Reisa_EX | 3 | Ability1 已确认 |
| BANW_Reisa | BAWN_Reisa_EXX | 3 | Ability1 已确认 |
| BANW_Reisa_A | BAWN_Reisa_A_EX | 4 | Ability1 已确认 |
| BANW_Reisa_A | BAWN_Reisa_A_EXX | 4 | Ability1 已确认 |
| BANW_Rio | BAWN_Rio_EX | 3 | 暂定 3 点 |
| BANW_Serika | BAWN_Serika_EX | 2 | Ability1 已确认 |
| BANW_Serika | BAWN_Serika_EXX | 2 | Ability1 已确认 |
| BANW_Serina | BAWN_Serina_EX | 2 | Ability1 已确认 |
| BANW_Serina | BAWN_Serina_EXX | 2 | Ability1 已确认 |
| BANW_Shiroko | BAWN_Shiroko_EX | 3 | 暂定 3 点 |
| BANW_Shun | BAWN_Shun_EXX | 3 | 暂定 3 点 |
| BANW_Suzumi | BAWN_Suzumi_EX | 4 | Ability1 已确认 |
| BANW_Suzumi | BAWN_Suzumi_EXX | 4 | Ability1 已确认 |
| BANW_Suzumi_A | BAWN_Suzumi_A_EXX | 5 | Ability1 已确认 |
| BANW_Tsubaki | BAWN_Tsubaki_EX | 3 | 暂定 3 点 |
| BANW_Wakamo_A | BAWN_Wakamo_A_EX | 3 | 暂定 3 点 |
| BANW_Wakamo_A | BAWN_Wakamo_A_EXX | 3 | 暂定 3 点 |
| BANW_Yoshimi | BAWN_Yoshimi_EX | 4 | Ability1 已确认 |
| BANW_Yoshimi | BAWN_Yoshimi_EXX | 4 | Ability1 已确认 |
| BANW_Yuuka | BAWN_Yuuka_EX | 3 | Ability1 已确认 |
| BANW_Yuuka | BAWN_Yuuka_EXX | 3 | Ability1 已确认 |

统计：69 个学生种类、112 个 `AbilityDef`；其中 50 个种类、91 个技能来自可确认的 Ability1 COST，19 个种类、21 个技能暂定为 3 点。

## 人工测试步骤

### 普通地图与共享池

1. 进入普通地图，确认只显示外圈，中央数字为 `0`。
2. 征召一名基沃托斯学生，记录 180 tick 后正好增加 1 点；蓄满后保持 10 点且不预存额外回复。
3. 同时选中两名拥有 COST 技能的学生，用其中一人施放 5 点技能，确认共享数字减少 5，另一人的技能按钮同步按新余额禁用或启用。
4. 切换到另一张地图，确认两张地图的 COST 独立；切回后原地图数值不变。

### 20 点任务

1. 启动 `BaseDefenseNode2` 并进入对应任务地图。
2. 确认内圈出现，基础回复速度为普通地图两倍，即约 90 tick 增加 1 点。
3. 确认 0—10 点填充外圈，10—20 点填充内圈，上限停在 20。

### 征召与清零

1. 有 COST 时解除全场学生征召，等待少于 180 tick 后重新征召，确认原 COST 保留。
2. 再次解除征召并等待满 180 tick，确认 COST 与未结算回复进度都清零。
3. 验证未生成、死亡、非玩家阵营或非 `BANW_KivotosStudent` 的 Pawn 不触发回复。

### 回复率与直接回复

1. 给已征召学生添加 `BANW_CostRecoveryRatePlus20`，确认普通地图每点耗时约 150 tick。
2. 添加 `BANW_CostRecoveryRateMinus20`，确认单独存在时每点耗时约 225 tick；正负 20% 同时存在时回到基础速度。
3. 叠加总偏移不高于 -100%，确认回复停止且不发生反向扣费。
4. 给已征召学生添加 `BANW_CostGrant3_8`，确认立即增加准确的 `3.8`，状态自动移除，超过上限的部分被截断。
5. 从较低值一次跨越多个整数格，确认中央数字只弹跳一次，所有新满格同时播放白色完成光效。

### 减费顺序、次数与持续时间

1. 对基础 5 点技能添加 `BANW_CostDiscount50Once`，确认按钮显示 3，成功施放一次后状态消失；费用不足导致的失败施放不消耗次数。
2. 同时添加固定减 1 与 50% 减费，确认最终费用为 2，而不是先乘算得到 3 后再减 1。
3. 添加 `BANW_CostDiscount50Twice`，确认两次成功施放后才消失。
4. 添加 `BANW_CostDiscount50Timed`，确认持续时间内不限次数，1800 tick 到时移除。
5. 自定义同时配置 `maxUses` 和 `HediffCompProperties_Disappears` 的状态，分别验证次数先到和时间先到都会移除。
6. 用 `affectedAbilities` 只填写一个 AbilityDef，确认同一学生的其他 COST 技能不享受该减费。

### 过载与负值 UI

1. 当前 COST 为 3，给施法者添加 `BANW_CostOverdraft5` 后使用 5 点技能，确认结果为 `-2`。
2. 当前 COST 为 1，尝试使用 7 点技能，确认会达到 -6，因此被拒绝。
3. COST 为负时确认普通学生全部 COST 技能禁用；拥有过载状态的学生只在扣费后不低于 -5 时可用。
4. 确认负值隐藏内圈图片，外圈从零点反向显示最多 5 格，环、光效和数字为橙色，数字格式可显示 `-2` 和 `-2.4`。
5. 让过载状态到期，确认负 COST 不被强制清零，并继续按正常回复规则逐步回到零。

### 存档

1. 在 `3.8`、负数、回复到半途和无人征召计时未满 180 tick 时分别保存并读档。
2. 确认当前十分位 COST、回复余数和无人征召计时连续，不发生额外直接回复，也不会重置限次减费的剩余次数。
3. 拖动COST轮盘到其他位置后保存并读档，确认轮盘恢复到保存位置；再从模组设置复位，确认立即回到入口按钮上方并保存新的默认坐标。

## 构建与部署

### 编译模组程序集

在项目根目录执行：

```powershell
dotnet build Source/BANWlLib/BANWlLib.csproj -c Release --nologo
```

项目会把结果输出到 `1.6/Assemblies/BANWlLib.dll`。修改纯 C#、Defs 或 Patch 时不需要重新构建 AssetBundle。

### 构建 UI 资源包

只有修改 `CostUI.prefab`、Shader、材质、数字图集或其他 Unity 资源时才需要执行：

```text
RimWorldTools/UI/Build and Deploy bamainui.ab
```

该 Unity 菜单会覆盖以下文件：

- `1.6/AssetBundles/bamainui.ab`
- `1.6/AssetBundles/bamainui.ab.manifest`

构建前应确认 `CostUI.prefab` 的 AssetBundle 标记为 `bamainui.ab` 对应的现有项目配置，构建后检查 Unity Console 没有 Error。不要把 `Assembly-CSharp` 自定义脚本序列化进交付预制体；运行时代码全部来自 `BANWlLib.dll`。

## 常见问题

### COST 不回复

确认当前地图至少有一名存活、已生成、玩家阵营、已征召的 `BANW_KivotosStudent`。回复率偏移总和达到 `-100%` 或更低时，最终回复倍率会被限制为 0。

### 技能按钮没有费用

确认目标 `AbilityDef` 的 `comps` 中存在且仅存在一个 `CompProperties_AbilityCost`。如果使用 Patch 添加，还要确认 XPath 命中了实际 Def。

### 减费结果与预期不同

固定减费先相加并从基础费用中扣除，百分比减费随后逐个乘算，最后统一向上取整。多个百分比状态不是简单相加。

### 限次减费没有消耗

只有技能进入实际 `Activate` 并成功通过 COST 支付时才会消耗。按钮点击被禁用、余额不足或过载边界不满足时不会消耗。

### 轮盘无法拖动

确认运行时根节点存在 `CostUiDragController`，`CanvasGroup.blocksRaycasts` 为 true，并且透明的 `CostDragSurface` 是唯一开启射线的轮盘图片。预制体内其他图片仍应关闭射线。

### 轮盘位置丢失

拖动结束后坐标只写入当前游戏的 `DisableCriticalComp`，需要正常保存游戏才能持久化。不同存档分别保存自己的轮盘位置。

## 交付验收清单

- [ ] `BANWlLib.csproj` Release 编译为 0 错误。
- [ ] 新增或修改的 XML 能以 UTF-8 正常解析。
- [ ] 每个 COST 技能仅有一个费用组件。
- [ ] 普通地图、20 点任务和多地图隔离行为通过人工检查。
- [ ] 回复率、3.8 直充、减费顺序、限次/限时和过载边界通过人工检查。
- [ ] 正数、十分位、负数、双环、白光和数字弹跳显示正常。
- [ ] 轮盘拖动、保存/读档恢复和设置页复位正常。
- [ ] 如修改过 Unity 资源，AssetBundle 已覆盖部署且 Unity Console 为 0 Error。
- [ ] 未把 Unity 工程预览脚本写入 `CostUI.prefab`。

## 编译与静态检查结果

- `BANWlLib.csproj` Release 编译：0 错误。
- 技能补丁目标：112 个唯一 AbilityDef，重复 0，目标 Def 缺失 0，`comps` 节点缺失 0。
- 费用分布：2 点 18 个、3 点 49 个、4 点 24 个、5 点 13 个、6 点 6 个、7 点 2 个。
- 按项目约定未启动 RimWorld，也未进入 Unity Play Mode；运行行为由上面的人工步骤在游戏内确认。
