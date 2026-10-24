using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //星野与若藻连射期间保持停步瞄准，死亡目标由尸体和最后位置接续。
    public abstract class JobDriver_SpecialBurstChannel : JobDriver_CastAbility
    {
        protected abstract SpecialSkillRole Role { get; }

        //开始前终止武器连射，避免技能前摇混入普攻。
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            foreach (Verb verb in pawn.equipment.Primary?.GetComp<CompEquippable>()?.AllVerbs ?? Enumerable.Empty<Verb>())
                verb.Reset();
            job.targetC = job.targetA.Cell;
        }

        //沿用原版前摇、进度与冷却，目标有效性仅限制发动前阶段。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var state = Hediff_SpecialSkillState.Find(pawn, Role);
            Map castMap = pawn.Map;
            var attacks = castMap.GetComponent<MapComponent_SpecialSkills>();
            this.FailOn(() => !SpecialCombatUtility.CanAct(pawn));
            AddFinishAction(condition =>
            {
                FinishBurst(state, condition);
                attacks.CancelCast(job);
                state.castEndTick = -1;
                if (pawn.stances.curStance is MultiShotAimStance) pawn.stances.SetStance(new Stance_Mobile());
                if (job.def.abilityCasting)
                    job.ability.StartCooldown(job.ability.def.cooldownTicksRange.RandomInRange);
            });
            Toil prepare = ToilMaker.MakeToil("连射技能准备");
            prepare.defaultCompleteMode = ToilCompleteMode.Instant;
            prepare.initAction = () => pawn.pather.StopDead();
            prepare.FailOn(() => (!job.ability.CanCast && !job.ability.Casting) ||
                !job.ability.CompOfType<CompAbilityEffect_SpecialSkill>().Valid(job.targetA));
            yield return prepare;
            Toil cast = Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
            if (job.ability.def.showCastingProgressBar)
                cast.WithProgressBar(TargetIndex.A, () => job.verbToUse.WarmupProgress);
            yield return cast;
            Toil channel = ToilMaker.MakeToil("连射技能持续瞄准");
            channel.defaultCompleteMode = ToilCompleteMode.Never;
            channel.handlingFacing = true;
            channel.initAction = () => pawn.stances.SetStance(new MultiShotAimStance(
                System.Math.Max(1, state.castEndTick - state.Now + 2), job.targetA));
            channel.tickAction = () =>
            {
                pawn.pather.StopDead();
                IntVec3 cell = job.targetC.Cell;
                LocalTargetInfo focus = SpecialBurstTarget.Resolve(job.targetA.Thing, castMap, ref cell);
                job.targetC = cell;
                pawn.rotationTracker.FaceTarget(focus);
                if (pawn.stances.curStance is MultiShotAimStance stance) stance.focusTarg = focus;
                if (!attacks.HasPendingCast(job)) ReadyForNextToil();
            };
            yield return channel;
        }

        //角色专属完成效果由对应工作处理。
        protected virtual void FinishBurst(Hediff_SpecialSkillState state, JobCondition condition) { }
    }
}
