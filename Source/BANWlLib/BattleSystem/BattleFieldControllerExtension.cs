using System.Collections.Generic;
using Verse;

namespace BANWlLib.BattleSystem
{
    public class BattleFieldControllerExtension : DefModExtension
    {
        public int durationTicks = 300;
        public int intervalTicks = 60;
        public float radius = 2.9f;
        public bool useCasterSnapshot = true;
        public EffecterDef pulseEffecter;
        public List<BattleActionConfig> actions = new List<BattleActionConfig>();
    }
}
