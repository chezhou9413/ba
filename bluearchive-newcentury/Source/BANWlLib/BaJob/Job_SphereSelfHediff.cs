using BANWlLib.BaDef;
using BANWlLib.BattleSystem;
using BANWlLib.Pojo;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BANWlLib.BaJob
{
    // 球形范围友方状态 Job，负责按延迟配置给自身和友方 Pawn 附加 Hediff。
    public class Job_SphereSelfHediff : JobDriver
    {
        private List<TickDelaySelfHediff> HediffSequence;
        private int nextActionIndex;
        private List<LocalTargetInfo> Cells;
        private List<Effecter> activeEffecters = new List<Effecter>();
        private List<PendingHediffAction> pendingActions = new List<PendingHediffAction>();

        private BaJobDef_SphereSelfHediff def => (BaJobDef_SphereSelfHediff)this.job.def;

        // 预约 Job，负责允许该技能直接开始执行。
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        // 执行单段 Hediff 配置，负责给范围内自身和友方 Pawn 附加状态。
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
                        if (CanApplyHediffToTarget(targetPawn))
                        {
                            HediffDef hediffDef = damage.ResolveHediff();
                            if (hediffDef != null)
                            {
                                BattleHediffSnapshotUtility.RegisterSnapshotIfNeeded(targetPawn, hediffDef, pawn);
                                Hediff hediff = HediffMaker.MakeHediff(hediffDef, targetPawn);
                                if (hediff != null)
                                {
                                    BattleHediffSnapshotUtility.ApplySnapshotIfNeeded(hediff, pawn);
                                    targetPawn.health.AddHediff(hediff);
                                }
                                else
                                {
                                    Log.Error("初始化hediff失败：" + hediffDef.defName);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 判断状态目标是否合法，负责放行施法者自身、同阵营 Pawn 和盟友阵营 Pawn。
        private bool CanApplyHediffToTarget(Pawn targetPawn)
        {
            if (targetPawn == null || targetPawn.Dead)
            {
                return false;
            }

            if (targetPawn == pawn)
            {
                return true;
            }

            if (pawn?.Faction == null || targetPawn.Faction == null)
            {
                return false;
            }

            if (targetPawn.Faction == pawn.Faction)
            {
                return true;
            }

            return pawn.Faction.RelationKindWith(targetPawn.Faction) == FactionRelationKind.Ally;
        }

        // 生成 Toil，负责维护状态时间轴、延迟执行列表和施法朝向。
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
                // 清理上一次执行遗留的特效引用。
                this.activeEffecters.Clear();
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

                // 处理等待触发的状态动作。
                for (int i = pendingActions.Count - 1; i >= 0; i--)
                {
                    PendingHediffAction pendingAction = pendingActions[i];
                    // 每 tick 减少等待时间，归零后立即执行。
                    pendingActions[i].fireAtTick--;

                    if (pendingActions[i].fireAtTick <= 0)
                    {
                        this.actionDamageSetting(pendingAction.setting, map);
                        pendingActions.RemoveAt(i);
                    }
                }

                // 检查主时间轴，把到达触发时间的配置加入等待列表。
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

        // 保存和读取 Job 状态，负责让待触发状态支持读档续跑。
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
