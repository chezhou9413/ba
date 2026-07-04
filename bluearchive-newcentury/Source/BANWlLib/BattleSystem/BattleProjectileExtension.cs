using Verse;

namespace BANWlLib.BattleSystem
{
    // 普通投射物战斗配置，负责让原版 Projectile 接入技能倍率、护盾和 EX 标记。
    public class BattleProjectileExtension : DefModExtension
    {
        public float attackPowerRatio = 0f;
        public float baseMasteryMultiplier = 1f;
        public float shieldPowerRatio = 0f;
        public HediffDef shieldHediffDef;
        public bool isNormalAttack = false;
        public bool isShield = false;
        public bool isExSkill = false;
        public bool canCrit = true;
        public bool alwaysCrit = false;
        public bool alwaysShowCriticalText = false;
        public bool applyAffinity = true;
        public bool canHitOwnBuilding = false;
        public bool canHitOwnPawn = false;
    }
}
