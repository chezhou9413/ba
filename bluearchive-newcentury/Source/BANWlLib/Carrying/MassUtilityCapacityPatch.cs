using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Carrying
{
    //质量负重补丁负责让背包、商队与装载系统读取装备、特性和状态修正后的承重。
    [HarmonyPatch(typeof(MassUtility), nameof(MassUtility.Capacity))]
    public static class MassUtilityCapacityPatch
    {
        //在基础承重上先加固定公斤数再乘倍率，并在原版负重说明中追加修正量。
        public static void Postfix(Pawn p, StringBuilder explanation, ref float __result)
        {
            if (!MassUtility.CanEverCarryAnything(p)) return;
            float offset = p.GetStatValue(CarryStatDefOf.BANW_CarryMassOffset);
            float multiplier = p.GetStatValue(CarryStatDefOf.BANW_CarryMassMultiplier);
            float previous = __result;
            __result = Mathf.Max(0f, (__result + offset) * multiplier);
            if (explanation != null && !Mathf.Approximately(previous, __result))
            {
                explanation.AppendLine();
                explanation.Append("    搬运词条：固定 " + offset.ToString("+0.##;-0.##;0")
                    + " kg，倍率 " + multiplier.ToStringPercent() + "，修正后 " + __result.ToString("0.##") + " kg");
            }
        }
    }
}
