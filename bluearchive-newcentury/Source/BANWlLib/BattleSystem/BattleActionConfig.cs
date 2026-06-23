using Verse;

namespace BANWlLib.BattleSystem
{
    // 统一战斗动作配置，负责描述一次伤害、治疗或附加状态效果的可配置参数。
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
        public bool isExSkill = false;
        public bool isProjectilePreview = false;

        // 保存和读取战斗动作配置，负责支持场地控制器等可存档对象。
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
            Scribe_Values.Look(ref isExSkill, "isExSkill", false);
            Scribe_Values.Look(ref isProjectilePreview, "isProjectilePreview", false);
            Scribe_Defs.Look(ref damageDef, "damageDef");
            Scribe_Defs.Look(ref triggerHediff, "triggerHediff");
            Scribe_Defs.Look(ref effecterDef, "effecterDef");
        }
    }
}
