using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using Verse;
using Verse.AI;

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

        //自身充能沿用瞬时施法，攻击则维持工作并在中断时取消剩余伤害。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (job.targetA.Thing == pawn)
            {
                foreach (Toil toil in base.MakeNewToils()) yield return toil;
                yield break;
            }
            var profile = job.ability.CompOfType<CompAbilityEffect_SpecialSkill>().Props.profile;
            var state = Hediff_SpecialSkillState.Find(pawn, profile);
            Map castMap = pawn.Map;
            this.FailOn(() => !SpecialCombatUtility.CanAct(pawn));
            AddFinishAction(condition =>
            {
                castMap.GetComponent<MapComponent_SpecialSkills>().CancelCast(job);
                state.castEndTick = -1;
                if (pawn.stances.curStance is MultiShotAimStance) pawn.stances.CancelBusyStanceSoft();
            });
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
                if (!castMap.GetComponent<MapComponent_SpecialSkills>().HasPendingCast(job)) ReadyForNextToil();
            };
            yield return channel;
        }
    }
}
