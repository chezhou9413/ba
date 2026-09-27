# 特殊技能 XML 配置说明

七组特殊技能的按钮均以“测试新技能”开头，可以与角色原有技能同时使用。下文介绍技能操作、XML 参数、特效绑定和调试菜单用法。修改 XML 后需要重新启动游戏，使配置生效。

## 1. 文件与完整示例

| 角色 | 正式 PawnKind | 完整配置与 AbilityDef |
|---|---|---|
| 妮露 | BANW_Nero | [Nero.xml](../1.6/Defs/SpecialSkills/Nero.xml) |
| 临战爱丽丝 | BANW_Arisu_B | [Arisu.xml](../1.6/Defs/SpecialSkills/Arisu.xml)、[ArisuNormal.xml](../1.6/Defs/SpecialSkills/ArisuNormal.xml)、[ArisuEffects.xml](../1.6/Defs/SpecialSkills/ArisuEffects.xml) |
| 黑子 | BANW_Shiroko | [Shiroko.xml](../1.6/Defs/SpecialSkills/Shiroko.xml) |
| 若藻 | BANW_Wakamo | [Wakamo.xml](../1.6/Defs/SpecialSkills/Wakamo.xml) |
| 双形态星野 | BANW_Hoshiro | [Hoshino.xml](../1.6/Defs/SpecialSkills/Hoshino.xml)、[HoshinoAbilities.xml](../1.6/Defs/SpecialSkills/HoshinoAbilities.xml) |
| 凯伊 | BANW_Kei | [Kei.xml](../1.6/Defs/SpecialSkills/Kei.xml) |
| 莉音 | BANW_Rio | [Rio.xml](../1.6/Defs/SpecialSkills/Rio.xml) |

以上文件提供角色机制配置和按钮定义；星野的定向特效另见 [HoshinoEffects.xml](../1.6/Defs/SpecialSkills/HoshinoEffects.xml)。复制为其他技能时，要为各个 Def 设置独立的 defName，并同步修改引用，避免重复定义。

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
  <!-- 护甲穿透50%，每发独立使用该值，不随shots均分。 -->
  <penetration>0.5</penetration>
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
| penetration | 本段每次伤害的护甲穿透，0.5为50%，BA与独立模式均有效，不随shots均分 |
| radius | 大于0则在命中点对范围内Pawn结算；0为单体 |
| shots / shotIntervalTicks | 发数／发射间隔 |
| projectileDef | 省略则直接伤害；填写时必须使用 Projectile_SpecialSkill 类 |
| effecterDef | 本段命中特效 |
| affectHostile / affectFriendly | 是否影响敌方／非敌方 |
| canHitOwnPawn | 是否允许伤害同阵营Pawn，含自己 |
| canHitBuilding / canHitOwnBuilding | 单体攻击的建筑允许规则；本组按钮默认只选择Pawn或位置 |

若藻把原始蓄积金额作为基础，再按 `burstAttack` 和若藻BA属性计算敌方状态中显示的待释放值；到期释放该显示值，不重复乘算。凯伊直接释放已蓄积金额，不再乘输出属性或暴击。原始记录上限的基数：若藻看 `exAttack.useBattleStats`，凯伊看 `burstAttack.useBattleStats`；关闭时读取相应 `basePower`。

### 七组测试技能的护甲穿透

所有18个实际伤害段均显式提供 `<penetration>0</penetration>`，可逐段独立修改。默认0保持原伤害配置；0.5表示50%穿透，1表示100%，允许大于1。使用原版护甲判定：护甲值减去穿透值，最低为0，例如80%护甲遇到50%穿透，按30%剩余护甲判定。

| 角色 | 护甲穿透配置位置 |
|---|---|
| 妮露 | `Nero.xml`：`exAttack.penetration` |
| 爱丽丝 | `Arisu.xml`：`exAttack.penetration`；`ArisuNormal.xml`：`attack.penetration`；EX充能倍率不改变穿透 |
| 黑子 | `Shiroko.xml`：`normalAttack.penetration`、`droneAttack.penetration`、`burstAttack.penetration` |
| 若藻 | `Wakamo.xml`：`exAttack.penetration`、`burstAttack.penetration`，首段和蓄积爆发独立配置 |
| 星野 | `Hoshino.xml`：`hoshino.exStages`每段的`attack.penetration`、`normalAttack.penetration`、`hoshino.empoweredAttack.penetration` |
| 凯伊 | `Kei.xml`：`burstAttack.penetration`，控制蓄积释放的穿透 |
| 莉音 | 复制EX沿用被复制技能上述各段的`penetration`，复制入口和支援增益本身无伤害 |

