using System.Linq;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //对决标记只记录持续时间，不改变持有者的通用EX伤害属性。
    public class Hediff_NeroDuel : Hediff
    {
        public int endTick;
        public override bool ShouldRemove => Find.TickManager.TicksGame >= endTick;
        public override string TipStringExtra => "对决剩余：" +
            (Mathf.Max(0, endTick - Find.TickManager.TicksGame) / 60f).ToString("0.0") + "秒";

        //施加或刷新对决，同一角色只保留一份有效标记。
        public static void Apply(Pawn pawn, int ticks)
        {
            var mark = pawn.health.hediffSet.hediffs.OfType<Hediff_NeroDuel>().FirstOrDefault();
            if (mark == null)
            {
                mark = (Hediff_NeroDuel)HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("BANW_NeroDuel"), pawn);
                pawn.health.AddHediff(mark);
            }
            mark.endTick = Find.TickManager.TicksGame + ticks;
        }

        //按绝对时间判断标记，避免过期Hediff尚未移除时仍获得增伤。
        public static bool ActiveOn(Pawn pawn) => pawn.health.hediffSet.hediffs
            .OfType<Hediff_NeroDuel>().Any(mark => !mark.ShouldRemove);

        //保存对决到期时间。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref endTick, "endTick");
        }
    }
}
