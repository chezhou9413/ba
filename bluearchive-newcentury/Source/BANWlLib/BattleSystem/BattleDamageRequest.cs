using Verse;

namespace BANWlLib.BattleSystem
{
    // 统一伤害请求，负责把施法者、目标和技能参数传入战斗结算层。
    public class BattleDamageRequest
    {
        public bool useBattleStats = true;
        public float basePower = 100f;
        public float mechanismMultiplier = 1f;
        public float resolvedAmount = -1f;
        public bool canAccumulate = true;
        public bool normalHit;
        public bool areaSecondary;
        public bool damagePrepared = true;
        public Thing instigator;
        public Thing target;
        public DamageDef damageDef;
        public float attackPowerRatio;
        public float weaponBaseAttack;
        public float baseMasteryMultiplier = 1f;
        public float penetration;
        public bool isNormalAttack = false;
        public bool useNormalAttackStat = false;
        public bool canCrit = true;
        public bool alwaysCrit = false;
        public bool alwaysShowCriticalText = false;
        public bool applyAffinity = true;
        public bool isExSkill = false;
        public BattleCasterSnapshot snapshot;
    }
}
