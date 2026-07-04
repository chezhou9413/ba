using Verse;

namespace BANWlLib.BattleSystem
{
    // PawnKind 基础战斗属性扩展，负责让学生、敌人和普通 PawnKind 统一配置固有战斗属性。
    public class BattleBaseStatExtension : DefModExtension
    {
        // 初始生命值，参与生命值公式的基础乘算项，1 表示 100 点生命值。
        public float initialHealth = 0f;

        // 基础攻击力平加，进入技能、武器显示和统一伤害结算。
        public float attackFlat = 0f;

        // 基础攻击力百分比，0.2 表示最终攻击力额外增加 20%。
        public float attackPercent = 0f;

        // 初始治愈力，参与治愈力公式的基础乘算项。
        public float initialHeal = 0f;

        // 基础受回复倍率平加，0.2 表示在默认 100% 受疗基础上额外增加 20%。
        public float healReceivedMultiplierOffset = 0f;

        // 基础 EX 技能倍率平加，0.5 表示 EX 技能倍率从默认 100% 提高到 150%。
        public float exSkillMultiplierOffset = 0f;

        // 基础精通倍率平加，0.2 表示普通攻击口径伤害的基础精通从默认 100% 提高到 120%。
        public float baseMasteryMultiplierOffset = 0f;

        // 普通攻击倍率平加，0.2 表示武器平A伤害的普通攻击倍率从默认 100% 提高到 120%。
        public float normalAttackStatOffset = 0f;

        // 护盾存在时持续维护的跟随特效，负责让每个 PawnKind 配置自己的护盾表现。
        public EffecterDef shieldEffecterDef;

        // 攻击类型配置，填写 Explosion、Mysterious、Vibration、Through 或 Composite。
        public string damageType;

        // 护甲类型配置，填写 Explosion、Mysterious、Vibration、Through 或 Composite。
        public string defenseType;
    }
}
