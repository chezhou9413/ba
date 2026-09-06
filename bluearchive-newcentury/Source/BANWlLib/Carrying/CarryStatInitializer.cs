using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.Carrying
{
    //搬运属性初始化器负责把倍率接入原版容量计算和属性说明。
    [StaticConstructorOnStartup]
    public static class CarryStatInitializer
    {
        //通过原版关联属性机制放大搬运容量，保留容量因子和物品堆叠上限。
        static CarryStatInitializer()
        {
            StatDef carrying = StatDefOf.CarryingCapacity;
            if (carrying.statFactors == null) carrying.statFactors = new List<StatDef>();
            if (!carrying.statFactors.Contains(CarryStatDefOf.BANW_HaulingCapacityMultiplier))
                carrying.statFactors.Add(CarryStatDefOf.BANW_HaulingCapacityMultiplier);
            carrying.cacheable = false;
        }
    }
}
