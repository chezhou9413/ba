using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.CostSystem
{
    //COST减费状态配置负责声明目标技能、固定减费、百分比减费和可用次数。
    public sealed class HediffCompProperties_CostDiscount : HediffCompProperties
    {
        public List<AbilityDef> affectedAbilities = new List<AbilityDef>();
        public int flatReduction;
        public float percentageReduction;
        public int maxUses = 1;

        //构造减费配置并绑定运行时组件类型。
        public HediffCompProperties_CostDiscount()
        {
            compClass = typeof(HediffComp_CostDiscount);
        }

        //检查减费比例、固定值与次数配置是否有效。
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (flatReduction < 0)
            {
                yield return parentDef.defName + " 的 flatReduction 不能小于0。";
            }

            if (percentageReduction < 0f || percentageReduction > 1f)
            {
                yield return parentDef.defName + " 的 percentageReduction 必须位于0到1之间。";
            }

            if (maxUses != -1 && maxUses <= 0)
            {
                yield return parentDef.defName + " 的 maxUses 只能为-1或正整数。";
            }
        }
    }

    //COST减费状态负责筛选目标技能、保存剩余次数并在成功施放后消费次数。
    public sealed class HediffComp_CostDiscount : HediffComp
    {
        private int remainingUses = int.MinValue;

        public HediffCompProperties_CostDiscount Props => (HediffCompProperties_CostDiscount)props;
        public bool HasUsesRemaining => RemainingUses == -1 || RemainingUses > 0;
        public int RemainingUses => remainingUses == int.MinValue ? Props.maxUses : remainingUses;
        public override bool CompShouldRemove => RemainingUses == 0;

        //状态创建时按配置初始化剩余次数。
        public override void CompPostMake()
        {
            base.CompPostMake();
            remainingUses = Props.maxUses;
        }

        //判断该状态是否作用于指定AbilityDef，空列表代表全部COST技能。
        public bool Matches(AbilityDef abilityDef)
        {
            return Props.affectedAbilities.NullOrEmpty() || Props.affectedAbilities.Contains(abilityDef);
        }

        //成功施放后消费一次有限次数并在归零时移除状态。
        public void ConsumeUse()
        {
            if (RemainingUses == -1)
            {
                return;
            }

            remainingUses = UnityEngine.Mathf.Max(0, RemainingUses - 1);
            if (remainingUses == 0 && Pawn?.health != null)
            {
                Pawn.health.RemoveHediff(parent);
            }
        }

        //显示限次状态的剩余施放次数和当前减费内容。
        public override string CompTipStringExtra
        {
            get
            {
                string text = "固定COST减费：-" + Props.flatReduction +
                              "\n百分比COST减费：" + Props.percentageReduction.ToStringPercent();
                return RemainingUses == -1 ? text : text + "\n剩余次数：" + RemainingUses;
            }
        }

        //保存剩余次数并在旧数据缺少字段时采用当前Def配置。
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref remainingUses, "remainingUses", int.MinValue);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && remainingUses == int.MinValue)
            {
                remainingUses = Props.maxUses;
            }
        }
    }
}
