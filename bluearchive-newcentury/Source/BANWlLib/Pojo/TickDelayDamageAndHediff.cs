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

    // 单个延迟效果配置，负责描述延迟伤害、治疗和附加状态参数。
    public class DamageSetting : IExposable
    {
        public int Delaytick;
        public DamageDef damageType;
        public EffecterDef effecterDef = null;
        public HediffDef tiggerHediff = null;
        public float penetration = 0f;
        public bool isAttackBuilding = false;
        public bool canHitOwnBuilding = false;
        public bool canHitOwnPawn = false;
        public float attackPowerRatio = 0f;
        public float healPowerRatio = 0f;
        public float shieldPowerRatio = 0f;
        public HediffDef shieldHediffDef;
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

        // 保存和读取延迟效果数据，负责支持存读档。
        public void ExposeData()
        {
            Scribe_Values.Look(ref Delaytick, "Delaytick", 0);
            Scribe_Values.Look(ref penetration, "penetration", 0f);
            Scribe_Values.Look(ref isAttackBuilding, "isAttackBuilding", false);
            Scribe_Values.Look(ref canHitOwnBuilding, "canHitOwnBuilding", false);
            Scribe_Values.Look(ref canHitOwnPawn, "canHitOwnPawn", false);
            Scribe_Values.Look(ref attackPowerRatio, "attackPowerRatio", 0f);
            Scribe_Values.Look(ref healPowerRatio, "healPowerRatio", 0f);
            Scribe_Values.Look(ref shieldPowerRatio, "shieldPowerRatio", 0f);
            Scribe_Values.Look(ref isHealing, "isHealing", false);
            Scribe_Values.Look(ref isShield, "isShield", false);
            Scribe_Values.Look(ref canCrit, "canCrit", true);
            Scribe_Values.Look(ref alwaysShowCriticalText, "alwaysShowCriticalText", false);
            Scribe_Values.Look(ref alwaysShowHealText, "alwaysShowHealText", false);
            Scribe_Values.Look(ref applyAffinity, "applyAffinity", true);
            Scribe_Values.Look(ref affectHostile, "affectHostile", true);
            Scribe_Values.Look(ref affectFriendly, "affectFriendly", false);
            Scribe_Values.Look(ref allowPermanentInjuryHealing, "allowPermanentInjuryHealing", false);
            Scribe_Values.Look(ref isExSkill, "isExSkill", false);
            Scribe_Defs.Look(ref damageType, "damageType");
            Scribe_Defs.Look(ref effecterDef, "effecterDef");
            Scribe_Defs.Look(ref tiggerHediff, "tiggerHediff");
            Scribe_Defs.Look(ref shieldHediffDef, "shieldHediffDef");
        }

        // 转成统一战斗配置，负责让范围伤害和场地伤害共用同一套入口。
        public BattleActionConfig ToBattleAction()
        {
            return new BattleActionConfig
            {
                attackPowerRatio = attackPowerRatio,
                isNormalAttack = false,
                healPowerRatio = healPowerRatio,
                shieldPowerRatio = shieldPowerRatio,
                damageDef = damageType,
                triggerHediff = tiggerHediff,
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
