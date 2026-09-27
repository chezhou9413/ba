using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //凯伊普通技能入口，负责在场地结束后寻找目标并启动自动施法工作。
    public static class KeiNormalSkill
    {
        //在可行动且未执行其他技能时启动前摇，目标缺失时保留蓄积。
        public static bool TryStart(Hediff_SpecialSkillState state)
        {
            Pawn pawn = state.pawn;
            if (!state.releaseReady || !SpecialCombatUtility.CanAct(pawn) || state.keiNormalJob != null ||
                pawn.CurJob?.ability != null || pawn.CurJob?.def == state.profile.kei.normalJob ||
                !pawn.jobs.IsCurrentJobPlayerInterruptible()) return false;
            Pawn target = SpecialCombatUtility.FindEnemy(pawn, state.profile.range);
            if (target == null) return false;
            //结束武器正在进行的连射，防止普通子弹与技能前摇同时执行。
            foreach (Verb verb in pawn.equipment.Primary?.GetComp<CompEquippable>()?.AllVerbs ?? Enumerable.Empty<Verb>())
                verb.Reset();
            Job job = JobMaker.MakeJob(state.profile.kei.normalJob, target);
            state.keiNormalJob = job;
            pawn.jobs.StartJob(job, JobCondition.InterruptForced, resumeCurJobAfterwards: true);
            return pawn.CurJob == job;
        }
    }
}
