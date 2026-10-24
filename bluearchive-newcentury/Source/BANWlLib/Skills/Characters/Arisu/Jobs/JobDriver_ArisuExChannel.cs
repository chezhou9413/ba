using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //爱丽丝EX施法工作，保持直线瞄准直到最后一段伤害完成。
    public class JobDriver_ArisuExChannel : JobDriver_CastAbility
    {
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
                pawn.rotationTracker.FaceTarget(job.targetA);
                if (state.Now > state.castEndTick) ReadyForNextToil();
            };
            yield return channel;
        }
    }
}
