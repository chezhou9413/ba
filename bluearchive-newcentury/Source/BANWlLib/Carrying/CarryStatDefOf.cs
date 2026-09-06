using RimWorld;
using Verse;

namespace BANWlLib.Carrying
{
    //搬运属性引用负责绑定单次搬运倍率与独立的质量负重属性。
    [DefOf]
    public static class CarryStatDefOf
    {
        public static StatDef BANW_HaulingCapacityMultiplier;
        public static StatDef BANW_CarryMassOffset;
        public static StatDef BANW_CarryMassMultiplier;

        //确保使用字段前完成定义绑定。
        static CarryStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CarryStatDefOf));
        }
    }
}
