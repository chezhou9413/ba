using System;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.CostSystem
{
    //COST过载状态配置负责声明该学生允许抵达的负COST下限。
    public sealed class HediffCompProperties_CostOverdraft : HediffCompProperties
    {
        public float overdraftLimit = 5f;

        //构造过载配置并绑定运行时组件类型。
        public HediffCompProperties_CostOverdraft()
        {
            compClass = typeof(HediffComp_CostOverdraft);
        }

        //检查过载额度是否位于0到5点之间并最多保留一位小数。
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (overdraftLimit <= 0f || overdraftLimit > 5f)
            {
                yield return parentDef.defName + " 的 overdraftLimit 必须大于0且不超过5。";
            }

            if (Math.Abs(overdraftLimit * 10f - Math.Round(overdraftLimit * 10f)) > 0.001f)
            {
                yield return parentDef.defName + " 的 overdraftLimit 最多保留一位小数。";
            }
        }
    }

    //COST过载状态负责向施放校验暴露当前学生的透支额度。
    public sealed class HediffComp_CostOverdraft : HediffComp
    {
        public HediffCompProperties_CostOverdraft Props => (HediffCompProperties_CostOverdraft)props;

        public override string CompTipStringExtra => "可将共享COST透支至 -" + Props.overdraftLimit.ToString("0.0");
    }
}
