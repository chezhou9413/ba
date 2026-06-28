using Verse;

namespace BANWlLib.BattleSystem
{
    // 统一治疗请求，负责把施法者、目标和治疗参数传入战斗结算层。
    public class BattleHealRequest
    {
        public Thing instigator;
        public Pawn target;
        public float healPowerRatio;
        public bool canCrit = false;
        public bool alwaysShowHealText = false;
        public bool allowPermanentInjuryHealing = false;
        public bool isExSkill = false;
        public BattleCasterSnapshot snapshot;
    }
}
