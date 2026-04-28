using BANWlLib.BaDef;
using BANWlLib.Pojo;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BANWlLib.BaJob
{
    public class Job_SphereAreaAttack : JobDriver
    {
        private List<TickDelayDamageAndHediff> damageSequence;
        private int nextActionIndex;
        private List<LocalTargetInfo> Cells;
        private List<Effecter> activeEffecters = new List<Effecter>();
        private List<PendingDamageAction> pendingActions = new List<PendingDamageAction>();

        private BaJobDef_SphereAreaAttack def => (BaJobDef_SphereAreaAttack)this.job.def;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private void actionDamageSetting(DamageSetting damage, Map map)
        {
            if (damage.effecterDef != null)
            {
                Effecter effecter = damage.effecterDef.Spawn();
                activeEffecters.Add(effecter);
                TargetInfo centerTarget = new TargetInfo(TargetA.Cell, map);
                effecter.Trigger(centerTarget, TargetInfo.Invalid);
            }
            DamageInfo info = new DamageInfo(
                damage.damageType,
                damage.damageAmount,
                damage.penetration,
                -1f,
                pawn,
                null,
                null);

            foreach (LocalTargetInfo target in Cells)
            {
                if (!target.IsValid) continue;
                IntVec3 cell = target.Cell;
                List<Thing> thingsInCell = cell.GetThingList(map);
                for (int i = thingsInCell.Count - 1; i >= 0; i--)
                {
                    Thing t = thingsInCell[i];
                    if (damage.isAttackBuilding && t is Building building)
                    {
                        building.TakeDamage(info);
                    }
                    else if (t is Pawn targetPawn)
                    {
                        // 建议逻辑：不伤害同阵营的人 (包括殖民者自己)
                        if (targetPawn.Faction != null && pawn.Faction != null && !targetPawn.HostileTo(pawn))
                            continue;
                        if(damage.tiggerHediff != null)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(damage.tiggerHediff, targetPawn);
                            targetPawn.health.AddHediff(hediff);
                        }
                        targetPawn.TakeDamage(info);
                    }
                }
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil channelingToil = new Toil();
            channelingToil.handlingFacing = true;
            this.rotateToFace = TargetIndex.A;
            Map map = pawn.Map;

            channelingToil.initAction = () =>
            {
                this.damageSequence = def.damages.OrderBy(d => d.tick).ToList();
                this.nextActionIndex = 0;
                this.pendingActions.Clear();
                this.activeEffecters.Clear(); // 确保重置
                Cells = this.job.targetQueueA;
                pawn.pather.StopDead();
            };

            channelingToil.tickAction = () =>
            {
                int currentToilTick = this.debugTicksSpentThisToil;

                // 【修正2：正确清理特效列表】
                // 使用 RemoveAll 在遍历的同时移除已结束的特效
                activeEffecters.RemoveAll(effecter =>
                {
                    effecter.EffectTick(pawn, this.job.targetA.ToTargetInfo(map));
                    if (effecter.ticksLeft > 300)
                    {
                        effecter.Cleanup();
                        return true; // 返回 true 表示从列表中移除
                    }
                    return false; // 返回 false 表示保留
                });

                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(this.job.targetA);

                if (pawn.stances != null && pawn.equipment?.Primary != null)
                {
                    var verb = pawn.equipment.Primary.GetComp<CompEquippable>()?.PrimaryVerb;
                    if (verb != null)
                    {
                        pawn.stances.SetStance(new Stance_Warmup(2, this.job.targetA, verb));
                    }
                }

                // 处理待执行的伤害 (Pending Actions)
                for (int i = pendingActions.Count - 1; i >= 0; i--)
                {
                    PendingDamageAction pendingAction = pendingActions[i];
                    pendingActions[i].fireAtTick--; // 倒计时

                    if (pendingActions[i].fireAtTick <= 0)
                    {
                        this.actionDamageSetting(pendingAction.setting, map);
                        pendingActions.RemoveAt(i);
                    }
                }

                // 处理主序列
                // 只要还有动作没执行，就检查时间
                if (this.damageSequence != null && this.nextActionIndex < this.damageSequence.Count)
                {
                    TickDelayDamageAndHediff nextAction = this.damageSequence[this.nextActionIndex];
                    if (currentToilTick >= nextAction.tick)
                    {
                        if (nextAction.effecterDef != null)
                        {
                            if (this.job.targetA.IsValid)
                                pawn.rotationTracker.FaceTarget(this.job.targetA);

                            Effecter effecter = nextAction.effecterDef.Spawn();
                            activeEffecters.Add(effecter);
                            effecter.Trigger(pawn, this.job.targetA.ToTargetInfo(map));
                        }

                        foreach (DamageSetting tickInfo in nextAction.damages)
                        {
                            PendingDamageAction pendingDamageAction = new PendingDamageAction();
                            pendingDamageAction.fireAtTick = tickInfo.Delaytick;
                            pendingDamageAction.setting = tickInfo;
                            pendingActions.Add(pendingDamageAction);
                        }
                        this.nextActionIndex++;
                    }
                }
                if (this.nextActionIndex >= this.damageSequence.Count && pendingActions.Count == 0)
                {
                    this.ReadyForNextToil();
                }
            };

            channelingToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return channelingToil;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pendingActions, "pendingActions", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (pendingActions == null) pendingActions = new List<PendingDamageAction>();
            }
        }
    }
}