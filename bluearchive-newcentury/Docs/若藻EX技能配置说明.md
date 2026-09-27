# 若藻EX技能配置说明

配置文件：[Wakamo.xml](../1.6/Defs/SpecialSkills/Wakamo.xml)。技能为 `BANW_Special_Wakamo_Ex`（测试新技能·若藻），机制配置为 `BANW_Special_Wakamo`。

## 单体连射

`exAttack` 默认发射6颗单体实体弹丸，总基础倍率为攻击力100%，由6发均分，每发分别进行BA伤害结算。`radius` 保持0。

| 字段 | 作用 |
| --- | --- |
| `shots` | 连射发数，至少2发 |
| `shotIntervalTicks` | 相邻发射间隔，默认6 tick；60 tick为1秒 |
| `shotTicks` | 可选的逐发发射时刻列表，填写时数量与发数一致，从第1 tick开始，按非递减顺序排列 |
| `attackPowerRatio` | 整轮总伤害倍率，默认1 |
| `projectileDef` | 实体弹丸定义，默认 `BANW_SpecialSkillBullet` |
| `damageDef` | 连射伤害类型，默认 `BANC_Damage_shenmi` |
| `penetration` | 每发护甲穿透，默认0；0.5表示50%，不按发数均分 |
| `shotSound` | 可选的逐发射击音效 |
| `effecterDef` | 本段攻击特效 |

## 敌方蓄积状态

任一EX子弹首次造成实际伤害后，给目标附加 `buffHediff` 指定的 `BANW_WakamoAccumulation`。首发有效伤害也会计入，此后若藻和同阵营单位对该目标造成的实际伤害均参与累计。完全被闪避、护甲或护盾抵挡的攻击不增加数值。

| 字段 | 作用 |
| --- | --- |
| `durationTicks` | 从状态建立起计算的持续时间，默认600，即10秒 |
| `recordRatio` | 实际伤害计入原始蓄积的比例，默认1 |
| `recordCapRatio` | 原始蓄积上限相对施法时攻击力的倍率，默认13.22 |
| `stateEffecter` | 蓄积期间附着在敌人身上的持续特效 |
| `endEffecter` | 到期时目标位置的释放特效 |

原始蓄积 = min（原始上限，累计实际伤害 × `recordRatio`）。开启 `exAttack.useBattleStats` 时，上限基数读取施法时BA攻击力；关闭时使用 `exAttack.basePower`。

待释放值 = 原始蓄积 × 本次爆发的BA换算倍率。

换算使用 `burstAttack` 配置和若藻的施法属性快照，包括输出倍率、暴击、伤害类型克制和EX加成。原始蓄积本身作为基础伤害，不再乘一次攻击力。标记建立时固定换算倍率与暴击结果，后续累计只更新金额，查看状态不会重新掷暴击。

敌人健康面板的状态显示待释放值，悬浮信息同时显示来源、换算后的上限、原始蓄积、伤害类型、暴击结果和剩余时间。这里的待释放值已经过BA输出计算；目标释放时的闪避、护甲与护盾仍可能减少最终扣血。

## 到期爆发

`burstAttack` 默认一次单体直接伤害，`shots=1`、`radius=0`。到期先终止记录，再释放状态中显示的金额，不重复计算BA输出，不再次参与若藻或凯伊的蓄积。

| 字段 | 作用 |
| --- | --- |
| `damageDef` | 爆发伤害类型，可与连射不同，默认 `BANC_Damage_shenmi` |
| `penetration` | 爆发护甲穿透，独立于连射配置 |
| `attackPowerRatio` | 蓄积金额的爆发倍率，默认1 |
| `useBattleStats` | 是否按BA属性换算蓄积，默认true |
| `isExSkill` | 是否应用EX输出加成，默认true |
| `canCrit` / `alwaysCrit` | 是否允许暴击／固定暴击 |
| `applyAffinity` | 是否计算伤害类型克制，默认true |
| `effecterDef` | 爆发伤害特效 |

同一若藻再次施放会清除旧记录并开始新的连射，不提前引爆。旧批次的在途子弹不能重新建立已取消的记录。目标或若藻死亡、离图，或者蓄积状态被移除时取消记录。不同施法者的标记分别保存和累计。
