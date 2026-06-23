using BANWlLib.BaDef;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 施法者属性快照，负责让脱手场地和延迟效果保存施放瞬间的战斗属性。
    public class BattleCasterSnapshot : IExposable
    {
        public float attackFlatBonus;
        public float attackPowerBase = 1f;
        public float attackMultiplier = 1f;
        public float attackPower;
        public float healFlatBonus;
        public float healMultiplier = 1f;
        public float healPower;
        public float criticalChance;
        public float criticalDamage;
        public float exSkillMultiplier = 1f;
        public damageType? damageType;

        // 保存和读取快照数据，负责让存档后的场地继续使用同一套施法属性。
        public void ExposeData()
        {
            Scribe_Values.Look(ref attackFlatBonus, "attackFlatBonus", 0f);
            Scribe_Values.Look(ref attackPowerBase, "attackPowerBase", 1f);
            Scribe_Values.Look(ref attackMultiplier, "attackMultiplier", 1f);
            Scribe_Values.Look(ref attackPower, "attackPower", 0f);
            Scribe_Values.Look(ref healFlatBonus, "healFlatBonus", 0f);
            Scribe_Values.Look(ref healMultiplier, "healMultiplier", 1f);
            Scribe_Values.Look(ref healPower, "healPower", 0f);
            Scribe_Values.Look(ref criticalChance, "criticalChance", 0f);
            Scribe_Values.Look(ref criticalDamage, "criticalDamage", 2f);
            Scribe_Values.Look(ref exSkillMultiplier, "exSkillMultiplier", 1f);
            Scribe_Values.Look(ref damageType, "damageType");
        }
    }
}
