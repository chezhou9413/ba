using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.CostSystem
{
    //技能费用配置负责在AbilityDef中声明基础COST。
    public sealed class CompProperties_AbilityCost : AbilityCompProperties
    {
        public int cost = 3;

        //构造费用配置并绑定运行时组件类型。
        public CompProperties_AbilityCost()
        {
            compClass = typeof(CompAbilityCost);
        }

        //检查技能基础费用是否落在系统支持范围。
        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (cost < 0 || cost > 20)
            {
                yield return parentDef.defName + " 的技能COST必须位于0到20之间。";
            }
        }
    }

    //技能费用组件负责在技能可用性检查阶段阻止无法支付的施放。
    public sealed class CompAbilityCost : AbilityComp
    {
        public CompProperties_AbilityCost Props => (CompProperties_AbilityCost)props;

        public override bool CanCast
        {
            get
            {
                string reason;
                return BACostPoolService.CanSpend(parent, out reason);
            }
        }

        //向技能按钮返回共享池不足或负值时的禁用原因。
        public override bool GizmoDisabled(out string reason)
        {
            return !BACostPoolService.CanSpend(parent, out reason);
        }
    }
}
