# 若藻EX技能配置说明

配置文件：[Wakamo.xml](../1.7/Defs/SpecialSkills/Wakamo.xml)。技能为 `BANW_Special_Wakamo_Ex`（测试新技能·若藻），机制配置为 `BANW_Special_Wakamo`。

## 单体连射

`wakamo.exStages` 按照星野EX的分段结构配置，每个 `li` 包含 `delayTicks` 和独立的 `attack`。每段可以分别设置 `damageDef`、伤害倍率、穿透、弹丸与命中特效，实际命中时使用该段参数结算。`radius` 保持0。

当前六段各发射一发，每段倍率为 `0.5416667`，合计约325%。每段倍率只在本段的 `shots` 内均分，不会再除以整轮发数；例如某段 `attackPowerRatio=1.2`、`shots=2`，该段每发造成60%攻击力伤害。

`jobDef=BANW_Job_WakamoExChannel` 保持前摇和连射期间停步瞄准。当前六段的 `delayTicks` 为180、192、204、216、228、244，默认段内首发延迟1 tick，因此仍在前摇完成后的181、193、205、217、229、245 tick发射。目标死亡后继续指向尸体或最后位置，施法者倒地、眩晕或取消工作会停止未发射的子弹。

| 字段 | 作用 |
| --- | --- |
| `delayTicks` | 本段相对前摇结束的延迟；实际发射时刻还需加上段内延迟 |
| `attack.shots` | 本段发数，至少1发；整轮各段合计至少2发 |
| `attack.shotIntervalTicks` | 不填写shotTicks时的段内发射间隔，首发延迟1 tick；60 tick为1秒 |
| `attack.shotTicks` | 可选的段内逐发时刻，另加delayTicks；条目数与shots一致，不小于1且按非递减顺序排列 |
| `attack.attackPowerRatio` | 本段总伤害倍率，当前每段0.5416667 |
| `attack.projectileDef` | 本段实体弹丸定义，当前 `BANW_Bullet_Wakamo_EXX` |
| `attack.damageDef` | 本段独立伤害类型，当前各段为 `BANC_Damage_Wakamo_EX_B` |
| `attack.penetration` | 每发护甲穿透，当前2；0.5表示50%，不按发数均分 |
| `attack.shotSound` | 本段可选的逐发射击音效 |
| `attack.effecterDef` | 本段攻击特效 |

例如，编辑 `wakamo.exStages` 中某个 `li` 即可单独改变该段；下列为一段配置，其余段独立保留：

```xml
<li>
  <delayTicks>180</delayTicks>
  <attack>
    <useBattleStats>true</useBattleStats>
    <attackPowerRatio>0.5416667</attackPowerRatio>
    <damageDef>BANC_Damage_Wakamo_EX_B</damageDef>
    <penetration>2</penetration>
    <isExSkill>true</isExSkill>
    <canCrit>true</canCrit>
    <applyAffinity>true</applyAffinity>
    <radius>0</radius>
    <shots>1</shots>
    <projectileDef>BANW_Bullet_Wakamo_EXX</projectileDef>
    <affectHostile>true</affectHostile>
    <affectFriendly>false</affectFriendly>
    <canHitOwnPawn>false</canHitOwnPawn>
  </attack>
</li>
```

技能按钮的伤害预览读取全部伤害段。任一段允许影响的阵营即可选为目标，每段命中时仍按自身阵营开关过滤。原来的若藻 `exAttack` 配置由 `wakamo.exStages` 替代。

## 敌方蓄积状态

任一EX子弹首次造成实际伤害后，给目标附加 `buffHediff` 指定的 `BANW_WakamoAccumulation`。首发有效伤害也会计入，此后若藻和同阵营单位对该目标造成的实际伤害均参与累计。完全被闪避、护甲或护盾抵挡的攻击不增加数值。

| 字段 | 作用 |
| --- | --- |
| `durationTicks` | 从状态建立起计算的持续时间，默认600，即10秒 |
| `recordRatio` | 实际伤害计入原始蓄积的比例，默认1 |
| `recordCapRatio` | 原始蓄积上限相对施法时攻击力的倍率，默认13.22 |
| `stateEffecter` | 蓄积期间附着在敌人身上的持续特效 |
| `endEffecter` | 到期时目标位置的释放特效 |

原始蓄积 = min（原始上限，累计实际伤害 × `recordRatio`）。上限基数取首次实际造成伤害、建立标记的那一段：开启该段 `attack.useBattleStats` 时读取施法时BA攻击力，关闭时使用该段 `attack.basePower`。后续伤害段不会重置标记、持续时间或蓄积上限。

待释放值 = 原始蓄积 × 本次爆发的BA换算倍率。

换算使用 `burstAttack` 配置和若藻的施法属性快照，包括输出倍率、暴击、伤害类型克制和EX加成。原始蓄积本身作为基础伤害，不再乘一次攻击力。标记建立时固定换算倍率与暴击结果，后续累计只更新金额，查看状态不会重新掷暴击。

敌人健康面板的状态显示待释放值，悬浮信息同时显示来源、换算后的上限、原始蓄积、伤害类型、暴击结果和剩余时间。这里的待释放值已经过BA输出计算；目标释放时的闪避、护甲与护盾仍可能减少最终扣血。

## 到期爆发

`burstAttack` 默认一次单体直接伤害，`shots=1`、`radius=0`。到期先终止记录，再释放状态中显示的金额，不重复计算BA输出，不再次参与若藻或凯伊的蓄积。

| 字段 | 作用 |
| --- | --- |
| `damageDef` | 爆发伤害类型，独立于各连射段，当前 `BANC_Damage_Wakamo_EX` |
| `penetration` | 爆发护甲穿透，独立于连射配置 |
| `attackPowerRatio` | 蓄积金额的爆发倍率，默认1 |
| `useBattleStats` | 是否按BA属性换算蓄积，默认true |
| `isExSkill` | 是否应用EX输出加成，默认true |
| `canCrit` / `alwaysCrit` | 是否允许暴击／固定暴击 |
| `applyAffinity` | 是否计算伤害类型克制，默认true |
| `effecterDef` | 爆发伤害特效 |

同一若藻再次施放会清除旧记录并开始新的连射，不提前引爆。旧批次的在途子弹不能重新建立已取消的记录。目标或若藻死亡、离图，或者蓄积状态被移除时取消记录。不同施法者的标记分别保存和累计。

## 定向命中特效

`BANWlLib.Projectiles.SubEffecter_DirectionalImpactMote` 直接读取子节点 `positionOffset` 和 `speed`。`absoluteAngle=false` 时偏移沿弹道旋转，三个分量依次为弹道右方、绘制高度、弹道前方；`absoluteAngle=true` 时保留固定地图轴偏移。例如 `<positionOffset>(0, 0.5, 0)</positionOffset>` 只提高绘制高度0.5，不改变地图上的生成格子。`<speed>0</speed>` 表示原地播放，不再被弹丸扩展中的速度覆盖。延迟节点保留命中瞬间的方向与位置。
