using BANWlLib.Pojo;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.BaDef
{
    public class BaJobDef_SphereAreaAttack : JobDef
    {
        public List<TickDelayDamageAndHediff> damages = new List<TickDelayDamageAndHediff>();
    }

    public class BaJobDef_SphereSelfHediff : JobDef
    {
        public List<TickDelaySelfHediff> damages = new List<TickDelaySelfHediff>();
    }
}
