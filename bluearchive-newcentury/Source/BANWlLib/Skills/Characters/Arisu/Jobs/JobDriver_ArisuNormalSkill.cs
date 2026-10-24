using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //爱丽丝普通技能的前摇和瞄准工作，伤害发射后独立完成。
    public class JobDriver_ArisuNormalSkill : JobDriver
    {
        //自动技能使用角色自己的运行状态，不创建额外能力按钮。
        private Hediff_SpecialSkillState State => pawn.health.hediffSet.hediffs
            .OfType<Hediff_SpecialSkillState>().FirstOrDefault(s => s.arisuNormalJob == job);

        //远程技能无需预约敌方角色。
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        //被打断的前摇保留触发次数，已发射的技能继续完成剩余弹丸。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var state = State;
            if (state == null) { Log.Error("爱丽丝普通技能工作缺少角色状态"); yield break; }
            var skill = state.profile.arisu.normalSkill;
            this.FailOn(() => !SpecialCombatUtility.CanAct(pawn));
            AddFinishAction(condition =>
            {
                if (state.arisuNormalJob == job) state.arisuNormalJob = null;
                if (pawn.stances.curStance is MultiShotAimStance) pawn.stances.CancelBusyStanceSoft();
            });
            Toil warmup = Toils_General.Wait(skill.warmupTicks, TargetIndex.A);
            if (skill.warmupTicks == 0) warmup.defaultCompleteMode = ToilCompleteMode.Instant;
            warmup.FailOn(() => !SpecialCombatUtility.ValidEnemy(pawn, job.targetA.Pawn, skill.range));
            warmup.handlingFacing = true;
            warmup.AddPreInitAction(() =>
            {
                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(job.targetA);
                pawn.stances.SetStance(new MultiShotAimStance(skill.warmupTicks + 1, job.targetA));
                skill.castSound?.PlayOneShot(new TargetInfo(pawn));
                SpecialDirectionalEffects.Trigger(skill.casterEffecter, pawn, job.targetA, skill.warmupTicks);
            });
            warmup.AddPreTickAction(() => pawn.rotationTracker.FaceTarget(job.targetA));
            yield return warmup;
            Toil fire = ToilMaker.MakeToil("爱丽丝普通技能发射");
            fire.defaultCompleteMode = ToilCompleteMode.Instant;
            fire.FailOn(() => !SpecialCombatUtility.ValidEnemy(pawn, job.targetA.Pawn, skill.range));
            fire.initAction = () => ArisuNormalSkill.Fire(state, job.targetA.Pawn);
            yield return fire;
            Toil channel = ToilMaker.MakeToil("爱丽丝普通技能持续瞄准");
            channel.defaultCompleteMode = ToilCompleteMode.Never;
            channel.handlingFacing = true;
            channel.initAction = () => pawn.stances.SetStance(
                new MultiShotAimStance(System.Math.Max(1, state.castEndTick - state.Now + 2), job.targetA));
            channel.tickAction = () =>
            {
                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(job.targetA);
                if (state.Now > state.castEndTick) ReadyForNextToil();
            };
            yield return channel;
        }
    }
}
