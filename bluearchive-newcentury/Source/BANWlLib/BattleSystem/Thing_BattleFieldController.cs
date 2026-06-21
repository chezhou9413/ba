using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    public class Thing_BattleFieldController : ThingWithComps
    {
        private Pawn caster;
        private BattleCasterSnapshot snapshot;
        private int ticksRemaining;
        private int ticksUntilPulse;
        private bool initialized;

        private BattleFieldControllerExtension Extension
        {
            get
            {
                return def.GetModExtension<BattleFieldControllerExtension>();
            }
        }

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
