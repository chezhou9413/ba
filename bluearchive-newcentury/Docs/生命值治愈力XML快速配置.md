# 生命值与治愈力 XML 配置

生命值字段使用百点单位：`initialHealth=10` 表示 1000 点生命值。治愈力直接使用面板数值。

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

## XML 示例

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

## 配置限制

- `initialHealth`、`initialHeal`、`healReceivedMultiplierOffset` 放在 `BattleBaseStatExtension`，不放入 `statOffsets`。
- `healthPercent`、`healPercent` 放在 `BattleStarGrowthExtension`；等级、装备、Trait 和普通状态使用表中的 StatDef。
- `BANW_HealReceivedMultiplier` 默认值为 1，最终值 1.2 表示 120% 受疗。基础偏移 `healReceivedMultiplierOffset=0.2` 表示额外增加 20%。
- `BANW_InitialHeal` 参与前段乘算，`BANW_HealFlatBonus` 在成长倍率之后相加，`BANW_HealBonusMultiplier` 最后参与整体乘算。

## 定义与计算位置

- StatDef：`1.7/Defs/RangedWeapon/Damage.xml`。
- 角色固有属性：`Source/BANWlLib/BattleSystem/BattleBaseStatExtension.cs`。
- 治疗计算：`BattleStatUtility.GetInitialHeal`、`GetFinalHealPower`、`GetHealReceivedMultiplier`。
- 生命值计算：`Source/BANWlLib/KindStats/BANWKindStatUtility.cs`。