单体弹丸、直接伤害、AOE的每个目标、每发连射、星野强化普攻和蓄积释放都使用对应段的穿透值。技能弹丸的实际伤害读取攻击段的 `penetration`，无需改共享弹丸的 `armorPenetrationBase`。普通武器平A仍使用武器弹丸自身配置；黑子消耗生命属于施法代价，不经过护甲结算。

穿透只参与原版护甲判定，不绕过闪避、护盾或其他减伤。若自定义 `damageDef` 没有 `armorCategory`，原版本身不对该伤害执行护甲判定；当前测试技能使用的神秘伤害配置为锐器护甲。

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

一段独立音效在 `BANW_Special_Nero_Ex/verbProperties/soundCast` 配置。二段每发枪声在 `exAttack.shotSound` 配置；`exAttack.shotTicks` 逐项指定相对技能生效的发射tick，默认 `1,7,13,19,25,31,37,43,49,55`，条目数必须等于 `shots`。删除整个列表后才改用 `shotIntervalTicks` 统一间隔。二段使用 `BANW_Job_NeroExChannel` 保持整轮停步瞄准，不能平A；中断、倒地或目标失效时取消未发射的子弹，已经射出的弹丸继续结算。详见[妮露EX技能配置说明](妮露EX技能配置说明.md)。

### 爱丽丝

点自己把 stage 从0升到1、再升到2；满档后不能继续自充能。`arisu.chargeEffecters`仅配置两项，分别为升至第一档、第二档时的自身特效。点敌人按0、1、2档从`arisu.attackEffecters`选择A、B、C对敌特效，同时应用1、2、3倍基础EX伤害，发动后清空充能。两套特效入口独立，不叠加通用施法特效。

EX为固定直线多段AOE，默认`arisu.lineLength=25`、`lineWidth=3`、`exAttack.shots=5`、`shotIntervalTicks=6`。总倍率1300%按5段均分；充能后总倍率2600%／3900%。预览与伤害共用同一套直线格子，施法时锁定起点和方向，各段重新筛选区域内敌人，原目标死亡不取消剩余伤害。定向特效沿用水花子的`SubEffecter_SprayerTriggeredRotatedOffset`，偏移与贴图一起旋转，延迟节点也固定施法方向。

普通技能独立为`ArisuNormal.xml`中的`ArisuNormalSkillDef`，由`arisu.normalSkill`引用；不注册主动按钮。`requiredHits=3`控制每三次有效普攻命中自动施放，`cooldownTicks=1`为最小触发间隔。普通技能可独立设置`castSound`、`casterEffecter`、`targetEffecter`以及`attack.effecterDef`、伤害与穿透；不消费充能、也不按充能放大伤害。详细字段见[爱丽丝测试技能配置说明](爱丽丝测试技能配置说明.md)。

### 黑子

durationTicks=2400，无人机持续40秒；lifeCostRatio=0.2，EX安全消耗最大生命20%。生命消耗通过可治疗伤口实现，不经过护盾和攻击事件，不破坏身体部位；接近致死时只消耗安全允许的部分。

`droneAttack` 默认单发50%，宿主主武器每成功发射一发就对该发瞄准对象跟射一次；宿主射偏、被闪避、护甲减伤或护盾吸收都不影响触发。无人机仍独立接受目标闪避、护甲与护盾判定。`normalAttack` 保留宿主400%伤害；每30秒自动选敌后，无人机锁定该目标执行 `burstAttack`，总普攻倍率1000%分20发，每发50%，间隔6 tick。

两段无人机攻击均标记 `isNormalAttack=true`、`isExSkill=false`，读取宿主普攻倍率、基础精通、暴击和克制，禁止EX增益。各段的 `shotSound` 在每次实际发射时播放，默认使用 `ShirokoEffects.xml` 中的 `BANW_Shiroko_DroneShot`。发射后的弹丸由统一伤害链结算，各段 `penetration` 仍可独立配置。详见[白子无人机技能配置说明](白子无人机技能配置说明.md)。

immortalityCooldownTicks=5400，lethalStackLimit=15。首次致命伤进入一层，此后每次致命伤加一层，同次伤害涉及多个部位也只加一次；满血清层，冷却保留。第15层强制死亡并绕过项目任务免死。保护不自动治疗旧伤、不额外免倒地。

无人机跟随宿主，不会被攻击。droneTexturePath 可填现有纹理路径；不填时显示简易机体。droneDrawSize 与 droneOffset 控制尺寸和宿主偏移。已经开始的20发连射可在EX到期后完成，目标死亡、消失或离图时取消剩余发射；EX到期后不再响应宿主新的普攻追击。

### 若藻

