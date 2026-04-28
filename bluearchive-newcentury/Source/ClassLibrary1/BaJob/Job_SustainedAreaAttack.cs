using BANWlLib.BaDef;
using BANWlLib.Pojo;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BANWlLib.BaJob
{
    public class Job_SustainedAreaAttack : JobDriver
    {
        private List<TickDamage> damageSequence;
        private int nextActionIndex;
        private List<LocalTargetInfo> Cells;
        private List<Effecter> activeEffecters = new List<Effecter>();
        private BaJobDef_SustainedAttack def
        {
            get { return (BaJobDef_SustainedAttack)this.job.def; }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<TickDamage> damages = def.damages;
            Toil channelingToil = new Toil();
            // 确保等待挨打的过程中，游戏内的自动朝向逻辑生效（使用 JobDriver.rotateToFace 机制）
            channelingToil.handlingFacing = true;
            this.rotateToFace = TargetIndex.A;
            Map map = pawn.Map;
            channelingToil.initAction = () =>
            {
                this.damageSequence = def.damages.OrderBy(d => d.tick).ToList();
                this.nextActionIndex = 0;
                Cells = this.job.targetQueueA;
                pawn.pather.StopDead();
            };
            channelingToil.tickAction = () =>
            {
                foreach (Effecter effecter in activeEffecters)
                {
                    effecter.EffectTick(pawn, this.job.targetA.ToTargetInfo(map));
                    if(effecter.ticksLeft > 300)
                    {
                        effecter.Cleanup();
                    }
                }
                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(this.job.targetA);
                // 让武器也对准目标
                if (pawn.stances != null && pawn.equipment?.Primary != null)
                {
                    var verb = pawn.equipment.Primary.GetComp<CompEquippable>()?.PrimaryVerb;
                    if (verb != null)
                    {
                        pawn.stances.SetStance(new Stance_Warmup(2, this.job.targetA, verb));
                    }
                }
                if (this.damageSequence == null || this.nextActionIndex >= this.damageSequence.Count)
                {
                    this.ReadyForNextToil(); // 序列完成，结束 Toil
                    return;
                }
                int currentToilTick = this.debugTicksSpentThisToil;
                TickDamage nextAction = this.damageSequence[this.nextActionIndex];
                if (currentToilTick >= nextAction.tick)
                {
                    // 触发 TickDamage 配置的特效：直接在 pawn 原地播放一次，特效朝向随 pawn 当前面向
                    if (nextAction.effecterDef != null)
                    {
                        // 保证面向主目标，然后在自身位置触发
                        if (this.job.targetA.IsValid)
                        {
                            pawn.rotationTracker.FaceTarget(this.job.targetA);
                        }
                        Effecter effecter = nextAction.effecterDef.Spawn();
                        activeEffecters.Add(effecter);
                        effecter.Trigger(pawn, this.job.targetA.ToTargetInfo(map));
                    }
                    DamageInfo info = new DamageInfo(
                              def.damages[nextActionIndex].damageType,
                              def.damages[nextActionIndex].damageAmount,
                              def.damages[nextActionIndex].penetration,
                              -1f,
                              pawn,
                              null,
                              null);
                    foreach (LocalTargetInfo target in Cells)
                    {
                        IntVec3 cell = target.Cell;
                        List<Thing> thingsInCell = cell.GetThingList(map);
                        for (int i = 0; i < thingsInCell.Count; i++)
                        {
                            if (nextAction.isAttackBuilding)
                            {
                                if (thingsInCell[i] is Building building)
                                {
                                    building.TakeDamage(info);
                                }
                            }
                            if (thingsInCell[i] is Pawn targetPawn)
                            {
                                if (targetPawn.Faction == Faction.OfPlayer)
                                    continue;
                                targetPawn.TakeDamage(info);
                            }
                        }
                    }
                    this.nextActionIndex++;
                }
            };
            channelingToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return channelingToil;
        }
    }
}

