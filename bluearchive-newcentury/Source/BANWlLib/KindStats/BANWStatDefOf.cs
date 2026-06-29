using RimWorld;
using Verse;

namespace BANWlLib.KindStats
{
    // 自定义属性 Def 引用，负责为射程和生命值补丁提供稳定入口。
    [DefOf]
    public static class BANWStatDefOf
    {
        // 非近战攻击最终射程平加属性。
        public static StatDef BANW_RangedWeapon_RangeOffset;

        // 静态构造函数，负责触发 DefOf 注入。
        static BANWStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BANWStatDefOf));
        }
    }
}
