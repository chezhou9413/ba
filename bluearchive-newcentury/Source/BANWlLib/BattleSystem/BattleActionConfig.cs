using Verse;

namespace BANWlLib.BattleSystem
{
    public class BattleActionConfig : IExposable
    {
        public float baseAmount = 0f;
        public float attackPowerRatio = 0f;
        public float healPowerRatio = 0f;
        public DamageDef damageDef;
        public HediffDef triggerHediff;
        public EffecterDef effecterDef;
        public float penetration = 0f;
        public bool isHealing = false;
        public bool canCrit = true;
        public bool applyAffinity = true;
        public bool canHitBuilding = false;
        public bool affectHostile = true;
        public bool affectFriendly = false;
        public bool allowPermanentInjuryHealing = false;

        public void ExposeData()
        {
            Scribe_Values.Look(ref baseAmount, "baseAmount", 0f);
            Scribe_Values.Look(ref attackPowerRatio, "attackPowerRatio", 0f);
            Scribe_Values.Look(ref healPowerRatio, "healPowerRatio", 0f);
            Scribe_Values.Look(ref penetration, "penetration", 0f);
            Scribe_Values.Look(ref isHealing, "isHealing", false);
            Scribe_Values.Look(ref canCrit, "canCrit", true);
            Scribe_Values.Look(ref applyAffinity, "applyAffinity", true);
            Scribe_Values.Look(ref canHitBuilding, "canHitBuilding", false);
            Scribe_Values.Look(ref affectHostile, "affectHostile", true);
            Scribe_Values.Look(ref affectFriendly, "affectFriendly", false);
            Scribe_Values.Look(ref allowPermanentInjuryHealing, "allowPermanentInjuryHealing", false);
            Scribe_Defs.Look(ref damageDef, "damageDef");
            Scribe_Defs.Look(ref triggerHediff, "triggerHediff");
            Scribe_Defs.Look(ref effecterDef, "effecterDef");
        }
    }
}
