using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //特殊生命操作，负责最大生命换算与不经过攻击结算的安全生命消耗。
    public static class SpecialHealthUtility
    {
        //将项目生命倍率转换为面板生命基数。
        public static float MaximumHealth(Pawn pawn) => pawn == null ? 0f : pawn.HealthScale * 100f;

        //通过可治疗伤口消耗指定最大生命比例，不破坏部位且不触发攻击事件。
        public static void SpendNonlethal(Pawn pawn, float ratio)
        {
            float current = pawn.health.summaryHealth.SummaryHealthPercent;
            float desired = Mathf.Max(0.05f, 1f / MaximumHealth(pawn), current - Mathf.Clamp01(ratio));
            if (desired >= current) return;
            //普通伤口要求部位的实际受击覆盖率大于零；生命消耗只使用仍有安全余量的外部部位。
            var parts = pawn.health.hediffSet.GetNotMissingParts(depth: BodyPartDepth.Outside)
                .Where(part => part.coverageAbs > 0f && pawn.health.hediffSet.GetPartHealth(part) > 1f)
                .OrderByDescending(part => pawn.health.hediffSet.GetPartHealth(part)).ToList();
            foreach (BodyPartRecord part in parts)
            {
                current = pawn.health.summaryHealth.SummaryHealthPercent;
                if (current <= desired + 0.0001f) break;
                var injury = (Hediff_Injury)HediffMaker.MakeHediff(HealthUtility.GetHediffDefFromDamage(DamageDefOf.Blunt, pawn, part), pawn, part);
                float existingSeverity = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                    .Where(h => h.def == injury.def && h.Part == part && !h.IsPermanent()).Sum(h => h.Severity);
                injury.Severity = Mathf.Min((1f - desired / current) * (75f * pawn.HealthScale - existingSeverity),
                    Mathf.Max(0f, pawn.health.hediffSet.GetPartHealth(part) - 1f));
                injury.destroysBodyParts = false;
                LimitToSurvivable(pawn, injury);
                if (injury.Severity > 0.001f) pawn.health.AddHediff(injury, part);
            }
        }

        //对已确认可能致死的伤口寻找仍可存活的严重度，不处理其他状态。
        public static void LimitToSurvivable(Pawn pawn, Hediff_Injury injury)
        {
            injury.Severity = Mathf.Min(injury.Severity, Mathf.Max(0f, pawn.health.hediffSet.GetPartHealth(injury.Part) - 1f));
            if (!pawn.health.WouldDieAfterAddingHediff(injury)) return;
            float low = 0f;
            float high = injury.Severity;
            for (int i = 0; i < 16; i++)
            {
                injury.Severity = (low + high) * 0.5f;
                if (pawn.health.WouldDieAfterAddingHediff(injury)) high = injury.Severity;
                else low = injury.Severity;
            }
            injury.Severity = low;
        }
    }
}
