using Verse;

namespace newpro
{
    public class PoitSaveComponent : GameComponent
    {
        public int poit = 0;

        public PoitSaveComponent(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref poit, "poit", 42);
        }
    }
}
