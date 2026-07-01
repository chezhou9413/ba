using Verse;

namespace BANWlLib.Projectiles
{
    // 穿透抛射体配置扩展，负责提供统一伤害参数、命中规则、飞行特效和方向特效参数。
    public class PiercingProjectileExtension : DefModExtension
    {
        public int damageIntervalTicks = 1;
        public float damageWidth = 1f;
        public float damageLength = 1f;
        public bool extendToMaxRange = false;
        public bool disablePiercing = false;
        public float maxRange = 0f;
        public bool immuneFriendlyFire = true;
        public float attackPowerRatio = 0f;
        public float normalAttackMultiplier = 1f;
        public float baseMasteryMultiplier = 1f;
        public bool isExSkill = false;
        public bool canCrit = true;
        // 是否强制暴击，负责让穿透弹本体伤害跳过随机暴击判定。
        public bool alwaysCrit = false;
        public bool alwaysShowCriticalText = false;
        public bool applyAffinity = true;
        public bool canHitBuilding = true;
        public bool affectHostile = true;
        public bool affectFriendly = false;
        public EffecterDef flightEffecter;
        public bool flightEffectAttachToProjectile = true;
        public bool flightEffectRotateWithProjectile = true;
        public float flightEffectOffsetForward = 0f;
        public float flightEffectOffsetRight = 0f;
        public float flightEffectOffsetUp = 0.4f;
        public EffecterDef directionalImpactEffect;
        public bool directionalImpactOnPawn = true;
        public bool directionalImpactOnBuilding = true;
        public float directionalImpactSpeed = 8f;
        public float directionalImpactOffsetForward = 0.2f;
        public float directionalImpactOffsetUp = 0.4f;
        public float startDrawScale = 1f;
        public float endDrawScale = 1f;
        public int fadeInTicks = 0;
        public int fadeOutTicks = 0;
    }
}
