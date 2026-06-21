using System.Collections.Generic;
using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Pojo
{
    // 延迟自我状态配置，负责在某个 tick 延迟后给自身附加 Hediff。
    public class TickDelaySelfHediff
    {
        public int tick;
        public EffecterDef effecterDef = null;
        public List<SelfHediffSetting> damages = new List<SelfHediffSetting>();
    }

    // 单个自我状态配置，负责描述延迟和目标 Hediff。
    public class SelfHediffSetting : IExposable
    {
        public int Delaytick;
        public EffecterDef effecterDef = null;
        public HediffDef tiggerHediff = null;

        // 保存和读取延迟状态数据，负责支持存读档。
        public void ExposeData()
        {
            Scribe_Values.Look(ref Delaytick, "Delaytick", 0);
            Scribe_Defs.Look(ref tiggerHediff, "tiggerHediff");
        }
    }

    // 延迟范围效果配置，负责描述某个主 tick 下的一组延迟伤害或治疗。
    public class TickDelayDamageAndHediff
    {
        public int tick;
        public EffecterDef effecterDef = null;
        public List<DamageSetting> damages = new List<DamageSetting>();
    }

    // 单个延迟效果配置，负责兼容旧伤害字段并接入统一战斗层。
    public class DamageSetting : IExposable
    {
        public int Delaytick;
        public DamageDef damageType;
        public EffecterDef effecterDef = null;
        public HediffDef tiggerHediff = null;
        public float damageAmount;
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
        public bool isExSkill = false;

        // 保存和读取延迟效果数据，负责支持存读档。
        public void ExposeData()
        {
            Scribe_Values.Look(ref Delaytick, "Delaytick", 0);
            Scribe_Values.Look(ref damageAmount, "damageAmount", 0f);
            Scribe_Values.Look(ref penetration, "penetration", 0f);
            Scribe_Values.Look(ref isAttackBuilding, "isAttackBuilding", false);
            Scribe_Values.Look(ref baseAmount, "baseAmount", 0f);
            Scribe_Values.Look(ref attackPowerRatio, "attackPowerRatio", 0f);
            Scribe_Values.Look(ref healPowerRatio, "healPowerRatio", 0f);
            Scribe_Values.Look(ref isHealing, "isHealing", false);
            Scribe_Values.Look(ref canCrit, "canCrit", true);
            Scribe_Values.Look(ref applyAffinity, "applyAffinity", true);
            Scribe_Values.Look(ref affectHostile, "affectHostile", true);
            Scribe_Values.Look(ref affectFriendly, "affectFriendly", false);
            Scribe_Values.Look(ref allowPermanentInjuryHealing, "allowPermanentInjuryHealing", false);
            Scribe_Values.Look(ref isExSkill, "isExSkill", false);
            Scribe_Defs.Look(ref damageType, "damageType");
            Scribe_Defs.Look(ref effecterDef, "effecterDef");
            Scribe_Defs.Look(ref tiggerHediff, "tiggerHediff");
        }

        // 转成统一战斗配置，负责让范围伤害和场地伤害共用同一套入口。
        public BattleActionConfig ToBattleAction()
        {
            return new BattleActionConfig
            {
                baseAmount = ResolveBaseAmount(),
                attackPowerRatio = attackPowerRatio,
                healPowerRatio = healPowerRatio,
                damageDef = damageType,
                triggerHediff = tiggerHediff,
                effecterDef = effecterDef,
                penetration = penetration,
                isHealing = isHealing,
                canCrit = canCrit,
                applyAffinity = applyAffinity,
                canHitBuilding = isAttackBuilding,
                affectHostile = affectHostile,
                affectFriendly = affectFriendly,
                allowPermanentInjuryHealing = allowPermanentInjuryHealing,
                isExSkill = isExSkill
            };
        }

        // 解析基础数值，负责兼容旧的 damageAmount 字段。
        private float ResolveBaseAmount()
        {
            return baseAmount != 0f ? baseAmount : damageAmount;
        }
    }
}
