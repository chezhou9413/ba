using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    public class CompProperties_AbilitySpawnBattleField : CompProperties_AbilityEffect
    {
        public ThingDef fieldThingDef;
        public int durationTicksOverride = -1;

        public CompProperties_AbilitySpawnBattleField()
        {
            compClass = typeof(CompAbilityEffect_SpawnBattleField);
        }
    }

    public class CompAbilityEffect_SpawnBattleField : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnBattleField Props
        {
            get
            {
                return (CompProperties_AbilitySpawnBattleField)props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = parent.pawn;
            if (pawn == null || pawn.Map == null || Props.fieldThingDef == null)
            {
                return;
            }

            Thing thing = ThingMaker.MakeThing(Props.fieldThingDef);
            Thing_BattleFieldController controller = thing as Thing_BattleFieldController;
            if (controller == null)
            {
                Log.Error($"场地控制器 {Props.fieldThingDef.defName} 不是 Thing_BattleFieldController");
                return;
            }

            GenSpawn.Spawn(controller, target.Cell, pawn.Map);
            controller.Setup(pawn, Props.durationTicksOverride);
        }
    }
}