`exAttack` 默认100%总倍率、6发单体实体弹丸，`shots`、`shotIntervalTicks`、`shotTicks` 可配置发数与时序。首次实际扣血后给敌人附加 `buffHediff=BANW_WakamoAccumulation`，并计入该发伤害；同轮后续子弹与同阵营友军对目标的实际扣血均计入。durationTicks=600，recordRatio=1，recordCapRatio=13.22。

原始蓄积以施法时攻击力的1322%为上限，再按若藻BA输出、暴击、克制和EX倍率换算。敌方状态显示换算后的待释放伤害与上限；标记建立时锁定换算倍率和暴击结果，查看状态不会重新随机计算。到期先关闭记录，再通过 `burstAttack` 按显示值释放，不重复乘算；最终扣血仍受目标闪避、护甲与护盾影响。

同一施法者重放会重新开始记录，不提前引爆。目标死亡或离图取消记录，不同施法者独立记录。连射与爆发可分别配置 `damageDef` 和 `penetration`，详细字段见 [若藻EX技能配置说明](若藻EX技能配置说明.md)。

### 星野

免费 SwitchForm 切换，冷却60 tick，初始攻击形态。outputBaseAttack/tankBaseAttack 默认30/10，outputBaseHealth/tankBaseHealth 默认1000/3000，再应用项目成长。保留同一Pawn、装备和其他状态，切换时按实际生命倍率缩放伤口以保持生命百分比。hoshino.outputBurstShots/tankBurstShots 默认6/1，仅改变该角色的主武器连射数，不修改共享武器定义。

攻击EX：outputCastTicks=60，准备后执行 hoshino.exStages 中的独立伤害段。默认前3发各50%单体伤害，后4发各87.5%的3格AOE，总倍率500%。后4发射向施法时目标所在格，每发落点只结算一次AOE，不附加单体伤害；AOE沿用允许误伤自己的角色与友军配置。准备与连射期间 damageTakenReduction=0.85 从当前承伤系数中减去，最低0。每段参数见 [星野测试技能配置说明](星野测试技能配置说明.md)。

攻击普通技能：每成功射出 hoshino.normalRequiredShots=30 发普攻后触发 normalAttack，默认3发单体技能弹，总倍率100%。技能弹和AOE受击人数不计入普攻发数；没有有效目标时保留触发进度。攻击EX或攻击普通技能发射结束后，下一整轮普攻强化，每发用定向特效替代原武器弹丸，对目标周围造成一次150%普攻AOE。6发连射对应6次AOE，受伤目标多少不影响次数。强化普攻本身每发仍计入30发进度。

坦克 EX：moveSpeed 控制移动，抵达后持续40秒。buffHediff 默认攻击力＋156%，shieldRatio=0.691。interceptRadius=3，只拦截穿过前方半圆边界的敌方实体弹丸，背面、友军和内部向外射击放行；本项目穿透弹也接入。瞬间伤害没有弹丸，不参与拦截。

防御普通技能：独立按钮主动施放，冷却2400 tick（40秒），赋予 tankHits=25 次受伤额度并添加 countHediff。每次实际受到伤害消耗一次，完全闪避或护盾完全吸收不计数；次数耗尽解除。该状态的 statOffsets 默认空，可在 Shared.xml 的 BANW_SpecialHoshinoCount 下填写加成。防御EX的移动和到期不清除此状态；再次施放刷新额度。切形态清除形态专属状态与强化资格、终止旧形态连射，但保留普攻累计和防御普通技能冷却。

### 凯伊

`kei.sourceExAbility=BAWN_Kei_EXX` 绑定现有EX“存在于此的我”。使用现有EX会直接连接它生成的实际场地；测试EX也读取此来源的场地配置和持续时间覆盖值。半径只修改 `AbilityDef/Kei.xml` 中场地的 `BattleFieldControllerExtension.radius`，持续时间优先读取来源EX的 `durationTicksOverride`。默认1500 tick、半径4.9格，无需同步第二份角色范围。详细入口见[凯伊技能配置说明](凯伊技能配置说明.md)。

场地期间只记录已获得 `kei.contributionHediff`、当前仍在实际场地格子范围内的其他同阵营单位对敌方造成的有效伤害。`recordRatio=0.1`、`recordCapRatio=50`，离开范围后不再贡献。结束后自动普通技能等待合法敌人，停步瞄准并经过 `kei.warmupTicks=60` 的前摇，再发射 `burstAttack.projectileDef` 指定的一颗单体弹丸；命中时释放记录值100%，无额外基础伤害。`burstAttack.penetration` 控制穿透，`shots` 必须为1、`radius` 必须为0。前摇中断或无目标时保留资格；发射后清空。测试EX在记录或待释放期间禁用；现有EX的多次施法各自独立记录。

若藻和凯伊的蓄积爆发均标记为不可再蓄积，防止互相循环记录。

