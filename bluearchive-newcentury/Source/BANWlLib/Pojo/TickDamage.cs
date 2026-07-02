using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Pojo
{
    // 持续伤害片段配置，负责描述某个 tick 触发的伤害或治疗行为。
    public class TickDamage
    {
        public int tick;
        public DamageDef damageType;
        public EffecterDef effecterDef;
        public float penetration = 0f;
        public bool isAttackBuilding = false;
        public bool canHitOwnBuilding = false;
        public bool canHitOwnPawn = false;
        public float attackPowerRatio = 0f;
        public float healPowerRatio = 0f;
        public float shieldPowerRatio = 0f;
        public HediffDef shieldHediffDef;
        public HediffDef triggerHediff;
        public bool isHealing = false;
        public bool isShield = false;
        public bool canCrit = true;
        public bool alwaysShowCriticalText = false;
        public bool alwaysShowHealText = false;
        public bool applyAffinity = true;
        public bool affectHostile = true;
        public bool affectFriendly = false;
        public bool allowPermanentInjuryHealing = false;
        public bool isExSkill = false;

        // 转成统一战斗配置，负责让持续伤害片段进入统一结算层。
        public BattleActionConfig ToBattleAction()
        {
            return new BattleActionConfig
            {
                attackPowerRatio = attackPowerRatio,
                isNormalAttack = false,
                healPowerRatio = healPowerRatio,
                shieldPowerRatio = shieldPowerRatio,
                damageDef = damageType,
                triggerHediff = triggerHediff,
                shieldHediffDef = shieldHediffDef,
                effecterDef = effecterDef,
                penetration = penetration,
                isHealing = isHealing,
                isShield = isShield,
                canCrit = canCrit,
                alwaysShowCriticalText = alwaysShowCriticalText,
                alwaysShowHealText = alwaysShowHealText,
                applyAffinity = applyAffinity,
                canHitBuilding = isAttackBuilding,
                canHitOwnBuilding = canHitOwnBuilding,
                canHitOwnPawn = canHitOwnPawn,
                affectHostile = affectHostile,
                affectFriendly = affectFriendly,
                allowPermanentInjuryHealing = allowPermanentInjuryHealing,
                isExSkill = isExSkill
            };
        }
    }
}
