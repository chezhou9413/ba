using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using BANWlLib.BaVerb;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //爱丽丝EX准备期追踪目标，出束后锁定方向直到最后一段伤害完成。
    public class JobDriver_ArisuExChannel : JobDriver_CastAbility
    {
        private int attackStartTick = -1;
        private int attackStage;
        private bool beamFired;
        private IntVec3 aimCell = IntVec3.Invalid;
        private ArisuExEffects attackEffects;

        //能力实际生效后启动准备表现，伤害等到光束出射时再安排。
        public void BeginAttack(int stage)
        {
            attackStartTick = Find.TickManager.TicksGame;
            attackStage = stage;
            aimCell = job.targetA.Cell;
            var config = job.ability.CompOfType<CompAbilityEffect_SpecialSkill>().Props.profile.arisu;
            config.attackSound?.PlayOneShot(new TargetInfo(pawn));
            attackEffects = new ArisuExEffects(config.attackEffecters[stage], pawn, attackStartTick);
            attackEffects.Tick(job.targetA);
        }

        //自身充能期间禁止玩家指令打断，对敌光束仍允许移动。
        public override bool PlayerInterruptable => job.targetA.Thing != pawn && base.PlayerInterruptable;

        //开始EX时停止主武器连射，避免引导期间继续普通攻击。
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            foreach (Verb verb in pawn.equipment.Primary?.GetComp<CompEquippable>()?.AllVerbs ?? Enumerable.Empty<Verb>())
                verb.Reset();
        }

        //充能先播放前摇；光束发动后独立结算，工作只负责持续瞄准。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var profile = job.ability.CompOfType<CompAbilityEffect_SpecialSkill>().Props.profile;
            var state = Hediff_SpecialSkillState.Find(pawn, profile);
            this.FailOn(() => !SpecialCombatUtility.CanAct(pawn));
            AddFinishAction(condition =>
            {
                attackEffects?.Cleanup(cancelMotes: !beamFired);
                attackEffects = null;
                if (attackStartTick >= 0 && !beamFired) state.castEndTick = -1;
                if (pawn.stances.curStance is MultiShotAimStance) pawn.stances.CancelBusyStanceSoft();
            });
            if (job.targetA.Thing == pawn)
            {
                Toil charge = Toils_General.Wait(profile.arisu.chargeWarmupTicks);
                charge.FailOn(() => state.stage >= 2);
                if (profile.arisu.chargeWarmupTicks == 0) charge.defaultCompleteMode = ToilCompleteMode.Instant;
                charge.AddPreInitAction(() =>
                {
                    pawn.pather.StopDead();
                    profile.arisu.chargeSound?.PlayOneShot(new TargetInfo(pawn));
                    SpecialDirectionalEffects.Trigger(profile.arisu.chargeEffecters[state.stage], pawn, pawn,
                        profile.arisu.chargeWarmupTicks);
                });
                yield return charge;
                foreach (Toil toil in base.MakeNewToils()) yield return toil;
                yield break;
            }
            foreach (Toil toil in base.MakeNewToils()) yield return toil;
            Toil channel = ToilMaker.MakeToil("爱丽丝EX持续施法");
            channel.defaultCompleteMode = ToilCompleteMode.Never;
            channel.handlingFacing = true;
            channel.initAction = () =>
            {
                pawn.pather.StopDead();
                pawn.stances.SetStance(new MultiShotAimStance(System.Math.Max(1, state.castEndTick - state.Now + 2), job.targetA));
            };
            channel.tickAction = () =>
            {
                pawn.pather.StopDead();
                if (!beamFired)
                {
                    LocalTargetInfo focus = SpecialBurstTarget.Resolve(job.targetA.Thing, pawn.Map, ref aimCell);
                    if (pawn.stances.curStance is MultiShotAimStance aim) aim.focusTarg = focus;
                    if (state.Now >= attackStartTick + profile.arisu.damageDelayTicks)
                    {
                        beamFired = true;
                        job.targetA = new LocalTargetInfo(aimCell);
                        if (pawn.stances.curStance is MultiShotAimStance lockedAim) lockedAim.focusTarg = job.targetA;
                        var cells = BattleTargetPreviewUtility.CalculateLineCells(pawn, job.targetA,
                            profile.arisu.lineLength, profile.arisu.lineWidth).ToList();
                        SpecialCombatUtility.Schedule(state, null, profile.exAttack, 1f + attackStage,
                            center: pawn.Position, areaCells: cells);
                    }
                }
                pawn.rotationTracker.FaceTarget(job.targetA);
                if (attackEffects == null && attackStartTick >= 0)
                    attackEffects = new ArisuExEffects(profile.arisu.attackEffecters[attackStage], pawn, attackStartTick);
                attackEffects?.Tick(beamFired ? job.targetA : new LocalTargetInfo(aimCell));
                if (state.Now > state.castEndTick) ReadyForNextToil();
            };
            yield return channel;
        }

        //保存准备期与出束状态，读档不会重新消费充能或重复安排光束伤害。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref attackStartTick, "arisuAttackStartTick", -1);
            Scribe_Values.Look(ref attackStage, "arisuAttackStage");
            Scribe_Values.Look(ref beamFired, "arisuBeamFired");
            Scribe_Values.Look(ref aimCell, "arisuAimCell", IntVec3.Invalid);
        }
    }
}
