# 妮露EX施法与逐发配置

配置文件：`1.6/Defs/SpecialSkills/Nero.xml`。

## 一段独立音效

在 `BANW_Special_Nero_Ex` 的 `verbProperties` 中设置：

```xml
<soundCast>Nero_Skill_EX</soundCast>
```

一段成功施放时播放一次，不与二段每发枪声共用配置。当前引用项目已有妮露语音，可以换成其他 `SoundDef`。

## 二段持续施法

二段 `BANW_Special_Nero_AlternateEx` 通过 `BANW_Job_NeroExChannel` 执行完整施法。开始时停止武器原有连射，施法期间停步瞄准且禁止武器平A，最后一发发射结束后退出施法。冷却沿用原版技能工作，在施法结束时开始。

玩家中断、倒地、眩晕、取消征召、离图，或目标死亡、离图、超出射程、失去视线时，停止未发射的子弹。已经发出的弹丸继续命中结算；已发动的EX不返还COST。

莉音复制的二段EX同样由妮露本体执行这套持续施法流程。

## 每发枪声与发射tick

在角色配置的 `exAttack` 中设置：

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

`shotSound` 在每发实际射出时播放。`NeroEffects.xml` 中的 `BANW_Nero_ExShot` 复用项目现有冲锋枪素材，允许密集枪声重叠；可替换其素材或改为其他音效定义。被取消的未发射子弹不会播放枪声。

二段仍默认10发、总倍率1300%，按发数均分；逐发时间不会改变总伤害。`exAttack.penetration` 继续控制每发护甲穿透。
