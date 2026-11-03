using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //EX普通技能瞄准前摇，投掷完成后归还白子的原工作。
    public class JobDriver_ShirokoNormalSkill : JobDriver
    {
        private AimedSkillEffects effects;
        private Hediff_SpecialSkillState State => pawn.health.hediffSet.hediffs
            .OfType<Hediff_SpecialSkillState>().FirstOrDefault(state => state.shiroko.normalJob == job);

        //自动远程技能无需预约目标。
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        //前摇持续跟随目标，到期或中断时清理姿态和特效。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var state = State;
            if (state == null) { Log.Error("白子普通技能工作缺少角色状态"); yield break; }
            var skill = state.profile.shiroko.normalSkill;
            this.FailOn(() => !state.Active || !SpecialCombatUtility.CanAct(pawn) ||
                !SpecialCombatUtility.ValidEnemy(pawn, job.targetA.Pawn, skill.range));
            AddFinishAction(condition =>
            {
                effects?.Cleanup(true);
                effects = null;
                if (state.shiroko.normalJob == job)
                {
                    state.shiroko.normalJob = null;
                    state.shiroko.warmupStartTick = -1;
                }
                if (pawn.stances?.curStance is MultiShotAimStance) pawn.stances.CancelBusyStanceSoft();
            });
            Toil warmup = Toils_General.Wait(skill.warmupTicks, TargetIndex.A);
            if (skill.warmupTicks == 0) warmup.defaultCompleteMode = ToilCompleteMode.Instant;
            warmup.handlingFacing = true;
            warmup.AddPreInitAction(() =>
            {
                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(job.targetA);
                pawn.stances.SetStance(new MultiShotAimStance(skill.warmupTicks + 1, job.targetA));
                state.shiroko.warmupStartTick = state.Now;
                skill.warmupSound?.PlayOneShot(new TargetInfo(pawn));
                UpdateEffects(state);
            });
            warmup.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceTarget(job.targetA);
                UpdateEffects(state);
            });
            yield return warmup;
            Toil fire = ToilMaker.MakeToil("白子手榴弹与无人机发射");
            fire.defaultCompleteMode = ToilCompleteMode.Instant;
            fire.initAction = () => ShirokoNormalSkill.Fire(state, job.targetA.Pawn);
            yield return fire;
        }

        //读档后按保存的起始时间重建表现，不重播前摇音效。
        private void UpdateEffects(Hediff_SpecialSkillState state)
        {
            if (effects == null) effects = new AimedSkillEffects(state.profile.shiroko.normalSkill.warmupEffecter,
                pawn, state.shiroko.warmupStartTick);
            effects.Tick(job.targetA);
        }
    }
}
