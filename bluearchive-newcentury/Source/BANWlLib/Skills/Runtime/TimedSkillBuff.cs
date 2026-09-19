using Verse;
using System.Linq;

namespace BANWlLib.Skills
{
    //可配置限时增益，负责不修改共享HediffDef的绝对过期时间。
    public class TimedSkillBuff : HediffWithComps
    {
        public int endTick = -1;
        public override bool ShouldRemove => endTick >= 0 && Find.TickManager.TicksGame >= endTick;

        //创建或刷新同类增益，负持续时间表示按次数手动移除。
        public static Hediff Apply(Pawn pawn, HediffDef def, int duration)
        {
            var buff = pawn.health.hediffSet.hediffs.OfType<TimedSkillBuff>().FirstOrDefault(h => h.def == def);
            if (buff != null)
            {
                buff.endTick = duration < 0 ? -1 : Find.TickManager.TicksGame + duration;
                return buff;
            }
            buff = (TimedSkillBuff)HediffMaker.MakeHediff(def, pawn);
            buff.endTick = duration < 0 ? -1 : Find.TickManager.TicksGame + duration;
            pawn.health.AddHediff(buff);
            return buff;
        }

        //同类增益保留各自来源与剩余时间，不进行原版合并。
        public override bool TryMergeWith(Hediff other) => false;

        //保存状态的绝对到期时间。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref endTick, "endTick", -1);
        }
    }
}
