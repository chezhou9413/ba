# 生命值治愈力 XML 快速配置

本文档只讲 3 类东西：

1. 生命值
2. 治愈力
3. 受回复率

适合直接查字段名，或者复制 XML 模板快速配置。

## 一眼看懂

### Pawn 固有基础值

这类值写在 `PawnKindDef.modExtensions` 的 `BattleBaseStatExtension` 里。

```xml
<initialHealth>10</initialHealth>
<initialHeal>100</initialHeal>
<healReceivedMultiplierOffset>0.2</healReceivedMultiplierOffset>
```

含义：

- `initialHealth`：初始生命值，`1` 表示 `100` 点生命值
- `initialHeal`：初始治愈力，`100` 表示 `100` 点治愈力
- `healReceivedMultiplierOffset`：基础受回复率加成，`0.2` 表示额外 `+20%`

### 可叠加 StatDef

这类值写在装备、Trait、Hediff、状态等支持 `StatDef` 的地方。

生命值相关：

- `BANW_InitialHealth`
- `BANW_HealthLevelMultiplier`
- `BANW_HealthFlatBonus`
- `BANW_HealthBonusMultiplier`

治愈力相关：

- `BANW_InitialHeal`
- `BANW_HealLevelMultiplier`
- `BANW_HealFlatBonus`
- `BANW_HealBonusMultiplier`

受回复率相关：

- `BANW_HealReceivedMultiplier`

## 公式

### 生命值公式

```text
生命值 =
((初始生命值 x 升级生命值倍率 x 升星生命值倍率) + 固定生命值) x 生命值加成
```

代码口径：

```text
初始生命值 = PawnKind.initialHealth + BANW_InitialHealth
升级生命值倍率 = 1 + BANW_HealthLevelMultiplier
升星生命值倍率 = 1 + BattleStarGrowthExtension.healthPercent
固定生命值 = BANW_HealthFlatBonus
生命值加成 = 1 + BANW_HealthBonusMultiplier
```

### 治愈力公式

```text
治愈力 =
((初始治愈力 x 升级治愈力倍率 x 升星治愈力倍率) + 固定治愈力) x 治愈力加成
```

代码口径：

```text
初始治愈力 = PawnKind.initialHeal + BANW_InitialHeal
升级治愈力倍率 = 1 + BANW_HealLevelMultiplier
升星治愈力倍率 = 1 + BattleStarGrowthExtension.healPercent
固定治愈力 = BANW_HealFlatBonus
治愈力加成 = 1 + BANW_HealBonusMultiplier
```

### 受回复率公式

```text
最终受回复率 = BANW_HealReceivedMultiplier + healReceivedMultiplierOffset + 其他状态加值
```

注意：

- `BANW_HealReceivedMultiplier` 的默认值是 `1`
- 所以写 `1.2` 就是 `120%` 受疗
- `healReceivedMultiplierOffset` 是在默认 `100%` 基础上额外加的偏移值

## 这些字段写在哪里

### 1. Pawn 固有基础值

写这里：

```text
PawnKindDef.modExtensions
BANWlLib.BattleSystem.BattleBaseStatExtension
```

可写字段：

- `initialHealth`
- `initialHeal`
- `healReceivedMultiplierOffset`

不要写：

- `healthPercent`
- `healPercent`

这两个旧基础字段已经不用了。

### 2. 升级倍率、固定值、最终加成

写这里：

- 装备：`equippedStatOffsets`
- Trait：`statOffsets`
- Hediff Stage：`statOffsets`
- 原生支持 StatDef 的 Def：`statBases` 或 `statOffsets`

可写字段：

- `BANW_InitialHealth`
- `BANW_HealthLevelMultiplier`
- `BANW_HealthFlatBonus`
- `BANW_HealthBonusMultiplier`
- `BANW_InitialHeal`
- `BANW_HealLevelMultiplier`
- `BANW_HealFlatBonus`
- `BANW_HealBonusMultiplier`
- `BANW_HealReceivedMultiplier`

### 3. 升星倍率

写这里：

```text
PawnKindDef.modExtensions
BANWlLib.BattleSystem.BattleStarGrowthExtension
```

可写字段：

- `healthPercent`
- `healPercent`

## 字段速查表

