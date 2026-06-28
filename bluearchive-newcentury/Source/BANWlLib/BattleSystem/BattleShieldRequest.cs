using Verse;

namespace BANWlLib.BattleSystem
{
    // 统一护盾请求，负责把施法者、目标和护盾倍率传入护盾结算层。
    public class BattleShieldRequest
    {
        public Thing instigator;
        public Pawn target;
        public float shieldPowerRatio;
        public HediffDef shieldHediffDef;
        public BattleCasterSnapshot snapshot;
    }
}
