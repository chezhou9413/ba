using Verse;
using RimWorld;

namespace BANWlLib
{
    // Hediff定义引用类
    // 用来在代码中引用XML定义的Hediff
    [DefOf]
    public static class HediffDefOf
    {
        // 减伤状态Hediff定义
        // 对应XML中的DamageReductionStatus
        public static HediffDef DamageReductionStatus;

        //阶级成长显示状态定义，在健康页显示当前阶级提供的成长属性。
        public static HediffDef BANW_StarGrowthDisplayStatus;

        //初始化 DefOf 引用。
        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
    }
} 
