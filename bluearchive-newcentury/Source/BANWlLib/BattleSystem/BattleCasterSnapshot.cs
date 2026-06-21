using BANWlLib.BaDef;
using Verse;

namespace BANWlLib.BattleSystem
{
    public class BattleCasterSnapshot : IExposable
    {
        public float attackFlatBonus;
        public float attackMultiplier = 1f;
        public float attackPower;
        public float healFlatBonus;
        public float healMultiplier = 1f;
        public float healPower;
        public float criticalChance;
        public float criticalDamage;
        public damageType? damageType;

        public void ExposeData()
        {
            Scribe_Values.Look(ref attackFlatBonus, "attackFlatBonus", 0f);
            Scribe_Values.Look(ref attackMultiplier, "attackMultiplier", 1f);
            Scribe_Values.Look(ref attackPower, "attackPower", 0f);
            Scribe_Values.Look(ref healFlatBonus, "healFlatBonus", 0f);
            Scribe_Values.Look(ref healMultiplier, "healMultiplier", 1f);
            Scribe_Values.Look(ref healPower, "healPower", 0f);
            Scribe_Values.Look(ref criticalChance, "criticalChance", 0f);
            Scribe_Values.Look(ref criticalDamage, "criticalDamage", 2f);
            Scribe_Values.Look(ref damageType, "damageType");
        }
    }
}
