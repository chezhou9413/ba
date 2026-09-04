using Verse;

namespace BANWlLib.CostSystem
{
    //COST回复率状态配置负责声明该学生对地图共享回复速度的加算偏移。
    public sealed class HediffCompProperties_CostRecoveryRate : HediffCompProperties
    {
        public float rateOffset;

        //构造回复率配置并绑定运行时组件类型。
        public HediffCompProperties_CostRecoveryRate()
        {
            compClass = typeof(HediffComp_CostRecoveryRate);
        }
    }

    //COST回复率状态负责向地图汇总逻辑暴露当前回复率偏移。
    public sealed class HediffComp_CostRecoveryRate : HediffComp
    {
        public HediffCompProperties_CostRecoveryRate Props => (HediffCompProperties_CostRecoveryRate)props;

        public override string CompTipStringExtra => "共享COST回复率：" + Props.rateOffset.ToStringPercentSigned();
    }
}
