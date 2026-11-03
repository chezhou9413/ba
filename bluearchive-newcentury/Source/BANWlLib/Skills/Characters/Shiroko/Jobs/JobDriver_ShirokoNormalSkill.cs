using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //EX普通技能前摇与连射锁定，末发射出后归还白子的原工作。
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

        //前摇持续跟随目标，投掷后保持普攻锁定直到无人机末发射出。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var state = State;
            if (state == null) { Log.Error("白子普通技能工作缺少角色状态"); yield break; }
            var skill = state.profile.shiroko.normalSkill;
            this.FailOn(() => !SpecialCombatUtility.CanAct(pawn) ||
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
            warmup.FailOn(() => !state.Active);
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
            //前摇到期投掷，紧接着进入连射等待，不在两阶段之间释放普攻。
            fire.atomicWithPrevious = true;
            fire.FailOn(() => !state.Active);
            fire.initAction = () =>
            {
                ShirokoNormalSkill.Fire(state, job.targetA.Pawn);
                effects?.Cleanup(true);
                effects = null;
            };
            yield return fire;
            Toil channel = ToilMaker.MakeToil("白子无人机连射锁定普攻");
            channel.defaultCompleteMode = ToilCompleteMode.Never;
            channel.atomicWithPrevious = true;
            channel.handlingFacing = true;
            channel.initAction = () => pawn.stances.SetStance(new MultiShotAimStance(
                System.Math.Max(1, state.shiroko.normalEndTick - state.Now + 2), job.targetA));
            channel.tickAction = () =>
            {
                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(job.targetA);
                //末发所在tick仍然锁定，避免角色更新早于地图发射队列时提前开枪。
                if (state.Now <= state.shiroko.normalEndTick) return;
                pawn.jobs.EndCurrentJob(JobCondition.Succeeded, startNewJob: false);
                pawn.jobs.CheckForJobOverride(ignoreQueue: false);
                ResumeWeaponWarmup(state.Now);
            };
            yield return channel;
        }

        //同一武器继续瞄准原目标时，技能前摇和连射等待都计入瞄准进度。
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
