using RimWorld;
using Verse;

namespace BANWlLib.KindStats
{
    /// <summary>
    /// 自定义属性 Def 引用，负责为射程和生命值补丁提供稳定入口。
    /// </summary>
    [DefOf]
    public static class BANWStatDefOf
    {
        /// <summary>
        /// 非近战攻击最终射程平加属性。
        /// </summary>
        public static StatDef BANW_RangedWeapon_RangeOffset;

        /// <summary>
        /// 生命值尺度平加属性。
        /// </summary>
        public static StatDef BANW_HealthScaleOffset;

        /// <summary>
        /// 生命值尺度百分比加成属性，0.10 表示增加 10%。
        /// </summary>
        public static StatDef BANW_HealthScalePercentOffset;

        /// <summary>
        /// 静态构造函数，负责触发 DefOf 注入。
        /// </summary>
        static BANWStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BANWStatDefOf));
        }
    }
}
