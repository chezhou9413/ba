using Verse;

namespace BANWlLib.BattleSystem
{
    // 统一伤害请求，负责把施法者、目标和技能参数传入战斗结算层。
    public class BattleDamageRequest
    {
        public Thing instigator;
        public Thing target;
        public DamageDef damageDef;
        public float baseAmount;
        public float attackPowerRatio;
        public float penetration;
        public bool canCrit = true;
        public bool applyAffinity = true;
        public bool isExSkill = false;
        public BattleCasterSnapshot snapshot;
    }
}
