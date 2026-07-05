using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BANWlLib.BattleSystem
{
    //延迟连射施法任务，负责按原版 Ability Job 链路锁定 pawn 到连射队列结束。
    public class JobDriver_MultiShotProjectileCasting : JobDriver_CastVerbOnce
    {
        //生成施法步骤，负责先执行原版技能前摇和 Verb，再等待后台延迟子弹全部发完。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !job.ability.CanCast && !job.ability.Casting);
            AddFinishAction(delegate
            {
                if (job.ability != null && job.def.abilityCasting)
                {
                    job.ability.StartCooldown(job.ability.def.cooldownTicksRange.RandomInRange);
                }
            });

            Toil stopBeforeCast = ToilMaker.MakeToil("BANW_MultiShot_StopBeforeCast");
            stopBeforeCast.initAction = delegate
            {
                pawn.pather.StopDead();
            };
            stopBeforeCast.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return stopBeforeCast;

            yield return Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
            yield return WaitForMultiShotProjectiles();
        }

        //开始任务，负责通知 Ability 进入原版施法状态。
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            job.ability?.Notify_StartedCasting();
        }

        //显示任务报告，负责复用原版技能施法文本和剩余前摇时间。
        public override string GetReport()
        {
            string text = "";
            if (job.ability == null || job.ability.def.targetRequired)
            {
                text = base.GetReport();
            }
            else
            {
                text = "UsingVerbNoTarget".Translate(job.verbToUse.ReportLabel).ToString();
            }

            if (job.ability != null && job.ability.def.showCastingProgressBar)
            {
                text += " " + "DurationLeft".Translate(job.verbToUse.WarmupTicksLeft.ToStringSecondsFromTicks()) + ".";
            }

            return text;
        }

        //等待延迟连射，负责在技能效果入队后保持当前 Job 不结束。
        private Toil WaitForMultiShotProjectiles()
        {
            Toil toil = ToilMaker.MakeToil("BANW_MultiShot_WaitForProjectiles");
            toil.initAction = delegate
            {
                pawn.pather.StopDead();
            };
            toil.tickAction = delegate
            {
                pawn.pather.StopDead();
                MultiShotProjectileDelayComponent component = pawn.Map?.GetComponent<MultiShotProjectileDelayComponent>();
                if (component == null || !component.HasPendingForCaster(pawn))
                {
                    ReadyForNextToil();
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            return toil;
        }
    }
}