### 莉音

复制机会交给目标本体，选中目标即可使用独立的“复制·EX”按钮。技能的施法者、属性、距离、前摇、语音、特效及伤害结算全部使用目标本体。妮露处于二段时复制二段，锐气层数直接使用并推进本体状态；爱丽丝沿用本体充能，星野沿用当前形态。阶段变化时复制按钮同步。目标获得 buffHediff，默认攻击力＋50%、持续30秒；数值在该Hediff的 statOffsets 配置。

复制基础费用先减1且最低0，再走目标本体的其他减费。复制实例从独立冷却开始，成功发动后消耗一次机会、隐藏复制按钮并恢复莉音入口；取消、目标失效或费用不足不会消耗。已经发出的子弹、场地、增益继续运行，原版能力维护的持续特效完成后才回收实例。

所有具有 `CompProperties_AbilityCost` 的技能都视为EX，包括基础费用为0或减费后为0的技能。识别目标实际持有且当前阶段可见的原生技能，排除莉音授予的复制实例，避免重复复制临时机会。默认优先当前角色专属测试EX；右键复制按钮可选择目标其他当前阶段可见的EX，不要求角色白名单。给技能声明COST的配置：

```xml
<li Class="BANWlLib.CostSystem.CompProperties_AbilityCost">
  <cost>4</cost>
</li>
```

该组件放在技能 `AbilityDef.comps` 中，已有费用组件无需重复添加。角色绑定只需保留 `profile`，不再配置 `copyableEx`。复制使用原定义的能力类和组件，并以目标为拥有者；七个测试技能不创建另一份角色机制状态。

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
| arisu.attackEffecters | 爱丽丝对敌定向特效，依次为0档A、1档B、2档C |
| arisu.chargeEffecters / arisu.chargeStateEffecters | 爱丽丝两档自身充能特效／可选持续状态特效，均按第一档、第二档配置 |
| ArisuNormal.xml中的castSound、casterEffecter、targetEffecter | 独立自动普通技能的声音、自身特效和目标特效 |
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
| 爱丽丝 | EX直线长25格、宽3格，5段总倍率1300%，充能后为2倍／3倍；每3次有效普攻命中触发独立100%普通技能 |
| 黑子 | 无人机40秒、消耗最大生命20%、普攻每发触发跟射50%；普通技能间隔30秒，宿主400%＋无人机20×50%普攻基准；不死冷却90秒、第15层死亡 |
| 若藻 | 6发单体连射总倍率100%，记录10秒，原始上限1322%，状态显示BA换算后的待释放值 |
| 星野形态 | 输出／坦克基础攻击30／10、基础生命1000／3000 |
| 星野攻击技能 | EX准备1秒，3×50%单体＋4×87.5%范围；每30发普攻触发3发普通技能；EX或普通技能后强化下一轮6发，每发150%普攻AOE |
| 星野防御技能 | EX持续40秒、攻击＋156%、护盾69.1%、前方拦截半径3格；普通技能独立主动按钮、冷却40秒、实际受伤25次解除 |
| 凯伊 | 场地25秒、半径4.9格；按10%蓄积，上限5000% |
| 莉音 | 复制费用减1 COST；目标攻击＋50%，持续30秒 |

星野防御普通技能状态的属性列表默认留空，只显示受伤计数。需要加成时，在 `BANW_SpecialHoshinoCount` 的 `statOffsets` 中配置。

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
3. **黑子**：使用EX召唤无人机，再让宿主攻击敌人。宿主射出每发子弹就触发无人机追击，即使射偏或被闪避也触发；普通技能锁定目标后自动连射20发，每发播放声音，伤害不享受EX增益。
4. **若藻**：对敌人使用EX，首次实际扣血后查看敌人的“若藻伤害蓄积”状态，再由友军持续攻击该目标。观察待释放伤害增长；记录满10秒后释放，重新施放会重新开始记录。
5. **星野**：使用形态切换按钮，分别操作输出EX和坦克EX。坦克形态选择移动落点，抵达后获得护盾与前方拦截；普通技能次数可在状态窗口查看。
6. **凯伊**：选择场地位置，让其他友军站在范围内攻击敌人。场地结束后，凯伊会寻找合法目标释放蓄积；没有目标时保留金额。
7. **莉音**：对拥有COST技能的友军使用复制技能，选中目标本体使用“复制·EX”按钮。妮露处于二段时显示二段，可右键切换其他EX。成功发动后临时按钮消失，莉音复制入口恢复；取消瞄准不会消耗机会。

需要重新开始时，使用“重置选中角色新技能”，再补满共享COST。保存游戏会保留阶段、次数、蓄积、飞行弹丸、待发连射与复制机会，读取后可继续操作。
