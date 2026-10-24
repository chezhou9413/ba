using System.Linq;
using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //爱丽丝普通技能执行器，按有效普攻命中自动触发独立声音、特效和攻击。
    public static class ArisuNormalSkill
    {
        //触发后先进入独立前摇，只有真正发射才消费有效命中次数。
        public static bool TryCast(Hediff_SpecialSkillState state)
        {
            ArisuNormalSkillDef skill = state.profile.arisu.normalSkill;
            if (!state.native || state.hits < skill.requiredHits || state.Now < state.nextNormalTick ||
                state.castEndTick >= state.Now || state.arisuNormalJob != null ||
                state.pawn.pather.MovingNow ||
                state.pawn.CurJob?.ability != null || !state.pawn.jobs.IsCurrentJobPlayerInterruptible() ||
                !SpecialCombatUtility.CanAct(state.pawn)) return false;
            var weapon = state.pawn.equipment.Primary?.GetComp<CompEquippable>();
            if (weapon?.AllVerbs.Any(v => v.Bursting) == true) return false;
            Pawn target = SpecialCombatUtility.FindEnemy(state.pawn, skill.range);
            if (target == null) return false;
            Job job = JobMaker.MakeJob(skill.jobDef, target);
            state.arisuNormalJob = job;
            state.pawn.jobs.StartJob(job, JobCondition.InterruptForced, resumeCurJobAfterwards: true);
            if (state.pawn.CurJob == job) return true;
            state.arisuNormalJob = null;
            return false;
        }

        //前摇完成后发动普通技能，EX倍率标记不改变技能的自动触发身份。
        public static void Fire(Hediff_SpecialSkillState state, Pawn target)
        {
            ArisuNormalSkillDef skill = state.profile.arisu.normalSkill;
            state.hits -= skill.requiredHits;
            state.nextNormalTick = state.Now + skill.cooldownTicks;
            SpecialDirectionalEffects.Trigger(skill.targetEffecter, target, target);
            SpecialCombatUtility.Schedule(state, target, skill.attack);
            state.castEndTick = state.Now + skill.attack.ShotDelay(skill.attack.shots - 1);
        }
    }
}
