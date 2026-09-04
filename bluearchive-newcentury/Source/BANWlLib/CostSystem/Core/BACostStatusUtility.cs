using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.CostSystem
{
    //COST状态工具负责识别参战学生并汇总回复率、减费和透支状态。
    public static class BACostStatusUtility
    {
        private const string StudentRaceDefName = "BANW_KivotosStudent";

        //判断地图中是否存在满足回复条件的已征召学生。
        public static bool HasDraftedStudent(Map map)
        {
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int index = 0; index < pawns.Count; index++)
            {
                if (IsEligibleDraftedStudent(pawns[index]))
                {
                    return true;
                }
            }

            return false;
        }

        //合计所有已征召学生的回复率偏移并把最低倍率限制为零。
        public static float GetTeamRecoveryMultiplier(Map map)
        {
            float totalOffset = 0f;
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int pawnIndex = 0; pawnIndex < pawns.Count; pawnIndex++)
            {
                Pawn pawn = pawns[pawnIndex];
                if (!IsEligibleDraftedStudent(pawn))
                {
                    continue;
                }

                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                for (int hediffIndex = 0; hediffIndex < hediffs.Count; hediffIndex++)
                {
                    HediffWithComps hediff = hediffs[hediffIndex] as HediffWithComps;
                    HediffComp_CostRecoveryRate comp = hediff?.GetComp<HediffComp_CostRecoveryRate>();
                    if (comp != null)
                    {
                        totalOffset += comp.Props.rateOffset;
                    }
                }
            }

            return UnityEngine.Mathf.Max(0f, 1f + totalOffset);
        }

        //收集指定技能当前可用的全部减费状态。
        public static List<HediffComp_CostDiscount> GetMatchingDiscounts(Ability ability)
        {
            var result = new List<HediffComp_CostDiscount>();
            Pawn pawn = ability?.pawn;
            if (pawn?.health?.hediffSet == null)
            {
                return result;
            }

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int hediffIndex = 0; hediffIndex < hediffs.Count; hediffIndex++)
            {
                HediffWithComps hediff = hediffs[hediffIndex] as HediffWithComps;
                HediffComp_CostDiscount comp = hediff?.GetComp<HediffComp_CostDiscount>();
                if (comp != null && comp.HasUsesRemaining && comp.Matches(ability.def))
                {
                    result.Add(comp);
                }
            }

            return result;
        }

        //取得施法者当前允许的最大透支十分位数。
        public static int GetOverdraftLimitTenths(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return 0;
            }

            int maximum = 0;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int hediffIndex = 0; hediffIndex < hediffs.Count; hediffIndex++)
            {
                HediffWithComps hediff = hediffs[hediffIndex] as HediffWithComps;
                HediffComp_CostOverdraft comp = hediff?.GetComp<HediffComp_CostOverdraft>();
                if (comp != null)
                {
                    maximum = UnityEngine.Mathf.Max(maximum, UnityEngine.Mathf.RoundToInt(comp.Props.overdraftLimit * 10f));
                }
            }

            return maximum;
        }

        //判断Pawn是否属于当前地图可参与COST系统的已征召玩家学生。
        public static bool IsEligibleDraftedStudent(Pawn pawn)
        {
            return pawn != null &&
                   pawn.Spawned &&
                   !pawn.Dead &&
                   pawn.Drafted &&
                   pawn.Faction == Faction.OfPlayer &&
                   pawn.def?.defName == StudentRaceDefName;
        }
    }
}
