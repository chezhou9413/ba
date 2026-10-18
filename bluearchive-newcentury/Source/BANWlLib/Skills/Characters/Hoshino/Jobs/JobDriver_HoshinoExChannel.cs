using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //星野输出EX保持整轮停步瞄准，敌军和友军共用同一目标跟踪。
    public class JobDriver_HoshinoExChannel : JobDriver_CastAbility
    {
        //停止此前的武器连射，避免EX前摇期间混入普通攻击。
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            foreach (Verb verb in pawn.equipment.Primary?.GetComp<CompEquippable>()?.AllVerbs ?? Enumerable.Empty<Verb>())
                verb.Reset();
        }

        //先执行原版前摇和声音，再维持瞄准直到最后一发发射。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var state = Hediff_SpecialSkillState.Find(pawn, SpecialSkillRole.Hoshino);
            var effect = job.ability.CompOfType<CompAbilityEffect_SpecialSkill>();
            Map castMap = pawn.Map;
            this.FailOn(() => !SpecialCombatUtility.CanAct(pawn) || !effect.Valid(job.targetA));
            AddFinishAction(condition =>
            {
                castMap.GetComponent<MapComponent_SpecialSkills>().CancelCast(job);
                if (condition == JobCondition.Succeeded && state.hoshino.grantAfterCast && state.stage == 0)
                    state.hoshino.empoweredReady = true;
                state.castEndTick = -1;
                state.hoshino.grantAfterCast = state.hoshino.outputExCasting = false;
                if (pawn.stances.curStance is MultiShotAimStance) pawn.stances.SetStance(new Stance_Mobile());
            });
            foreach (Toil toil in base.MakeNewToils()) yield return toil;
            Toil channel = ToilMaker.MakeToil("星野EX持续瞄准");
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
