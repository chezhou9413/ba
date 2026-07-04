using Verse;

namespace BANWlLib.BattleSystem
{
    // 统一战斗动作配置，负责描述一次伤害、治疗或附加状态效果的可配置参数。
    public class BattleActionConfig : IExposable
    {
        public float attackPowerRatio = 0f;
        // 本次动作指定的武器基础攻击力，0 表示由施法者当前主武器解析。
        public float weaponBaseAttack = 0f;
        public float baseMasteryMultiplier = 1f;
        public float healPowerRatio = 0f;
        public float shieldPowerRatio = 0f;
        public DamageDef damageDef;
        public HediffDef triggerHediff;
        public HediffDef shieldHediffDef;
        public EffecterDef effecterDef;
        public float penetration = 0f;
        public bool isHealing = false;
        public bool isShield = false;
        public bool isNormalAttack = false;
        public bool canCrit = true;
        public bool alwaysCrit = false;
        public bool alwaysShowCriticalText = false;
        public bool alwaysShowHealText = false;
        public bool applyAffinity = true;
        public bool canHitBuilding = false;
        public bool canHitOwnBuilding = false;
        public bool canHitOwnPawn = false;
        public bool affectHostile = true;
        public bool affectFriendly = false;
        public bool allowPermanentInjuryHealing = false;
        public bool isExSkill = false;
        public bool isProjectilePreview = false;
        public float previewWeaponBaseAttack = 0f;

        // 保存和读取战斗动作配置，负责支持场地控制器等可存档对象。
        public void ExposeData()
        {
            Scribe_Values.Look(ref attackPowerRatio, "attackPowerRatio", 0f);
            Scribe_Values.Look(ref weaponBaseAttack, "weaponBaseAttack", 0f);
            Scribe_Values.Look(ref baseMasteryMultiplier, "baseMasteryMultiplier", 1f);
            Scribe_Values.Look(ref healPowerRatio, "healPowerRatio", 0f);
            Scribe_Values.Look(ref shieldPowerRatio, "shieldPowerRatio", 0f);
            Scribe_Values.Look(ref penetration, "penetration", 0f);
            Scribe_Values.Look(ref isHealing, "isHealing", false);
            Scribe_Values.Look(ref isShield, "isShield", false);
            Scribe_Values.Look(ref isNormalAttack, "isNormalAttack", false);
            Scribe_Values.Look(ref canCrit, "canCrit", true);
            Scribe_Values.Look(ref alwaysCrit, "alwaysCrit", false);
            Scribe_Values.Look(ref alwaysShowCriticalText, "alwaysShowCriticalText", false);
            Scribe_Values.Look(ref alwaysShowHealText, "alwaysShowHealText", false);
            Scribe_Values.Look(ref applyAffinity, "applyAffinity", true);
            Scribe_Values.Look(ref canHitBuilding, "canHitBuilding", false);
            Scribe_Values.Look(ref canHitOwnBuilding, "canHitOwnBuilding", false);
            Scribe_Values.Look(ref canHitOwnPawn, "canHitOwnPawn", false);
            Scribe_Values.Look(ref affectHostile, "affectHostile", true);
            Scribe_Values.Look(ref affectFriendly, "affectFriendly", false);
            Scribe_Values.Look(ref allowPermanentInjuryHealing, "allowPermanentInjuryHealing", false);
            Scribe_Values.Look(ref isExSkill, "isExSkill", false);
            Scribe_Values.Look(ref isProjectilePreview, "isProjectilePreview", false);
            Scribe_Values.Look(ref previewWeaponBaseAttack, "previewWeaponBaseAttack", 0f);
            Scribe_Defs.Look(ref damageDef, "damageDef");
            Scribe_Defs.Look(ref triggerHediff, "triggerHediff");
            Scribe_Defs.Look(ref shieldHediffDef, "shieldHediffDef");
            Scribe_Defs.Look(ref effecterDef, "effecterDef");
        }
    }
}
