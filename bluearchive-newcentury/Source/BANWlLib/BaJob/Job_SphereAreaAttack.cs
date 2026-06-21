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
    // 球形范围攻击 Job，负责按主序列和延迟序列在目标区域结算效果。
    public class Job_SphereAreaAttack : JobDriver
    {
        private List<TickDelayDamageAndHediff> damageSequence;
        private int nextActionIndex;
        private List<LocalTargetInfo> Cells;
        private List<Effecter> activeEffecters = new List<Effecter>();
        private List<PendingDamageAction> pendingActions = new List<PendingDamageAction>();

        private BaJobDef_SphereAreaAttack def => (BaJobDef_SphereAreaAttack)job.def;

        // 预约 Job，负责允许该技能直接开始执行。
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        // 执行单次延迟动作，负责对范围内目标应用统一战斗效果。
        private void actionDamageSetting(DamageSetting damage, Map map)
        {
            BattleActionConfig action = damage.ToBattleAction();
            foreach (LocalTargetInfo target in Cells)
            {
                if (!target.IsValid)
                {
                    continue;
                }

                List<Thing> thingsInCell = target.Cell.GetThingList(map);
                for (int i = thingsInCell.Count - 1; i >= 0; i--)
                {
                    Thing thing = thingsInCell[i];
                    if (!BattleStatUtility.ShouldAffectTarget(pawn, thing, action))
                    {
                        continue;
                    }

                    BattleStatUtility.ApplyAction(pawn, thing, action);
                }
            }
        }

        // 生成 Toil，负责维护主时间轴、待触发列表和朝向。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil channelingToil = new Toil();
            channelingToil.handlingFacing = true;
            rotateToFace = TargetIndex.A;
            Map map = pawn.Map;

            channelingToil.initAction = () =>
            {
                damageSequence = def.damages.OrderBy(d => d.tick).ToList();
                nextActionIndex = 0;
                pendingActions.Clear();
                activeEffecters.Clear();
                Cells = job.targetQueueA;
                pawn.pather.StopDead();
            };

            channelingToil.tickAction = () =>
            {
                int currentToilTick = debugTicksSpentThisToil;
                activeEffecters.RemoveAll(effecter =>
                {
                    effecter.EffectTick(pawn, job.targetA.ToTargetInfo(map));
                    if (effecter.ticksLeft > 300)
                    {
                        effecter.Cleanup();
                        return true;
                    }

                    return false;
                });

                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(job.targetA);
                if (pawn.stances != null && pawn.equipment?.Primary != null)
                {
                    var verb = pawn.equipment.Primary.GetComp<CompEquippable>()?.PrimaryVerb;
                    if (verb != null)
                    {
                        pawn.stances.SetStance(new Stance_Warmup(2, job.targetA, verb));
                    }
                }

                for (int i = pendingActions.Count - 1; i >= 0; i--)
                {
                    PendingDamageAction pendingAction = pendingActions[i];
                    pendingActions[i].fireAtTick--;
                    if (pendingActions[i].fireAtTick <= 0)
                    {
                        actionDamageSetting(pendingAction.setting, map);
                        pendingActions.RemoveAt(i);
                    }
                }

                if (damageSequence != null && nextActionIndex < damageSequence.Count)
                {
                    TickDelayDamageAndHediff nextAction = damageSequence[nextActionIndex];
                    if (currentToilTick >= nextAction.tick)
                    {
                        if (nextAction.effecterDef != null)
                        {
                            if (job.targetA.IsValid)
                            {
                                pawn.rotationTracker.FaceTarget(job.targetA);
                            }

                            Effecter effecter = nextAction.effecterDef.Spawn();
                            activeEffecters.Add(effecter);
                            effecter.Trigger(pawn, job.targetA.ToTargetInfo(map));
                        }

                        foreach (DamageSetting tickInfo in nextAction.damages)
                        {
                            PendingDamageAction pendingDamageAction = new PendingDamageAction();
                            pendingDamageAction.fireAtTick = tickInfo.Delaytick;
                            pendingDamageAction.setting = tickInfo;
                            pendingActions.Add(pendingDamageAction);
                        }
                        nextActionIndex++;
                    }
                }

                if (nextActionIndex >= damageSequence.Count && pendingActions.Count == 0)
                {
                    ReadyForNextToil();
                }
            };

            channelingToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return channelingToil;
        }

        // 保存和读取 Job 状态，负责让待触发动作支持读档续跑。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pendingActions, "pendingActions", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pendingActions == null)
            {
                pendingActions = new List<PendingDamageAction>();
            }
        }
    }
}
