using System.Linq;
using BANWlLib.BattleSystem;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //白子EX普通技能的自动索敌、前摇入口和同轮发射。
    public static class ShirokoNormalSkill
    {
        //只在EX期间启动一个前摇，缺少目标时保留就绪机会。
        public static bool TryCast(Hediff_SpecialSkillState state)
        {
            if (!state.native || !state.Active || state.Now < state.nextNormalTick ||
                state.shiroko.normalJob != null || state.shiroko.normalEndTick >= state.Now ||
                !SpecialCombatUtility.CanAct(state.pawn) || state.pawn.CurJob?.ability != null ||
                !state.pawn.jobs.IsCurrentJobPlayerInterruptible()) return false;
            var weapon = state.pawn.equipment?.Primary?.GetComp<CompEquippable>();
            if (weapon?.AllVerbs.Any(verb => verb.Bursting) == true) return false;
            ShirokoNormalSkillDef skill = state.profile.shiroko.normalSkill;
            Pawn target = null;
            float nearest = float.MaxValue;
            foreach (Pawn candidate in state.pawn.Map.mapPawns.AllPawnsSpawned)
            {
                float distance = candidate.Position.DistanceToSquared(state.pawn.Position);
                if (distance >= nearest || !SpecialCombatUtility.ValidEnemy(state.pawn, candidate, skill.range)) continue;
                nearest = distance;
                target = candidate;
            }
            if (target == null) return false;
            Job job = JobMaker.MakeJob(skill.jobDef, target);
            state.shiroko.normalJob = job;
            state.shiroko.warmupStartTick = -1;
            state.pawn.jobs.StartJob(job, JobCondition.InterruptForced, resumeCurJobAfterwards: true);
            if (state.pawn.CurJob == job) return true;
            state.shiroko.normalJob = null;
            return false;
        }

        //手榴弹与所有无人机弹使用同一份发射快照，各发倍率独立。
        public static void Fire(Hediff_SpecialSkillState state, Pawn target)
        {
            ShirokoNormalSkillDef skill = state.profile.shiroko.normalSkill;
            var snapshot = BattleStatUtility.CreateSnapshot(state.pawn);
            SpecialCombatUtility.Schedule(state, target, skill.grenadeAttack,
                retainTargetPosition: true, casterSnapshot: snapshot);
            int lastShot = 1;
            foreach (ShirokoDroneShot shot in skill.droneShots)
            {
                SpecialCombatUtility.Schedule(state, target, shot.attack, drone: true,
                    extraDelay: shot.delayTicks, casterSnapshot: snapshot);
                lastShot = Mathf.Max(lastShot, 1 + shot.delayTicks);
            }
            state.shiroko.normalEndTick = state.Now + lastShot;
            state.castEndTick = Mathf.Max(state.castEndTick, state.shiroko.normalEndTick);
            state.nextNormalTick = state.Now + 1 + skill.cooldownTicks;
        }

        //只中断本角色的前摇，已经飞出的弹丸继续交给地图结算。
        public static void CancelWarmup(Hediff_SpecialSkillState state)
        {
            Job job = state.shiroko.normalJob;
            if (job != null && state.pawn.CurJob == job)
                state.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            state.shiroko.normalJob = null;
            state.shiroko.warmupStartTick = -1;
        }
    }
}
