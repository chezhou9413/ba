using System.Collections.Generic;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 投射物多段追加伤害配置，负责让单发子弹命中后按配置追加多段统一战斗伤害。
    public class ProjectileMultiHitExtension : DefModExtension
    {
        public int damageIntervalTicks = 0;
        public bool canHitOwnPawn = false;
        public bool canHitOwnBuilding = false;
        public List<ProjectileExtraDamageConfig> extraDamages = new List<ProjectileExtraDamageConfig>();
    }

    // 投射物追加伤害段配置，负责描述单段追加伤害的公式参数、暴击和阵营过滤规则。
    public class ProjectileExtraDamageConfig : IExposable
    {
        public DamageDef damageDef;
        public DamageDef damageType;
        public float attackPowerRatio = 0f;
        public float normalAttackMultiplier = 1f;
        public float baseMasteryMultiplier = 1f;
        public int delayTicks = -1;
        public float penetration = -1f;
        public bool isNormalAttack = false;
        public bool canCrit = true;
        public bool alwaysCrit = false;
        public bool alwaysShowCriticalText = false;
        public bool applyAffinity = true;
        public bool isExSkill = false;
        public bool canHitOwnPawn = false;
        public bool canHitOwnBuilding = false;

        // 保存和读取单段追加伤害配置，负责支持延迟多段子弹在存读档后继续触发。
        public void ExposeData()
        {
            Scribe_Defs.Look(ref damageDef, "damageDef");
            Scribe_Defs.Look(ref damageType, "damageType");
            Scribe_Values.Look(ref attackPowerRatio, "attackPowerRatio", 0f);
            Scribe_Values.Look(ref normalAttackMultiplier, "normalAttackMultiplier", 1f);
            Scribe_Values.Look(ref baseMasteryMultiplier, "baseMasteryMultiplier", 1f);
            Scribe_Values.Look(ref delayTicks, "delayTicks", -1);
            Scribe_Values.Look(ref penetration, "penetration", -1f);
            Scribe_Values.Look(ref isNormalAttack, "isNormalAttack", false);
            Scribe_Values.Look(ref canCrit, "canCrit", true);
            Scribe_Values.Look(ref alwaysCrit, "alwaysCrit", false);
            Scribe_Values.Look(ref alwaysShowCriticalText, "alwaysShowCriticalText", false);
            Scribe_Values.Look(ref applyAffinity, "applyAffinity", true);
            Scribe_Values.Look(ref isExSkill, "isExSkill", false);
            Scribe_Values.Look(ref canHitOwnPawn, "canHitOwnPawn", false);
            Scribe_Values.Look(ref canHitOwnBuilding, "canHitOwnBuilding", false);
        }

        // 获取实际伤害类型，负责兼容 damageDef 和持续伤害配置中的旧字段 damageType。
        public DamageDef ResolveDamageDef()
        {
            return damageDef ?? damageType;
        }
    }
}
