using Verse;

namespace BANWlLib.BattleSystem
{
    public class BattleHealRequest
    {
        public Thing instigator;
        public Pawn target;
        public float baseAmount;
        public float healPowerRatio;
        public bool canCrit = false;
        public bool allowPermanentInjuryHealing = false;
        public BattleCasterSnapshot snapshot;
    }
}
