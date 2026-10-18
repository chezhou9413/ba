# BA 地图共享 COST 系统配置说明

适用项目：`bluearchive-newcentury`
适用版本：RimWorld 1.6
核心命名空间：`BANWlLib.CostSystem`

## 功能

COST 由地图共享，技能和什亭之匣入口 UI 读取同一份余额：

- 每张地图独立的共享 COST 池，普通上限 10 点，特殊任务上限 20 点。
- 已征召学生驱动的自动回复、团队回复率加减算、直接回复和无人征召清零。
- 技能基础费用、固定减费、百分比减费、限次或限时减费，以及最多透支 5 点的过载机制。
- 技能按钮实际费用、禁用原因和 Tooltip 明细。
- 外圈/内圈进度、负数橙色显示、小数显示、满格柔光和数字弹跳。
- COST 轮盘独立拖动、随游戏存档保存位置，以及模组设置页一键复位。
- 69 个学生种类的 112 个实际技能已完成费用绑定。

费用与状态通过 XML 配置；计算规则和界面表现分别由 C# 与 Unity 资源控制。

## 文件位置

| 内容 | 项目内位置 |
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

## 代码结构

| 类型 | 内容 |
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
- 普通地图上限为 10 点，每 216 tick（3.6 游戏秒）回复 1 点。
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

`maxCost` 只允许 10 或 20。不配置时采用 10 点上限和 1 倍回复。

```xml
<costRules>
  <maximumCost>20</maximumCost>
  <recoveryMultiplier>2</recoveryMultiplier>
</costRules>
```

### 技能基础费用

同一个 AbilityDef 只能有一个费用组件。正常施放由 `Ability.Activate` 补丁扣费，不要在技能代码中重复调用 `TrySpend`。

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

构建前应确认 `CostUI.prefab` 的 AssetBundle 标记为 `bamainui.ab` 对应的现有项目配置，构建后检查 Unity Console 没有 Error。不要把 `Assembly-CSharp` 自定义脚本序列化进预制体；运行时代码全部来自 `BANWlLib.dll`。

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
