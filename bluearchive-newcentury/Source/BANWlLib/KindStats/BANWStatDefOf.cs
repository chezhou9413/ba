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

        // 升级生命值倍率属性，0.8 表示在默认 100% 基础上增加 80%。
        public static StatDef BANW_HealthLevelMultiplier;

        // 固定生命值加算属性，1 表示 100 点生命值。
        public static StatDef BANW_HealthFlatBonus;

        // 最终生命值加成属性，0.10 表示增加 10%。
        public static StatDef BANW_HealthBonusMultiplier;

        // 静态构造函数，负责触发 DefOf 注入。
        static BANWStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BANWStatDefOf));
        }
    }
}
