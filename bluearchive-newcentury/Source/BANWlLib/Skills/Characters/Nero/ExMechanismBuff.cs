using System.Linq;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //独立EX乘区状态，负责让大亢奋作用于角色全部BA结算EX且不混入加算属性。
    public class ExMechanismBuff : Hediff
    {
        public float multiplier = 1f;
        public int endTick;
        public override bool ShouldRemove => Find.TickManager.TicksGame >= endTick;
        public override string TipStringExtra => "EX独立倍率：" + multiplier.ToString("0.##");

        //施加或刷新同类状态，重复获得时覆盖而不反复相乘。
        public static void Apply(Pawn pawn, float multiplier, int ticks)
        {
            var buff = pawn.health.hediffSet.hediffs.OfType<ExMechanismBuff>().FirstOrDefault();
            if (buff == null)
            {
                buff = (ExMechanismBuff)HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("BANW_SpecialExMechanism"), pawn);
                pawn.health.AddHediff(buff);
            }
            buff.multiplier = multiplier;
            buff.endTick = Find.TickManager.TicksGame + ticks;
        }

        //取得角色当前有效的大亢奋倍率。
        public static float Factor(Pawn pawn)
        {
            var buff = pawn?.health?.hediffSet.hediffs.OfType<ExMechanismBuff>().FirstOrDefault(b => !b.ShouldRemove);
            return buff?.multiplier ?? 1f;
        }

        //保存倍率与绝对过期时间。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref multiplier, "multiplier", 1f);
            Scribe_Values.Look(ref endTick, "endTick");
        }
    }
}

