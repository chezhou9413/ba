# 特殊技能 XML 配置说明

七组特殊技能的按钮均以“测试新技能”开头，可以与角色原有技能同时使用。下文介绍技能操作、XML 参数、特效绑定和调试菜单用法。修改 XML 后需要重新启动游戏，使配置生效。

## 1. 文件与完整示例

| 角色 | 正式 PawnKind | 完整配置与 AbilityDef |
|---|---|---|
| 妮露 | BANW_Nero | [Nero.xml](../1.6/Defs/SpecialSkills/Nero.xml) |
| 临战爱丽丝 | BANW_Arisu_B | [Arisu.xml](../1.6/Defs/SpecialSkills/Arisu.xml) |
| 黑子 | BANW_Shiroko | [Shiroko.xml](../1.6/Defs/SpecialSkills/Shiroko.xml) |
| 若藻 | BANW_Wakamo | [Wakamo.xml](../1.6/Defs/SpecialSkills/Wakamo.xml) |
| 双形态星野 | BANW_Hoshiro | [Hoshino.xml](../1.6/Defs/SpecialSkills/Hoshino.xml) |
| 凯伊 | BANW_Kei | [Kei.xml](../1.6/Defs/SpecialSkills/Kei.xml) |
| 莉音 | BANW_Rio | [Rio.xml](../1.6/Defs/SpecialSkills/Rio.xml) |

以上文件均为完整 XML 示例，包含角色机制配置和按钮定义。复制为其他技能时，要为各个 Def 设置独立的 defName，并同步修改引用，避免重复定义。

公共弹丸、护盾、增益与移动载体见 [Shared.xml](../1.6/Defs/SpecialSkills/Shared.xml)。正式角色追加补丁见 [AppendSpecialSkills.xml](../1.6/Patches/SpecialSkills/AppendSpecialSkills.xml)。

## 2. 每个攻击段是否使用 BA 属性

`exAttack`、`normalAttack`、`droneAttack` 和 `burstAttack` 是互相独立的攻击段。没有对应行为的角色不需要填写全部四段。

```xml
<exAttack>
  <!-- 使用BA攻击力、成长、暴击、克制与EX属性。 -->
  <useBattleStats>true</useBattleStats>
  <basePower>100</basePower>
  <attackPowerRatio>13</attackPowerRatio>
  <damageDef>BANC_Damage_shenmi</damageDef>
  <isExSkill>true</isExSkill>
  <canCrit>true</canCrit>
  <applyAffinity>true</applyAffinity>
  <radius>0</radius>
  <shots>10</shots>
  <shotIntervalTicks>6</shotIntervalTicks>
  <projectileDef>BANW_SpecialSkillBullet</projectileDef>
  <effecterDef>BANW_Effecter_C</effecterDef>
</exAttack>
```

`attackPowerRatio=13` 表示整个攻击段总共 1300%，10 发各承担 130%。`shotIntervalTicks=6` 表示相邻两发间隔 0.1 游戏秒；1 秒为 60 tick。持续与冷却均为游戏时间，不是墙钟时间。

关闭 BA 时改为：

```xml
<useBattleStats>false</useBattleStats>
<basePower>100</basePower>
<attackPowerRatio>13</attackPowerRatio>
```

此时总基础伤害为 `100×13=1300`，10 发各为 130。成长、BA攻击加成、暴击、克制和 EX 属性不参与，但该技能自身的充能、锐气机制倍率仍参与。目标防御、减伤、护盾、闪避仍正常处理，因此实际受伤量可能低于 1300。

`useBattleStats` 缺省为 true，`basePower` 缺省为 100。不填写这两个字段时，技能按 BA 属性结算。通用 BattleActionConfig、BattleProjectileExtension、PiercingProjectileExtension 和 ProjectileExtraDamageConfig 也支持这两个字段；需要分别配置的攻击段应分别填写。通用治疗动作关闭 BA 后以独立基数计算治疗量，目标受疗率仍生效。

### 攻击段字段

| 字段 | 作用 |
|---|---|
| useBattleStats / basePower | BA结算开关／独立攻击基数 |
| attackPowerRatio | 总攻击倍率，连射按 shots 均分 |
| weaponBaseAttack | BA模式指定的基础武器攻击，0读取当前武器 |
| canCrit / alwaysCrit | BA模式的暴击允许／强制暴击 |
| applyAffinity / isExSkill | BA模式的克制／EX属性乘区 |
| penetration | 护甲穿透，独立模式仍有效 |
| radius | 大于0则在命中点对范围内Pawn结算；0为单体 |
| shots / shotIntervalTicks | 发数／发射间隔 |
| projectileDef | 省略则直接伤害；填写时必须使用 Projectile_SpecialSkill 类 |
| effecterDef | 本段命中特效 |
| affectHostile / affectFriendly | 是否影响敌方／非敌方 |
| canHitOwnPawn | 是否允许伤害同阵营Pawn，含自己 |
| canHitBuilding / canHitOwnBuilding | 单体攻击的建筑允许规则；本组按钮默认只选择Pawn或位置 |

