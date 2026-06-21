using Verse;

namespace BANWlLib.BattleSystem
{
    public class BattleDamageRequest
    {
        public Thing instigator;
        public Thing target;
        public DamageDef damageDef;
        public float baseAmount;
        public float attackPowerRatio;
        public float penetration;
        public bool canCrit = true;
        public bool applyAffinity = true;
        public BattleCasterSnapshot snapshot;
    }
}
