using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // BA战斗属性引用，负责让代码安全访问 XML 中定义的战斗 StatDef。
    [DefOf]
    public static class BattleStatDefOf
    {
        public static StatDef BANW_RangedWeapon_Damage;
        public static StatDef BANW_FinalDamageMultiplier;
        public static StatDef BANW_CriticalChance;
        public static StatDef BANW_CriticalDamage;
        public static StatDef BANW_HealPowerBase;
        public static StatDef BANW_HealPowerMultiplier;
        public static StatDef BANW_HealReceivedMultiplier;
        public static StatDef BANW_AffinityBonus_Explosion;
        public static StatDef BANW_AffinityBonus_Mysterious;
        public static StatDef BANW_AffinityBonus_Vibration;
        public static StatDef BANW_AffinityBonus_Through;
        public static StatDef BANW_AffinityBonus_Composite;
        public static StatDef BANW_HealthScaleOffset;
        public static StatDef BANW_HealthScalePercentOffset;
        public static StatDef BANW_ExSkillMultiplier;

        static BattleStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BattleStatDefOf));
        }
    }
}