若藻和凯伊释放的伤害取自已蓄积金额，不会再次乘输出属性或暴击。其记录上限的基数：若藻看 `exAttack.useBattleStats`，凯伊看 `burstAttack.useBattleStats`；关闭时读取相应 `basePower`。

### 护盾来源

通用 BattleActionConfig 的 `shieldSource` 支持 HealPower、MaxHealth、Independent；`shieldPowerRatio` 为系数。`useBattleStats=false` 或 Independent 模式使用 basePower，MaxHealth 使用施法者最大生命，默认 HealPower 保持原有治疗力护盾行为。

星野使用角色配置：

```xml
<shieldUseBattleStats>true</shieldUseBattleStats>
<shieldBasePower>1000</shieldBasePower>
<shieldRatio>0.691</shieldRatio>
<shieldHediff>BANW_SpecialHoshinoShield</shieldHediff>
```

关闭 shieldUseBattleStats 后，此例护盾为 `1000×0.691=691`。个人护盾沿用项目规则：盾值不足时仍完整挡住最后一次攻击，不向生命溢出。

## 3. 各角色机制与可调字段

### 妮露

一阶段 2 COST：给自己和一名友军大亢奋，durationTicks=4200。自身切到二阶段，4 COST；先加锐气，再快照攻击。maxStacks=5、stackMultiplier=0.35、exMultiplier=1.2。

锐气倍率为 `1＋0.35×层数`；五层与大亢奋合计 `2.75×1.2=3.3`。大亢奋通过独立乘区影响该角色的 BA EX；独立模式的妮露新EX也显式保留该技能机制倍率。自身阶段到期清空锐气并切回一阶段。

### 爱丽丝

点自己把 stage 从0升到1、再升到2；满档后不能继续自充能。点敌人分别以1、2、3倍基础EX攻击，发射快照后消耗充能。requiredHits=3：每三次有效普攻命中自动发动 normalAttack，保留多余次数。

chargeCastEffecters、chargeImpactEffecters、chargeStateEffecters 均依次配置未充能、一档、二档表现。状态特效随充能阶段替换，开炮清除。

### 黑子

durationTicks=2400，无人机持续40秒；lifeCostRatio=0.2，EX安全消耗最大生命20%。生命消耗通过可治疗伤口实现，不经过护盾和攻击事件，不破坏身体部位；接近致死时只消耗安全允许的部分。

droneAttack 默认单发50%，每次宿主有效普攻命中跟射一次；normalAttack 默认400%。无人机存在时，普通技能额外执行 burstAttack，总500%分10发，合计900%。

immortalityCooldownTicks=5400，lethalStackLimit=15。首次致命伤进入一层，此后每次致命伤加一层，同次伤害涉及多个部位也只加一次；满血清层，冷却保留。第15层强制死亡并绕过项目任务免死。保护不自动治疗旧伤、不额外免倒地。

无人机跟随宿主，不会被攻击。droneTexturePath 可填现有纹理路径；不填时显示简易机体。droneDrawSize 与 droneOffset 控制尺寸和宿主偏移。

### 若藻

exAttack 默认100%，首段结算后开始记录，因此首段不计入蓄积。durationTicks=600，recordRatio=1，recordCapRatio=13.22。

只记录同阵营单位对当前目标的实际伤害，包含若藻后续攻击。上限在施法时快照。到期先关闭记录，再通过 burstAttack 释放；同一施法者重放会重新开始记录，不提前引爆。目标死亡或离图取消记录。不同施法者独立记录。

### 星野

免费 SwitchForm 切换，冷却60 tick，初始输出形态。outputBaseAttack/tankBaseAttack 默认30/10，outputBaseHealth/tankBaseHealth 默认1000/3000，再应用项目成长。保留同一Pawn、装备和其他状态，切换时按实际生命倍率缩放伤口以保持生命百分比。

输出 EX：outputCastTicks=60，在结束时结算 exAttack；允许命中自己与队友。期间 damageTakenReduction=0.85 从当前承伤系数中减去，最低0。例如2.0变为1.15，不是2.0乘0.15。

