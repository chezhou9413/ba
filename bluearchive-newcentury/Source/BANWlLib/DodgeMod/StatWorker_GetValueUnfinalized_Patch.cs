using HarmonyLib;
using RimWorld;
using Verse;

namespace DodgeMod
{
    // 闪避属性计算补丁，负责把特性、装备和状态中的 BANW_Miss 平加值汇总到 Pawn 身上。
    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public static class StatWorker_GetValueUnfinalized_Patch
    {
        // 汇总未最终化属性值，负责让 BANW_Miss 能读取 Pawn 身上各来源的 statOffsets。
        public static void Postfix(ref float __result, StatRequest req, StatDef ___stat)
        {
            if (___stat != DodgeStatDefOf.BANW_Miss || !req.HasThing || !(req.Thing is Pawn pawn) || pawn.Destroyed)
            {
                return;
            }

            __result = 0f;
            AddTraitOffsets(pawn, ref __result);
            AddApparelOffsets(pawn, ref __result);
            AddHediffOffsets(pawn, ref __result);
        }

        // 汇总特性闪避值，负责读取 TraitDef 当前阶段的 statOffsets。
        private static void AddTraitOffsets(Pawn pawn, ref float result)
        {
            if (pawn.story?.traits?.allTraits == null)
            {
                return;
            }

            foreach (Trait trait in pawn.story.traits.allTraits)
            {
                if (trait?.CurrentData?.statOffsets != null)
                {
                    result += StatUtility.GetStatOffsetFromList(trait.CurrentData.statOffsets, DodgeStatDefOf.BANW_Miss);
                }
            }
        }

        // 汇总装备闪避值，负责读取穿戴装备的 BANW_Miss 属性。
        private static void AddApparelOffsets(Pawn pawn, ref float result)
        {
            if (pawn.apparel?.WornApparel == null)
            {
                return;
            }

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel != null)
                {
                    result += apparel.GetStatValue(DodgeStatDefOf.BANW_Miss);
                }
            }
        }

        // 汇总状态闪避值，负责读取当前 Hediff 阶段的 statOffsets。
        private static void AddHediffOffsets(Pawn pawn, ref float result)
        {
            if (pawn.health?.hediffSet?.hediffs == null)
            {
                return;
            }

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff?.CurStage?.statOffsets != null)
                {
                    result += StatUtility.GetStatOffsetFromList(hediff.CurStage.statOffsets, DodgeStatDefOf.BANW_Miss);
                }
            }
        }
    }
}
