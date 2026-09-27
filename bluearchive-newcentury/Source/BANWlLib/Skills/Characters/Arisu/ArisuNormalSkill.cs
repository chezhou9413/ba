using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //爱丽丝普通技能执行器，负责按有效普攻命中自动触发独立声音、特效和攻击。
    public static class ArisuNormalSkill
    {
        //条件满足才消费命中次数，普通技能不读取或消费EX充能阶段。
        public static bool TryCast(Hediff_SpecialSkillState state)
        {
            ArisuNormalSkillDef skill = state.profile.arisu.normalSkill;
            if (!state.native || state.hits < skill.requiredHits || state.Now < state.nextNormalTick ||
                state.castEndTick >= state.Now || !SpecialCombatUtility.CanAct(state.pawn)) return false;
            Pawn target = SpecialCombatUtility.FindEnemy(state.pawn, skill.range);
            if (target == null) return false;
            state.hits -= skill.requiredHits;
            state.nextNormalTick = state.Now + skill.cooldownTicks;
            skill.castSound?.PlayOneShot(new TargetInfo(state.pawn));
            SpecialDirectionalEffects.Trigger(skill.casterEffecter, state.pawn, target);
            SpecialDirectionalEffects.Trigger(skill.targetEffecter, target, target);
            SpecialCombatUtility.Schedule(state, target, skill.attack);
            return true;
        }
    }
}
