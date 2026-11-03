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
        private Verb interruptedWeaponVerb;
        private LocalTargetInfo interruptedWeaponTarget = LocalTargetInfo.Invalid;
        private int weaponReadyTick = -1;
        private Hediff_SpecialSkillState State => pawn.health.hediffSet.hediffs
            .OfType<Hediff_SpecialSkillState>().FirstOrDefault(state => state.shiroko.normalJob == job);

        //自动远程技能无需预约目标。
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        //保存被技能打断的武器瞄准，读档后仍按原就绪时间恢复。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref interruptedWeaponVerb, "interruptedWeaponVerb");
            Scribe_TargetInfo.Look(ref interruptedWeaponTarget, "interruptedWeaponTarget");
            Scribe_Values.Look(ref weaponReadyTick, "weaponReadyTick", -1);
        }

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
                if (pawn.stances?.curStance is MultiShotAimStance) pawn.stances.SetStance(new Stance_Mobile());
            });
            Toil warmup = Toils_General.Wait(skill.warmupTicks, TargetIndex.A);
            //原武器可能仍在瞄准，由本工作接管，不能先等待它结束。
            warmup.atomicWithPrevious = true;
            if (skill.warmupTicks == 0) warmup.defaultCompleteMode = ToilCompleteMode.Instant;
            warmup.handlingFacing = true;
            warmup.AddPreInitAction(() =>
            {
                if (pawn.stances.curStance is Stance_Warmup weaponWarmup &&
                    weaponWarmup.verb?.verbProps.Ranged == true &&
                    weaponWarmup.verb.EquipmentSource == pawn.equipment?.Primary)
                {
                    interruptedWeaponVerb = weaponWarmup.verb;
                    interruptedWeaponTarget = weaponWarmup.focusTarg;
                    weaponReadyTick = state.Now + weaponWarmup.ticksLeft;
                }
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
            //前摇到期直接投掷并归还原工作，后续连射由地图队列独立完成。
            fire.atomicWithPrevious = true;
            fire.initAction = () =>
            {
                ShirokoNormalSkill.Fire(state, job.targetA.Pawn);
                //同一刻恢复排队工作或自动开火，避免先进入不能远程射击的姿态等待工作。
                pawn.jobs.EndCurrentJob(JobCondition.Succeeded, startNewJob: false);
                pawn.jobs.CheckForJobOverride(ignoreQueue: false);
                ResumeWeaponWarmup(state.Now);
            };
            yield return fire;
        }

        //同一武器继续瞄准原目标时，技能前摇也计入已等待时间，不重新计算整段瞄准。
        private void ResumeWeaponWarmup(int now)
        {
            if (weaponReadyTick < 0 || !(pawn.stances.curStance is Stance_Warmup warmup) ||
                warmup.verb != interruptedWeaponVerb || warmup.focusTarg != interruptedWeaponTarget) return;
            warmup.ticksLeft = System.Math.Min(warmup.ticksLeft, System.Math.Max(1, weaponReadyTick - now));
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
