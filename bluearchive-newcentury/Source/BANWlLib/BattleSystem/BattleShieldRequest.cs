using Verse;

namespace BANWlLib.BattleSystem
{
    // 统一护盾请求，负责把施法者、目标和护盾倍率传入护盾结算层。
    public class BattleShieldRequest
    {
        public bool useBattleStats = true;
        public float basePower = 100f;
        public BattleShieldSource source = BattleShieldSource.HealPower;
        public Thing instigator;
        public Pawn target;
        public float shieldPowerRatio;
        public HediffDef shieldHediffDef;
        public BattleCasterSnapshot snapshot;
    }

    //护盾基数来源，负责区分治疗力、最大生命和独立XML数值。
    public enum BattleShieldSource { HealPower, MaxHealth, Independent }
}
