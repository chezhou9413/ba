using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Skills
{
    //支援动作职责：执行特殊技能中的治疗、护盾和附加状态，避免重复播放命中特效。
    public static class SpecialSupportActionUtility
    {
        //执行职责：分摊治疗倍率，护盾按每次授予值结算，限时增益刷新到期时间。
        public static void Apply(SpecialPendingAttack attack, Thing target)
        {
            SpecialAttackConfig source = attack.action;
            if (!source.isHealing && !source.isShield && source.triggerHediff == null) return;
            BattleActionConfig action = source.Copy();
            action.damageDef = null;
            action.effecterDef = null;
            action.healPowerRatio *= attack.multiplier;
            //护盾使用覆盖规则，连射发数不应把每次授予的护盾值重复缩小。
            action.shieldPowerRatio *= attack.multiplier * source.shots;
            bool timed = source.triggerHediff != null && typeof(TimedSkillBuff).IsAssignableFrom(source.triggerHediff.hediffClass);
            if (timed) action.triggerHediff = null;
            BattleStatUtility.ApplyAction(attack.caster, target, action, attack.snapshot);
            if (timed && target is Pawn pawn)
            {
                BattleHediffSnapshotUtility.RegisterSnapshotIfNeeded(pawn, source.triggerHediff, attack.snapshot);
                TimedSkillBuff.Apply(pawn, source.triggerHediff, source.buffDurationTicks);
            }
        }
    }
}
