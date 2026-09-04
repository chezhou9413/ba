using System.Collections.Generic;

namespace BANWlLib.CostSystem
{
    //技能费用快照负责保存一次施放所采用的基础费用、减费明细与最终费用。
    public sealed class AbilityCostCalculation
    {
        private readonly List<HediffComp_CostDiscount> discounts;

        public int BaseCost { get; }
        public int FlatReduction { get; }
        public float RemainingMultiplier { get; }
        public int EffectiveCost { get; }
        public bool HasDiscount => discounts.Count > 0;

        //构造不可变费用快照并保存参与本次计算的状态列表。
        public AbilityCostCalculation(
            int baseCost,
            int flatReduction,
            float remainingMultiplier,
            int effectiveCost,
            List<HediffComp_CostDiscount> discounts)
        {
            BaseCost = baseCost;
            FlatReduction = flatReduction;
            RemainingMultiplier = remainingMultiplier;
            EffectiveCost = effectiveCost;
            this.discounts = discounts;
        }

        //成功施放后让所有参与本次计算的限次状态各消耗一次。
        public void ConsumeDiscountUses()
        {
            for (int index = 0; index < discounts.Count; index++)
            {
                discounts[index].ConsumeUse();
            }
        }
    }
}
