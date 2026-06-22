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

        // 静态构造函数，确保DefOf正确初始化
        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
    }
} 