using BANWlLib.BaDef;
using BANWlLib.Pojo;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BANWlLib.BaJob
{
    public class Job_SphereSelfHediff : JobDriver
    {
        private List<TickDelaySelfHediff> HediffSequence;
        private int nextActionIndex;
        private List<LocalTargetInfo> Cells;
        private List<Effecter> activeEffecters = new List<Effecter>();
        private List<PendingHediffAction> pendingActions = new List<PendingHediffAction>();

        private BaJobDef_SphereSelfHediff def => (BaJobDef_SphereSelfHediff)this.job.def;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private void actionDamageSetting(SelfHediffSetting damage, Map map)
        {
            if (damage.effecterDef != null)
            {
                Effecter effecter = damage.effecterDef.Spawn();
                activeEffecters.Add(effecter);
                TargetInfo centerTarget = new TargetInfo(TargetA.Cell, map);
                effecter.Trigger(centerTarget, TargetInfo.Invalid);
            }
            foreach (LocalTargetInfo target in Cells)
            {
                if (!target.IsValid) continue;
                IntVec3 cell = target.Cell;
                List<Thing> thingsInCell = cell.GetThingList(map);
                for (int i = thingsInCell.Count - 1; i >= 0; i--)
                {
                    Thing t = thingsInCell[i];
                    if (t is Pawn targetPawn)
                    {
                        if (targetPawn.Faction.IsPlayer)
                        {
                            if (damage.tiggerHediff != null)
                            {
                                Hediff hediff = HediffMaker.MakeHediff(damage.tiggerHediff, targetPawn);
                                if (hediff != null)
                                {
                                    targetPawn.health.AddHediff(hediff);
                                }
                                else
                                {
                                    Log.Error("初始化hediff失败：" + damage.tiggerHediff.defName);
                                }
                            }
                        }
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
                this.HediffSequence = def.damages.OrderBy(d => d.tick).ToList();
                this.nextActionIndex = 0;
                this.pendingActions.Clear();
                this.activeEffecters.Clear(); // 确保重置
                Cells = this.job.targetQueueA;
                pawn.pather.StopDead();
            };

            channelingToil.tickAction = () =>
            {
                int currentToilTick = this.debugTicksSpentThisToil;
                activeEffecters.RemoveAll(effecter =>
                {
                    effecter.EffectTick(pawn, this.job.targetA.ToTargetInfo(map));
                    if (effecter.ticksLeft > 300)
                    {
                        effecter.Cleanup();
                        return true;
                    }
                    return false;
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
                    PendingHediffAction pendingAction = pendingActions[i];
                    pendingActions[i].fireAtTick--; // 倒计时

                    if (pendingActions[i].fireAtTick <= 0)
                    {
                        this.actionDamageSetting(pendingAction.setting, map);
                        pendingActions.RemoveAt(i);
                    }
                }

                // 处理主序列
                // 只要还有动作没执行，就检查时间
                if (this.HediffSequence != null && this.nextActionIndex < this.HediffSequence.Count)
                {
                    TickDelaySelfHediff nextAction = this.HediffSequence[this.nextActionIndex];
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

                        foreach (SelfHediffSetting tickInfo in nextAction.damages)
                        {
                            PendingHediffAction pendingHediffAction = new PendingHediffAction();
                            pendingHediffAction.fireAtTick = tickInfo.Delaytick;
                            pendingHediffAction.setting = tickInfo;
                            pendingActions.Add(pendingHediffAction);
                        }
                        this.nextActionIndex++;
                    }
                }
                if (this.nextActionIndex >= this.HediffSequence.Count && pendingActions.Count == 0)
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
                if (pendingActions == null) pendingActions = new List<PendingHediffAction>();
            }
        }
    }
}