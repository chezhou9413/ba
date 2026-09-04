using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BANWlLib.CostSystem
{
    //本地目标技能补丁负责在实际发动前原子扣除共享COST。
    [HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
    public static class AbilityActivateLocalCostPatch
    {
        //在技能效果与冷却开始前完成最终费用检查。
        public static bool Prefix(Ability __instance, ref bool __result)
        {
            string reason;
            if (BACostPoolService.TrySpend(__instance, out reason))
            {
                return true;
            }

            Messages.Message(reason, MessageTypeDefOf.RejectInput, false);
            __result = false;
            return false;
        }
    }

    //世界目标技能补丁负责为跨地图目标技能执行同一套原子扣费。
    [HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(GlobalTargetInfo))]
    public static class AbilityActivateGlobalCostPatch
    {
        //在世界目标技能效果与冷却开始前完成最终费用检查。
        public static bool Prefix(Ability __instance, ref bool __result)
        {
            string reason;
            if (BACostPoolService.TrySpend(__instance, out reason))
            {
                return true;
            }

            Messages.Message(reason, MessageTypeDefOf.RejectInput, false);
            __result = false;
            return false;
        }
    }

    //技能按钮费用标签补丁负责显示经过状态修正后的实际COST。
    [HarmonyPatch(typeof(Command_Ability), "get_TopRightLabel")]
    public static class CommandAbilityCostLabelPatch
    {
        //把实际费用放到技能按钮右上角并保留原有充能标签。
        public static void Postfix(Command_Ability __instance, ref string __result)
        {
            AbilityCostCalculation calculation = BACostPoolService.GetEffectiveCost(__instance.Ability);
            if (calculation == null)
            {
                return;
            }

            string costLabel = "COST " + calculation.EffectiveCost;
            __result = __result.NullOrEmpty() ? costLabel : costLabel + "  " + __result;
        }
    }

    //技能按钮说明补丁负责展示基础费用、实际费用和减费顺序。
    [HarmonyPatch(typeof(Command_Ability), "get_Tooltip")]
    public static class CommandAbilityCostTooltipPatch
    {
        //在既有技能说明末尾追加共享COST明细。
        public static void Postfix(Command_Ability __instance, ref string __result)
        {
            AbilityCostCalculation calculation = BACostPoolService.GetEffectiveCost(__instance.Ability);
            if (calculation == null)
            {
                return;
            }

            __result += "\n\nCOST：" + calculation.EffectiveCost;
            if (calculation.HasDiscount)
            {
                int reductionPercent = UnityEngine.Mathf.RoundToInt((1f - calculation.RemainingMultiplier) * 100f);
                __result += "\n基础COST：" + calculation.BaseCost;
                __result += "\n固定减费：-" + calculation.FlatReduction;
                __result += "\n百分比减费：" + reductionPercent + "%";
            }

            string reason;
            if (!BACostPoolService.CanSpend(__instance.Ability, out reason))
            {
                __result += "\n无法使用：" + reason;
            }
        }
    }
}
