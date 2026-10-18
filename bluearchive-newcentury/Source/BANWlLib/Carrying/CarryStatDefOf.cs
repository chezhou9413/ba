using RimWorld;
using Verse;

namespace BANWlLib.Carrying
{
    //搬运属性引用绑定单次搬运倍率与独立的质量负重属性。
    [DefOf]
    public static class CarryStatDefOf
    {
        public static StatDef BANW_HaulingCapacityMultiplier;
        public static StatDef BANW_CarryMassOffset;
        public static StatDef BANW_CarryMassMultiplier;

        //初始化 DefOf 引用。
        static CarryStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CarryStatDefOf));
        }
    }
}
