using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Pojo
{
    // 持续伤害片段配置，负责描述某个 tick 触发的伤害或治疗行为。
    public class TickDamage
    {
        public int tick;
        public float damageAmount;
        public DamageDef damageType;
        public EffecterDef effecterDef;
        public float penetration = 0f;
        public bool isAttackBuilding = false;
        public float baseAmount = 0f;
        public float attackPowerRatio = 0f;
        public float healPowerRatio = 0f;
        public bool isHealing = false;
        public bool canCrit = true;
        public bool applyAffinity = true;
        public bool affectHostile = true;
        public bool affectFriendly = false;
        public bool allowPermanentInjuryHealing = false;

        // 转成统一战斗配置，负责让旧字段继续可用并进入新结算层。
        public BattleActionConfig ToBattleAction()
        {
            return new BattleActionConfig
            {
                baseAmount = ResolveBaseAmount(),
                attackPowerRatio = attackPowerRatio,
                healPowerRatio = healPowerRatio,
                damageDef = damageType,
                effecterDef = effecterDef,
                penetration = penetration,
                isHealing = isHealing,
                canCrit = canCrit,
                applyAffinity = applyAffinity,
                canHitBuilding = isAttackBuilding,
                affectHostile = affectHostile,
                affectFriendly = affectFriendly,
                allowPermanentInjuryHealing = allowPermanentInjuryHealing
            };
        }

        // 解析基础数值，负责兼容旧的 damageAmount 字段。
        private float ResolveBaseAmount()
        {
            return baseAmount != 0f ? baseAmount : damageAmount;
        }
    }
}
