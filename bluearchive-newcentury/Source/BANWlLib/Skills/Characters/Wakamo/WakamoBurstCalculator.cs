using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Skills
{
    //若藻蓄积换算器，负责将每点累计伤害按若藻BA属性换算为待释放伤害。
    public static class WakamoBurstCalculator
    {
        //标记建立时锁定单位伤害的BA结算，界面读取和最终释放不重复掷暴击。
        public static BattleDamageResult CalculateUnit(Hediff_SpecialSkillState state, Pawn target, BattleCasterSnapshot snapshot)
        {
            var attack = state.profile.burstAttack;
            return BattleStatUtility.BuildDamageResult(new BattleDamageRequest
            {
                instigator = state.pawn, target = target, snapshot = snapshot,
                damageDef = attack.damageDef, penetration = attack.penetration,
                baseDamageOverride = 1f, attackPowerRatio = attack.attackPowerRatio,
                useBattleStats = attack.useBattleStats, isExSkill = attack.isExSkill,
                canCrit = attack.canCrit, alwaysCrit = attack.alwaysCrit, applyAffinity = attack.applyAffinity
            });
        }
    }
}