| 作用 | 字段名 | 写入位置 |
| --- | --- | --- |
| Pawn 固有初始生命值 | `initialHealth` | `BattleBaseStatExtension` |
| Pawn 固有初始治愈力 | `initialHeal` | `BattleBaseStatExtension` |
| Pawn 固有受回复率偏移 | `healReceivedMultiplierOffset` | `BattleBaseStatExtension` |
| 额外初始生命值 | `BANW_InitialHealth` | `statBases/statOffsets/equippedStatOffsets` |
| 升级生命值倍率 | `BANW_HealthLevelMultiplier` | `statBases/statOffsets/equippedStatOffsets` |
| 固定生命值 | `BANW_HealthFlatBonus` | `statBases/statOffsets/equippedStatOffsets` |
| 生命值加成 | `BANW_HealthBonusMultiplier` | `statBases/statOffsets/equippedStatOffsets` |
| 额外初始治愈力 | `BANW_InitialHeal` | `statBases/statOffsets/equippedStatOffsets` |
| 升级治愈力倍率 | `BANW_HealLevelMultiplier` | `statBases/statOffsets/equippedStatOffsets` |
| 固定治愈力 | `BANW_HealFlatBonus` | `statBases/statOffsets/equippedStatOffsets` |
| 治愈力加成 | `BANW_HealBonusMultiplier` | `statBases/statOffsets/equippedStatOffsets` |
| 受回复率 | `BANW_HealReceivedMultiplier` | `statBases/statOffsets/equippedStatOffsets` |
| 升星生命值倍率 | `healthPercent` | `BattleStarGrowthExtension` |
| 升星治愈力倍率 | `healPercent` | `BattleStarGrowthExtension` |

## 直接复制的 XML 模板

### 1. 给角色写固有生命值、治愈力、受回复率

```xml
<modExtensions>
  <li Class="BANWlLib.BattleSystem.BattleBaseStatExtension">
    <!-- 10 表示 1000 点生命值。 -->
    <initialHealth>10</initialHealth>

    <!-- 100 表示 100 点治愈力。 -->
    <initialHeal>100</initialHeal>

    <!-- 额外 +20% 受疗。 -->
    <healReceivedMultiplierOffset>0.2</healReceivedMultiplierOffset>
  </li>
</modExtensions>
```

### 2. 给等级 Hediff 写升级倍率

```xml
<stages>
  <li>
    <label>等级 30</label>
    <statOffsets>
      <!-- 升级生命值倍率 +80%。 -->
      <BANW_HealthLevelMultiplier>0.8</BANW_HealthLevelMultiplier>

      <!-- 升级治愈力倍率 +35%。 -->
      <BANW_HealLevelMultiplier>0.35</BANW_HealLevelMultiplier>
    </statOffsets>
  </li>
</stages>
```

### 3. 给装备写固定生命值、固定治愈力、最终加成

```xml
<equippedStatOffsets>
  <!-- +500 点生命值。 -->
  <BANW_HealthFlatBonus>5</BANW_HealthFlatBonus>

  <!-- 最终生命值 +20%。 -->
  <BANW_HealthBonusMultiplier>0.20</BANW_HealthBonusMultiplier>

  <!-- 固定治愈力 +60。 -->
  <BANW_HealFlatBonus>60</BANW_HealFlatBonus>

  <!-- 最终治愈力 +18%。 -->
  <BANW_HealBonusMultiplier>0.18</BANW_HealBonusMultiplier>

  <!-- 最终受疗 120%。 -->
  <BANW_HealReceivedMultiplier>1.2</BANW_HealReceivedMultiplier>
</equippedStatOffsets>
```

### 4. 给 Trait 写治疗职业倾向

```xml
<statOffsets>
  <BANW_InitialHeal>50</BANW_InitialHeal>
  <BANW_HealFlatBonus>200</BANW_HealFlatBonus>
  <BANW_HealBonusMultiplier>0.26</BANW_HealBonusMultiplier>
</statOffsets>
```

### 5. 给角色写升星生命值倍率和升星治愈力倍率

```xml
<modExtensions>
  <li Class="BANWlLib.BattleSystem.BattleStarGrowthExtension">
    <healthPercent>
      <starValues>
        <li>0</li>
        <li>0.08</li>
        <li>0.16</li>
        <li>0.25</li>
        <li>0.35</li>
      </starValues>
    </healthPercent>
    <healPercent>
      <starValues>
        <li>0</li>
        <li>0.05</li>
        <li>0.10</li>
        <li>0.18</li>
        <li>0.25</li>
      </starValues>
    </healPercent>
  </li>
</modExtensions>
```

## 常见需求怎么写

### 只想给角色更厚的血量

如果这是角色固有面板值，写：

```xml
<initialHealth>12</initialHealth>
```

如果这是装备追加值，写：

```xml
<BANW_HealthFlatBonus>3</BANW_HealthFlatBonus>
```

### 只想让高等级角色血量成长更高

写：