输出普通技能每 normalIntervalTicks=1800 尝试触发，赋予 empoweredHits=6。原始普攻命中被 normalAttack 的150%伤害替换，并向周围目标传播一次。范围次级命中各自消耗额度，不再扩散；完全闪避和盾完全吸收不消耗。

坦克 EX：moveSpeed 控制移动，抵达后持续40秒。buffHediff 默认攻击力＋156%，shieldRatio=0.691。interceptRadius=3，只拦截穿过前方半圆边界的敌方实体弹丸，背面、友军和内部向外射击放行；本项目穿透弹也接入。瞬间伤害没有弹丸，不参与拦截。

坦克普通技能赋予 tankHits=25，并添加 countHediff。该状态的 statOffsets 默认空，可在 Shared.xml 的 BANW_SpecialHoshinoCount 下填写实际加成；最后一次有效命中后移除。状态面板显示剩余次数。切形态清除该形态专属护盾、拦截、次数和增益，不重置普通技能计时。

### 凯伊

durationTicks=1500、radius=4.9、recordRatio=0.1、recordCapRatio=50。fieldThingDef 绑定增益场地，场地 actions 沿用原有 BAWN_Kei_buff。更改范围时，角色 radius 与场地 BattleFieldControllerExtension.radius 必须一致。

场地期间只记录当前处于范围内的其他同阵营单位对敌方造成的实际伤害。离开范围后不再贡献；上限为施法时攻击基数×50。结束后自动普通技能等待合法敌人，释放记录值100%，无额外基础伤害；成功发出后清空。待释放期间不能开下一轮EX。

若藻和凯伊的蓄积爆发均标记为不可再蓄积，防止互相循环记录。

### 莉音

复制目标的显式默认EX，属性来自莉音自身，不继承目标已有阶段、层数、冷却和无人机。目标获得 buffHediff，默认攻击力＋50%、持续30秒；数值在该Hediff的 statOffsets 配置。

复制基础费用先减1且最低0，再走其他常规减费。成功发动后消耗一次机会、移除复制按钮并恢复入口；取消或目标失效不会消耗。已经发出的子弹、场地、增益继续运行。技能自身产生的状态正常产生；复制爱丽丝时初始未充能，妮露为大亢奋入口，星野为输出EX。

不复制莉音复制入口本身。给其他角色配置可复制EX时，在其 PawnKindDef.modExtensions 中填写：

```xml
<li Class="BANWlLib.Skills.SpecialSkillKindExtension">
  <copyableEx>你的默认EX的AbilityDef名称</copyableEx>
</li>
```

若该角色已经有同类型扩展，修改原扩展，不能重复添加。新复制实例使用原技能的组件与费用，因此该 EX 必须能由普通 Pawn 能力系统独立运行；不根据名称猜测或继承目标专属组件运行状态。

## 4. 正式角色追加与特效绑定

普通特殊技能配置引用方式：

```xml
<abilities>
  <!-- 保留原有li，在末尾追加。 -->
  <li>BANW_Special_Nero_Ex</li>
  <li>BANW_Special_Nero_AlternateEx</li>
</abilities>
<modExtensions>
  <li Class="BANWlLib.Skills.SpecialSkillKindExtension">
    <profile>BANW_Special_Nero</profile>
    <copyableEx>BANW_Special_Nero_Ex</copyableEx>
  </li>
</modExtensions>
```

AppendSpecialSkills.xml 配置了七个正式角色的技能追加。修改角色绑定后，请生成对应角色使用技能；仅修改 XML 不会为存档中已经存在的角色补上按钮。可以通过下文的调试菜单生成角色。

| 特效入口 | 配置位置与生命周期 |
|---|---|
| casterEffecter / targetEffecter | 角色配置；每次主动技能发动时触发 |
| stateEffecter | 角色配置；持续状态期间维护一份，结束或离图清理 |
| stageEffecter / endEffecter | 角色配置；阶段变化或持续结束触发 |
| effecterDef | 每个攻击段；实际命中位置触发 |
| chargeCastEffecters / chargeImpactEffecters / chargeStateEffecters | 爱丽丝配置；三个阶段分别绑定 |
| castSound | 角色配置；一次性施法音效 |
| projectileDef | 攻击段；使用特殊技能弹丸类型并配置其 graphicData 与速度 |

完整声音和特效引用片段：

```xml
<casterEffecter>BANW_Effecter_A</casterEffecter>
<targetEffecter>BANW_Effecter_C</targetEffecter>
<stateEffecter>BANW_Effecter_B</stateEffecter>
<stageEffecter>BANW_Effecter_B</stageEffecter>
<endEffecter>BANW_Effecter_C</endEffecter>
<castSound>Nero_Skill_EX</castSound>
```

