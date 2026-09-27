using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //敌方若藻标记，负责显示实际待释放数值、维护目标特效并保存本次BA换算结果。
    public class Hediff_WakamoMark : HediffWithComps
    {
        public Hediff_SpecialSkillState owner;
        public float rawAmount;
        public float rawCap;
        public float damageFactor;
        public bool critical;
        private Effecter effect;

        public override bool Visible => true;
        public override bool ShouldRemove => false;
        public override string LabelInBrackets => $"蓄积伤害 {owner?.recorded ?? 0f:0.##}";
        public override string TipStringExtra => owner == null ? "" :
            $"来源：{owner.pawn.LabelShort}\n待释放伤害：{owner.recorded:0.##} / {owner.recordCap:0.##}" +
            $"\n原始累计：{rawAmount:0.##} / {rawCap:0.##}\nBA换算：×{damageFactor:0.###}　暴击：{(critical ? "是" : "否")}" +
            $"\n伤害类型：{owner.profile.burstAttack.damageDef.label}\n剩余：{Mathf.Max(0, owner.endTick - owner.Now) / 60f:0.0}秒" +
            "\n显示值已计算BA输出，释放时仍接受目标闪避、护甲与护盾判定。";

        //不同若藻的标记独立保存，禁止同名状态合并。
        public override bool TryMergeWith(Hediff other) => false;

        //只累计实际扣血，并同步显示同一份已经BA换算的释放金额。
        public void AddDamage(float amount)
        {
            rawAmount = Mathf.Min(rawCap, rawAmount + amount * owner.profile.recordRatio);
            owner.recorded = rawAmount * damageFactor;
            owner.recordCap = rawCap * damageFactor;
        }

        //维护敌人身上的持续特效，失效来源或目标离图时移除标记。
        public override void Tick()
        {
            base.Tick();
            if (owner == null || owner.wakamo.mark != this || owner.pawn.Dead || pawn.Dead ||
                !pawn.Spawned || owner.pawn.Map != pawn.Map || !owner.pawn.health.hediffSet.hediffs.Contains(owner))
            { pawn.health.RemoveHediff(this); return; }
            if (owner.profile.stateEffecter == null) return;
            if (effect == null)
            {
                effect = owner.profile.stateEffecter.Spawn();
                effect.Trigger(pawn, pawn);
            }
            effect.EffectTick(pawn, pawn);
        }

        //清理持续表现并废弃仍指向该标记的记录，防止被驱散后继续爆发。
        public override void PostRemoved()
        {
            effect?.Cleanup();
            effect = null;
            if (owner?.wakamo.mark == this)
            {
                owner.wakamo.mark = null;
                WakamoSkills.Clear(owner);
            }
            base.PostRemoved();
        }

        //保存来源、累计金额与暴击结果，读档不重新随机BA结算。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref owner, "owner");
            Scribe_Values.Look(ref rawAmount, "rawAmount");
            Scribe_Values.Look(ref rawCap, "rawCap");
            Scribe_Values.Look(ref damageFactor, "damageFactor");
            Scribe_Values.Look(ref critical, "critical");
        }
    }
}
