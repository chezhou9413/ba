using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
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

        static BattleStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BattleStatDefOf));
        }
    }
}