```xml
<BANW_HealthLevelMultiplier>1.2</BANW_HealthLevelMultiplier>
```

表示升级生命值倍率额外 `+120%`，最终乘区按 `2.2` 算。

### 只想让奶妈治疗量更高

写任意一种：

```xml
<BANW_InitialHeal>80</BANW_InitialHeal>
```

```xml
<BANW_HealFlatBonus>100</BANW_HealFlatBonus>
```

```xml
<BANW_HealBonusMultiplier>0.30</BANW_HealBonusMultiplier>
```

区别：

- `BANW_InitialHeal`：进前段乘算
- `BANW_HealFlatBonus`：乘算后固定加
- `BANW_HealBonusMultiplier`：最后整体再乘

### 只想让目标更容易吃满治疗

写：

```xml
<BANW_HealReceivedMultiplier>1.3</BANW_HealReceivedMultiplier>
```

表示目标最终受到 `130%` 治疗。

## 常见错误

### 错误 1：把 `initialHealth` 写进 `statOffsets`

错：

```xml
<statOffsets>
  <initialHealth>10</initialHealth>
</statOffsets>
```

对：

```xml
<statOffsets>
  <BANW_InitialHealth>10</BANW_InitialHealth>
</statOffsets>
```

或者：

```xml
<modExtensions>
  <li Class="BANWlLib.BattleSystem.BattleBaseStatExtension">
    <initialHealth>10</initialHealth>
  </li>
</modExtensions>
```

### 错误 2：把 `healthPercent`、`healPercent` 写进 `BattleBaseStatExtension`

这两个不是基础属性字段，只能写在 `BattleStarGrowthExtension` 里。

### 错误 3：把 `BANW_HealReceivedMultiplier` 当成 `+20%`

当前这个 StatDef 默认值是 `1`。

- 写 `1.2` 才是 `120%`
- 写 `0.2` 会变成只有 `20%` 受疗

## 定义文件在哪里

### StatDef 定义

文件：

`1.6/Defs/RangedWeapon/Damage.xml`

里面有：

- `BANW_InitialHealth`
- `BANW_HealthLevelMultiplier`
- `BANW_HealthFlatBonus`
- `BANW_HealthBonusMultiplier`
- `BANW_InitialHeal`
- `BANW_HealLevelMultiplier`
- `BANW_HealFlatBonus`
- `BANW_HealBonusMultiplier`
- `BANW_HealReceivedMultiplier`

### Pawn 固有基础属性类

文件：

`Source/BANWlLib/BattleSystem/BattleBaseStatExtension.cs`

里面有：

- `initialHealth`
- `initialHeal`
- `healReceivedMultiplierOffset`

### 代码读取入口

文件：

`Source/BANWlLib/BattleSystem/BattleStatUtility.cs`

主要函数：

- `GetInitialHealth`
- `GetInitialHeal`
- `GetFinalHealPower`
- `GetHealReceivedMultiplier`

## 最短可用案例

如果你现在只想先让一个角色能跑起来，直接抄这段：

```xml
<modExtensions>
  <li Class="BANWlLib.BattleSystem.BattleBaseStatExtension">
    <initialHealth>10</initialHealth>
    <initialHeal>100</initialHeal>
    <healReceivedMultiplierOffset>0</healReceivedMultiplierOffset>
  </li>
  <li Class="BANWlLib.BattleSystem.BattleStarGrowthExtension">
    <healthPercent>
      <starValues>
        <li>0</li>
        <li>0.08</li>
        <li>0.16</li>
        <li>0.25</li>
        <li>0.35</li>
      </starValues>
    </healthPercent>
    <healPercent>
      <starValues>
        <li>0</li>
        <li>0.05</li>
        <li>0.10</li>
        <li>0.18</li>
        <li>0.25</li>
      </starValues>
    </healPercent>
  </li>
</modExtensions>
```

再给等级或装备补：

```xml
<statOffsets>
  <BANW_HealthLevelMultiplier>0.8</BANW_HealthLevelMultiplier>
  <BANW_HealLevelMultiplier>0.35</BANW_HealLevelMultiplier>
</statOffsets>
```

```xml
<equippedStatOffsets>
  <BANW_HealthFlatBonus>5</BANW_HealthFlatBonus>
  <BANW_HealthBonusMultiplier>0.20</BANW_HealthBonusMultiplier>
  <BANW_HealFlatBonus>60</BANW_HealFlatBonus>
  <BANW_HealBonusMultiplier>0.18</BANW_HealBonusMultiplier>
  <BANW_HealReceivedMultiplier>1.2</BANW_HealReceivedMultiplier>
</equippedStatOffsets>
```
