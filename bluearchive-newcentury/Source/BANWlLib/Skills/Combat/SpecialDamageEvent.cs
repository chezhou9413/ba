using Verse;

namespace BANWlLib.Skills
{
    //实际伤害事件，负责保存普攻身份、蓄积资格和单次致命伤去重编号。
    public class SpecialDamageEvent
    {
        public Pawn attacker;
        public Thing target;
        public bool normalHit;
        public bool canAccumulate;
        public float amount;
        public int id;
        public SpecialDamageEvent previous;
        public SpecialDamageScope scope;
        public bool expandArea = true;
    }
}

