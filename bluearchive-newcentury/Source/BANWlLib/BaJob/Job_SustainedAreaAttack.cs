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
    // 持续区域攻击 Job，负责按 tick 序列对一组格子反复执行伤害或治疗。
    public class Job_SustainedAreaAttack : JobDriver
    {
        private List<TickDamage> damageSequence;
        private int nextActionIndex;
        private List<LocalTargetInfo> Cells;
        private List<Effecter> activeEffecters = new List<Effecter>();

        private BaJobDef_SustainedAttack def
        {
            get { return (BaJobDef_SustainedAttack)job.def; }
        }

        // 预约 Job，负责允许该技能直接开始执行。
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        // 生成 Toil，负责在持续引导期间按时间轴结算每段效果。
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
                Cells = job.targetQueueA;
                pawn.pather.StopDead();
            };
            channelingToil.tickAction = () =>
            {
                for (int effecterIndex = activeEffecters.Count - 1; effecterIndex >= 0; effecterIndex--)
                {
                    Effecter effecter = activeEffecters[effecterIndex];
                    effecter.EffectTick(pawn, job.targetA.ToTargetInfo(map));
                    if (effecter.ticksLeft > 300)
                    {
                        effecter.Cleanup();
                        activeEffecters.RemoveAt(effecterIndex);
                    }
                }

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

                if (damageSequence == null || nextActionIndex >= damageSequence.Count)
                {
                    ReadyForNextToil();
                    return;
                }

                int currentToilTick = debugTicksSpentThisToil;
                TickDamage nextAction = damageSequence[nextActionIndex];
                if (currentToilTick < nextAction.tick)
                {
                    return;
                }

                BattleActionConfig action = nextAction.ToBattleAction();
                TriggerAreaEffect(nextAction.effecterDef, map);
                action.effecterDef = null;
                foreach (LocalTargetInfo target in Cells)
                {
                    if (!target.IsValid)
                    {
                        continue;
                    }

                    List<Thing> thingsInCell = target.Cell.GetThingList(map);
                    for (int i = 0; i < thingsInCell.Count; i++)
                    {
                        Thing thing = thingsInCell[i];
                        if (!BattleStatUtility.ShouldAffectTarget(pawn, thing, action))
                        {
                            continue;
                        }

                        BattleStatUtility.ApplyAction(pawn, thing, action);
                    }
                }

                nextActionIndex++;
            };
            channelingToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return channelingToil;
        }

        // 播放范围段特效，负责让 AOE 技能在目标点显示技能特效而不是依赖命中目标触发。
        private void TriggerAreaEffect(EffecterDef effecterDef, Map map)
        {
            if (effecterDef == null || map == null)
            {
                return;
            }

            TargetInfo targetInfo = job.targetA.ToTargetInfo(map);
            Effecter effecter = effecterDef.Spawn();
            activeEffecters.Add(effecter);
            effecter.Trigger(pawn, targetInfo);
        }
    }
}
