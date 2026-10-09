# 妮露EX施法与逐发配置

技能配置文件：`1.6/Defs/AbilityDef/Nero_B.xml`。引导特效定义：`1.6/Defs/Effecter/Nero_C.xml`。

## 一段独立音效

在 `BANW_Special_Nero_Ex` 的 `verbProperties` 中设置：

```xml
<soundCast>Nero_Skill_EX</soundCast>
```

一段成功施放时播放一次，不与二段每发枪声共用配置。当前引用项目已有妮露语音，可以换成其他 `SoundDef`。

## 施法特效的瞄准方向

`SpecialSkillProfileDef/stageEffecter`在一段大亢奋成功施放时播放一次，生成在妮露位置，朝向本次选择的队友。`casterEffecter`一段按普通特效播放，二段交给`JobDriver_NeroExChannel`维护，生成在施法者位置并朝向所选敌人。

水花子的旋转偏移特效可以直接放在这两个入口，例如：

```xml
<stageEffecter>BANW_SpecialArisu_Attack_C</stageEffecter>
```

对应EffecterDef的子节点使用`BANWlLib.BaClass.SubEffecter_SprayerTriggeredRotatedOffset`，并设置`absoluteAngle=false`。`positionOffset`和`rotation`都会叠加本次瞄准角度；`absoluteAngle=true`表示保持配置的固定方向。

使用原水花子子节点时，方向在触发时确定，延迟子节点保持同一方向。需要整个二段引导期间实时转向时，使用下述引导专用子节点。二段攻击敌人的施法表现放在`casterEffecter`；它同样会在一段播放，`stageEffecter`只在一段播放。

`exAttack/effecterDef`是子弹命中位置的特效；它使用`rotateImpactEffect`控制弹道方向，与施法者位置的特效入口不同。

## 与引导工作一致的特效生命周期

实际技能配置使用：

```xml
<casterEffecter>BANW_Nero_B_Ef_D</casterEffecter>
```

`BANW_Nero_B_Ef_D`的Mote子节点使用`BANWlLib.Skills.SubEffecter_ChannelRotatedMote`，引用的`BAWN_Nero_B_7_Mote`至`BAWN_Nero_B_13_Mote`使用`BANWlLib.Skills.Mote_ChannelEffect`。保留子节点的`initialDelayTicks`、`positionOffset`、`scale`、`rotation`和`absoluteAngle`配置；当前贴图轴向校正为`rotation=-90`，实时转向需要`absoluteAngle=false`。

二段成功发动时建立一份Effecter，引导工作每tick推进尚未生成的节点，并按当前目标位置更新已生成Mote的角度和偏移。各节点从自己的延迟时刻开始显示，已经生成的节点保持到本次工作结束。它们的存活时间不受XML中原有的短`solidTime`限制，不需要把共享Mote定义改成一个固定的超长寿命。

最后一发发射结束，或中断、倒地、眩晕、取消征召、离图、目标失效时，Job清理全部已生成的引导Mote，并取消未生成的延迟节点。已经飞出的子弹和其命中特效继续按原有流程结算。

一段使用同一Effecter时仍按Mote自己的淡入、持续和淡出配置播放，不绑定70秒的大亢奋状态。引导起始时刻保存在Job中，读档后重建视觉实例并继续使用原来的延迟时间表。

## 二段持续施法

二段 `BANW_Special_Nero_AlternateEx` 通过 `BANW_Job_NeroExChannel` 执行完整施法。开始时停止武器原有连射，施法期间停步瞄准且禁止武器平A，最后一发发射结束后退出施法。冷却沿用原版技能工作，在施法结束时开始。

玩家中断、倒地、眩晕、取消征召、离图，或目标死亡、离图、超出射程、失去视线时，停止未发射的子弹。已经发出的弹丸继续命中结算；已发动的EX不返还COST。

莉音复制的二段EX同样由妮露本体执行这套持续施法流程。

## 每发枪声与发射tick

在角色配置的 `exAttack` 中设置，以下为10发的填写示例：

```xml
<shots>10</shots>
<shotSound>BANW_Nero_ExShot</shotSound>
<shotTicks>
  <li>1</li><li>7</li><li>13</li><li>19</li><li>25</li>
  <li>31</li><li>37</li><li>43</li><li>49</li><li>55</li>
</shotTicks>
```

每个条目对应一发子弹，表示相对本次EX生效的tick。60 tick为1秒；例如将第十项改为120，最后一发就在生效后2秒发射，期间持续施法。条目数量必须等于 `shots`，每项必须至少为1，并按时间排列；同一tick可以配置多发。

这些数值是各发的发射时刻，不是相邻两发之间的间隔。若只需要统一间隔，删除整个 `shotTicks` 节点，再设置 `shotIntervalTicks`；此时第一发在第1 tick射出，后续按固定间隔发射。

`shotSound` 在每发实际射出时播放。当前实际配置使用`BANW_Wp_SMG_At`，可替换成其他音效定义。被取消的未发射子弹不会播放枪声。

当前实际配置为46发、总攻击力倍率14.69（1469%），按发数均分；发射时刻按引导特效`BANW_Nero_B_Ef_D`的演出节点（0／60／120／180／240 tick）分段：1～56 tick的12发、60～115 tick的12发、120～175 tick的12发、180～240 tick的10发。最后一发在第240 tick，约4.0秒，与特效演出收尾同步；实际引导结束由Job完成流程决定，特效跟随Job结束。逐发时间不会改变总伤害，锐气与大亢奋仍参与伤害倍率。`exAttack.penetration`控制每发护甲穿透。
