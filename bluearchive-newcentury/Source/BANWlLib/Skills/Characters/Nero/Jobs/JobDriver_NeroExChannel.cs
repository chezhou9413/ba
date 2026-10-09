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
        private Effecter channelEffect;
        private int channelStartTick = -1;

        //提供本次生效时刻，让读档后的延迟特效继续使用原始相对时间。
        public int ChannelStartTick => channelStartTick;

        //从实际发动入口接管施法特效，持续时间和清理由本工作负责。
        public void BeginChannelEffect(EffecterDef effectDef)
        {
            if (effectDef == null) return;
            if (channelStartTick < 0) channelStartTick = Find.TickManager.TicksGame;
            channelEffect?.Cleanup();
            channelEffect = effectDef.Spawn();
            channelEffect.Trigger(pawn, job.targetA.ToTargetInfo(pawn.Map));
        }

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
            var effect = job.ability.CompOfType<CompAbilityEffect_SpecialSkill>();
            Map castMap = pawn.Map;
            this.FailOn(() => !SpecialCombatUtility.CanAct(pawn) ||
                !effect.Valid(job.targetA));
            AddFinishAction(condition =>
            {
                channelEffect?.Cleanup();
                channelEffect = null;
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
                //视觉实例不进入存档，读档时按工作保存的生效时刻重建。
                if (channelEffect == null && channelStartTick >= 0)
                    BeginChannelEffect(state.profile.casterEffecter);
                channelEffect?.EffectTick(pawn, job.targetA.ToTargetInfo(pawn.Map));
                if (!castMap.GetComponent<MapComponent_SpecialSkills>().HasPendingCast(job)) ReadyForNextToil();
            };
            yield return channel;
        }

        //保存引导特效的起始时刻，原版工作负责保存目标和施法进度。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref channelStartTick, "channelEffectStartTick", -1);
        }
    }
}
