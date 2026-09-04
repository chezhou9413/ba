using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace BANWlLib.CostSystem
{
    //技能费用计算器负责按固定减费、百分比乘算和最终向上取整生成费用快照。
    public static class AbilityCostCalculator
    {
        //计算指定技能当前实际COST，没有费用组件时返回空值。
        public static AbilityCostCalculation Calculate(Ability ability)
        {
            CompAbilityCost costComp = FindCostComp(ability);
            if (costComp == null)
            {
                return null;
            }

            int baseCost = Mathf.Max(0, costComp.Props.cost);
            int flatReduction = 0;
            float remainingMultiplier = 1f;
            List<HediffComp_CostDiscount> discounts = BACostStatusUtility.GetMatchingDiscounts(ability);

            for (int index = 0; index < discounts.Count; index++)
            {
                HediffCompProperties_CostDiscount props = discounts[index].Props;
                flatReduction += Mathf.Max(0, props.flatReduction);
                remainingMultiplier *= 1f - Mathf.Clamp01(props.percentageReduction);
            }

            int afterFlat = Mathf.Max(0, baseCost - flatReduction);
            int effectiveCost = afterFlat == 0
                ? 0
                : Mathf.CeilToInt(afterFlat * remainingMultiplier - 0.00001f);

            return new AbilityCostCalculation(
                baseCost,
                flatReduction,
                remainingMultiplier,
                effectiveCost,
                discounts);
        }

        //从技能组件列表中取得唯一的COST组件。
        public static CompAbilityCost FindCostComp(Ability ability)
        {
            if (ability?.comps == null)
            {
                return null;
            }

            for (int index = 0; index < ability.comps.Count; index++)
            {
                CompAbilityCost comp = ability.comps[index] as CompAbilityCost;
                if (comp != null)
                {
                    return comp;
                }
            }

            return null;
        }
    }
}
