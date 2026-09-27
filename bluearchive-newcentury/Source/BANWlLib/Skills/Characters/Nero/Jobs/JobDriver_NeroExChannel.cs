using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using RimWorld;
using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //妮露二段EX施法工作，负责整轮停步瞄准与中断时取消剩余射击。
    public class JobDriver_NeroExChannel : JobDriver_CastAbility
    {
        //开始技能前停止武器正在进行的连射，保留原版能力前摇和通知链路。
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            foreach (Verb verb in pawn.equipment.Primary?.GetComp<CompEquippable>()?.AllVerbs ?? Enumerable.Empty<Verb>())
                verb.Reset();
        }

        //执行原版施法，再保持本次任务直到最后一发发射或施法被中断。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var state = Hediff_SpecialSkillState.Find(pawn, SpecialSkillRole.Nero);
            Map castMap = pawn.Map;
            this.FailOn(() => !SpecialCombatUtility.CanAct(pawn) ||
                !SpecialCombatUtility.ValidEnemy(pawn, job.targetA.Pawn, state.profile.range));
            AddFinishAction(condition =>
            {
                castMap.GetComponent<MapComponent_SpecialSkills>().CancelCast(job);
                state.castEndTick = -1;
                if (pawn.stances.curStance is MultiShotAimStance) pawn.stances.CancelBusyStanceSoft();
            });
            foreach (Toil toil in base.MakeNewToils()) yield return toil;
            Toil channel = ToilMaker.MakeToil("妮露二段EX持续施法");
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