不需要某个可选表现入口时删除该字段。显式填写的Def或贴图必须存在；配置错误直接报日志。AbilityDef.verbProperties.range 控制玩家选取范围，角色 range 控制自动索敌和角色目标校验，调整射程时两者同时修改。

## 5. 默认配置速查

| 配置项 | 默认值 |
|---|---|
| 独立攻击基数 | 100 |
| EX 费用 | 妮露一阶段2、二阶段4；其他攻击EX为4 COST；形态切换免费 |
| 主动技能冷却 | 1秒 |
| 施法范围／常规范围攻击半径 | 25格／3格 |
| 妮露 | 大亢奋70秒、EX倍率×1.2；锐气每层＋0.35、最多5层；二阶段基础EX为1300% |
| 爱丽丝 | 基础EX为1300%，充能后为2倍／3倍；每3次有效普攻命中触发100%普通技能 |
| 黑子 | 无人机40秒、消耗最大生命20%、跟射50%；普通技能间隔30秒，400%＋无人机10×50%；不死冷却90秒、第15层死亡 |
| 若藻 | 首段100%，记录10秒，上限1322% |
| 星野形态 | 输出／坦克基础攻击30／10、基础生命1000／3000 |
| 星野输出技能 | EX为500%，执行1秒，期间承伤系数减0.85；普通技能间隔30秒，强化6次、倍率150%、范围3格 |
| 星野坦克技能 | EX持续40秒、攻击＋156%、护盾69.1%、前方拦截半径3格；普通技能间隔30秒、状态持续25次命中 |
| 凯伊 | 场地25秒、半径4.9格；按10%蓄积，上限5000% |
| 莉音 | 复制费用减1 COST；目标攻击＋50%，持续30秒 |

星野25次命中状态的属性列表默认留空，只显示计数。需要加成时，在 `BANW_SpecialHoshinoCount` 的 `statOffsets` 中配置。

## 6. 调试菜单使用教程

开发者模式 → 调试操作菜单 → **BA调试 → 特殊技能**：

- **生成全部七名正式角色**：点击地图空地，一次生成七人并补满COST。
- **选择生成一名正式角色**：先选角色，再点击地图。
- **补满共享COST**：补满当前地图共享池。
- **重置选中角色新技能**：取消本模块未完成攻击并重置新技能；保留旧技能。
- **查看选中角色新技能状态**：弹窗并输出阶段、层数、计数、蓄积和结算模式。
- **触发选中角色普通技能**：角色需征召且可行动，附近需有可见敌人；凯伊仍需场地结束获得释放资格。

自动普通技能只在征召、存活、未倒地、未眩晕且能操作时执行。没有合法目标时等待，不消耗机会；普通技能不消耗COST。有效普攻命中按实际非零伤害计数，子弹分别记，多部位伤口不重复记。

先用“生成全部七名正式角色”在空地生成角色，再使用游戏的生成工具准备敌方目标。选中角色后，使用带“测试新技能”的按钮；通过“查看选中角色新技能状态”查看当前阶段、次数与蓄积金额。

### 常用操作示例

1. **妮露**：对另一名友军使用一阶段EX，随后使用二阶段攻击。每次二阶段先增加一层锐气；大亢奋结束后恢复一阶段并清空锐气。
2. **爱丽丝**：连续两次对自己使用EX完成充能，再对敌人释放。观察普通攻击每造成三次有效命中后自动发动普通技能。
3. **黑子**：使用EX召唤无人机，再让宿主攻击敌人。无人机随有效普攻命中跟射，40秒后结束；状态窗口可以查看不死层数。
4. **若藻**：对敌人使用EX，再由友军持续攻击该目标。记录满10秒后释放蓄积伤害；重新对同一目标施放会重新开始记录。
5. **星野**：使用形态切换按钮，分别操作输出EX和坦克EX。坦克形态选择移动落点，抵达后获得护盾与前方拦截；普通技能次数可在状态窗口查看。
6. **凯伊**：选择场地位置，让其他友军站在范围内攻击敌人。场地结束后，凯伊会寻找合法目标释放蓄积；没有目标时保留金额。
7. **莉音**：对配置了默认EX的友军使用复制技能，再使用出现的临时技能按钮。成功发动后临时按钮消失，复制入口恢复；取消瞄准不会消耗机会。

需要重新开始时，使用“重置选中角色新技能”，再补满共享COST。保存游戏会保留阶段、次数、蓄积、飞行弹丸、待发连射与复制机会，读取后可继续操作。
