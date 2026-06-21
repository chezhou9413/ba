namespace BANWlLib.BattleSystem
{
    // 伤害结算结果，负责记录最终伤害和参与显示的关键倍率。
    public class BattleDamageResult
    {
        public float finalAmount;
        public bool isCrit;
        public float affinityMultiplier = 1f;
        public float exSkillMultiplier = 1f;
    }

    // 治疗结算结果，负责记录最终治疗和实际恢复量。
    public class BattleHealResult
    {
        public float finalAmount;
        public float actualHealedAmount;
        public bool isCrit;
        public float exSkillMultiplier = 1f;
    }
}
