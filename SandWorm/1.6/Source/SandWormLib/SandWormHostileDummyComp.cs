using RimWorld;
using Verse;

namespace SandWormLib
{
    public class CompProperties_SandWormHostileDummy : CompProperties_CausesGameCondition
    {
        public CompProperties_SandWormHostileDummy()
        {
            compClass = typeof(CompSandWormHostileDummy);
        }
    }

    public class CompSandWormHostileDummy : CompCauseGameCondition
    {
        public override bool Active => false;

        public override void CompTick()
        {
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
        }
    }
}
