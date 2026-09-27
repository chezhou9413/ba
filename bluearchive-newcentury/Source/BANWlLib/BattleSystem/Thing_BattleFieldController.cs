using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    //战斗场地实体，负责按范围和间隔应用效果，并在持续时间结束时销毁。
    public class Thing_BattleFieldController : ThingWithComps
    {
        private Pawn caster;
        private BattleCasterSnapshot snapshot;
        private int ticksRemaining;
        private int ticksUntilPulse;
        private bool initialized;

        //公开实际剩余时间，供依赖场地的技能建立到期计时。
        public int RemainingTicks => ticksRemaining;

        //取得该场地定义的范围、脉冲和持续时间配置。
        private BattleFieldControllerExtension Extension
        {
            get
            {
                return def.GetModExtension<BattleFieldControllerExtension>();
            }
        }

        //初始化施法者、实际持续时间和可选属性快照。
        public void Setup(Pawn casterPawn, int durationTicksOverride = -1)
        {
            caster = casterPawn;
            initialized = true;
            ticksRemaining = durationTicksOverride > 0 ? durationTicksOverride : Extension.durationTicks;
            ticksUntilPulse = 0;
            if (Extension.useCasterSnapshot && casterPawn != null)
            {
                snapshot = BattleStatUtility.CreateSnapshot(casterPawn);
            }
        }

        //推进场地脉冲并处理持续时间结束。
        protected override void Tick()
        {
            base.Tick();
            if (!initialized)
            {
                Setup(caster);
            }

            ticksRemaining--;
            ticksUntilPulse--;
            if (ticksUntilPulse <= 0)
            {
                DoPulse();
                ticksUntilPulse = Extension.intervalTicks;
            }

            if (ticksRemaining <= 0)
            {
                Destroy();
            }
        }

        //按场地格子范围筛选目标并应用配置的战斗效果。
        private void DoPulse()
        {
            if (Map == null)
            {
                return;
            }

            if (Extension.pulseEffecter != null)
            {
                Effecter effecter = Extension.pulseEffecter.Spawn();
                effecter.Trigger(new TargetInfo(Position, Map), TargetInfo.Invalid);
                effecter.Cleanup();
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(Position, Extension.radius, true))
            {
                if (!cell.InBounds(Map))
                {
                    continue;
                }

                List<Thing> things = cell.GetThingList(Map);
                for (int i = 0; i < things.Count; i++)
                {
                    Thing thing = things[i];
                    for (int actionIndex = 0; actionIndex < Extension.actions.Count; actionIndex++)
                    {
                        BattleActionConfig action = Extension.actions[actionIndex];
                        if (!BattleStatUtility.ShouldAffectTarget(caster, thing, action))
                        {
                            continue;
                        }

                        BattleStatUtility.ApplyAction(caster, thing, action, snapshot);
                    }
                }
            }
        }

        //保存施法者、快照和场地运行时间。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref caster, "caster");
            Scribe_Deep.Look(ref snapshot, "snapshot");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
            Scribe_Values.Look(ref ticksUntilPulse, "ticksUntilPulse", 0);
            Scribe_Values.Look(ref initialized, "initialized", false);
        }
    }
}
