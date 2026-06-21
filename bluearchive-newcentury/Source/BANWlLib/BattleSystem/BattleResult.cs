namespace BANWlLib.BattleSystem
{
    public class BattleDamageResult
    {
        public float finalAmount;
        public bool isCrit;
        public float affinityMultiplier = 1f;
    }

    public class BattleHealResult
    {
        public float finalAmount;
        public float actualHealedAmount;
        public bool isCrit;
    }
}
