using Verse;

namespace BANWlLib.BattleSystem
{
    // PawnKind 基础战斗属性扩展，负责让学生、敌人和普通 PawnKind 统一配置固有战斗属性。
    public class BattleBaseStatExtension : DefModExtension
    {
        // 基础生命尺度平加，参与 Pawn.HealthScale 计算，不是直接最终生命值。
        public float healthFlat = 0f;

        // 基础生命尺度百分比，0.2 表示生命尺度额外增加 20%。
        public float healthPercent = 0f;

        // 基础攻击力平加，进入技能、武器显示和统一伤害结算。
        public float attackFlat = 0f;

        // 基础攻击力百分比，0.2 表示最终攻击力额外增加 20%。
        public float attackPercent = 0f;

        // 基础治疗力平加，会叠加到 BANW_HealPowerBase 之后参与统一治疗结算。
        public float healFlat = 0f;

        // 基础治疗力百分比，0.2 表示最终治疗力额外增加 20%。
        public float healPercent = 0f;

        // 基础受回复倍率平加，0.2 表示在默认 100% 受疗基础上额外增加 20%。
        public float healReceivedMultiplierOffset = 0f;

        // 基础 EX 技能倍率平加，0.5 表示 EX 技能倍率从默认 100% 提高到 150%。
        public float exSkillMultiplierOffset = 0f;

        // 攻击类型配置，填写 Explosion、Mysterious、Vibration、Through 或 Composite。
        public string damageType;

        // 护甲类型配置，填写 Explosion、Mysterious、Vibration、Through 或 Composite。
        public string defenseType;
    }
}
