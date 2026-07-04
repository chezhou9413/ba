using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // BA战斗属性引用，负责让代码安全访问 XML 中定义的战斗 StatDef。
    [DefOf]
    public static class BattleStatDefOf
    {
        public static StatDef BANW_InitialHealth;
        public static StatDef BANW_InitialHeal;
        public static StatDef BANW_AttackLevelMultiplier;
        public static StatDef BANW_FinalDamageMultiplier;
        public static StatDef BANW_NormalAttackMultiplier;
        public static StatDef BANW_BaseMasteryMultiplier;
        public static StatDef BANW_CriticalChance;
        public static StatDef BANW_CriticalDamage;
        public static StatDef BANW_CriticalChanceResistance;
        public static StatDef BANW_CriticalDamageResistance;
        public static StatDef BANW_HealLevelMultiplier;
        public static StatDef BANW_HealFlatBonus;
        public static StatDef BANW_HealBonusMultiplier;
        public static StatDef BANW_HealReceivedMultiplier;
        public static StatDef BANW_AffinityBonus_Explosion;
        public static StatDef BANW_AffinityBonus_Mysterious;
        public static StatDef BANW_AffinityBonus_Vibration;
        public static StatDef BANW_AffinityBonus_Through;
        public static StatDef BANW_AffinityBonus_Composite;
        public static StatDef BANW_HealthLevelMultiplier;
        public static StatDef BANW_HealthFlatBonus;
        public static StatDef BANW_HealthBonusMultiplier;
        public static StatDef BANW_ExSkillMultiplier;

        static BattleStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BattleStatDefOf));
        }
    }
}
