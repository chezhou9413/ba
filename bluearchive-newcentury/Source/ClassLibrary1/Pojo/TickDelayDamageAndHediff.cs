using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.Pojo
{
    public class TickDelaySelfHediff
    {
        public int tick;
        public EffecterDef effecterDef = null;
        public List<SelfHediffSetting> damages = new List<SelfHediffSetting>();
    }

    public class SelfHediffSetting : IExposable
    {
        public int Delaytick;
        public EffecterDef effecterDef = null;
        public HediffDef tiggerHediff = null;
        public void ExposeData()
        {
            Scribe_Values.Look(ref Delaytick, "Delaytick", 0);
            Scribe_Defs.Look(ref tiggerHediff, "tiggerHediff");
        }
    }
    public class TickDelayDamageAndHediff
    {
        public int tick;
        public EffecterDef effecterDef = null;
        public List<DamageSetting> damages = new List<DamageSetting>();
    }

    public class DamageSetting : IExposable
    {
        public int Delaytick;
        public DamageDef damageType;
        public EffecterDef effecterDef = null;
        public HediffDef tiggerHediff = null;
        public float damageAmount;
        public float penetration = 0f;
        public bool isAttackBuilding = false;
        public void ExposeData()
        {
            Scribe_Values.Look(ref Delaytick, "Delaytick", 0);
            Scribe_Values.Look(ref damageAmount, "damageAmount", 0f);
            Scribe_Values.Look(ref penetration, "penetration", 0f);
            Scribe_Values.Look(ref isAttackBuilding, "isAttackBuilding", false);
            Scribe_Defs.Look(ref damageType, "damageType");
            Scribe_Defs.Look(ref effecterDef, "effecterDef");
            Scribe_Defs.Look(ref tiggerHediff, "tiggerHediff");
        }
    }
}
