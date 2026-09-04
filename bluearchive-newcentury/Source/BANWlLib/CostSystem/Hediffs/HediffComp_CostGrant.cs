using System;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.CostSystem
{
    //直接回复COST状态配置负责声明一次性加入共享池的精确数值。
    public sealed class HediffCompProperties_CostGrant : HediffCompProperties
    {
        public float amount = 1f;

        //构造直接回复配置并绑定运行时组件类型。
        public HediffCompProperties_CostGrant()
        {
            compClass = typeof(HediffComp_CostGrant);
        }

        //检查回复量是否为正数且最多保留一位小数。
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (amount <= 0f)
            {
                yield return parentDef.defName + " 的直接COST回复量必须大于0。";
            }

            if (Math.Abs(amount * 10f - Math.Round(amount * 10f)) > 0.001f)
            {
                yield return parentDef.defName + " 的直接COST回复量最多保留一位小数。";
            }
        }
    }

    //直接回复COST状态负责在获得状态时向所在地图共享池结算并请求移除自身。
    public sealed class HediffComp_CostGrant : HediffComp
    {
        private bool applied;

        public HediffCompProperties_CostGrant Props => (HediffCompProperties_CostGrant)props;
        public override bool CompShouldRemove => applied;

        //状态加入后立即结算一次直接回复。
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (applied)
            {
                return;
            }

            if (!BACostStatusUtility.IsEligibleDraftedStudent(Pawn) || Pawn.Map == null)
            {
                Log.Error("[BA COST] 直接回复状态只能给予已征召、存活的玩家学生。Hediff=" + parent.def.defName);
                applied = true;
                return;
            }

            BACostPoolService.Grant(Pawn.Map, Props.amount);
            applied = true;
        }

        //保存一次性状态是否已经结算，避免读档时重复回复。
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref applied, "applied", false);
        }
    }
}
