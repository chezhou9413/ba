using Verse;

namespace BANWlLib.Skills
{
    //连射跟踪活体、尸体或最后位置，目标死亡不会提前结束整轮攻击。
    public static class SpecialBurstTarget
    {
        //尸体被搬走或销毁后保留最后可见落点，仍可结算该处的范围伤害。
        public static LocalTargetInfo Resolve(Thing target, Map map, ref IntVec3 lastCell)
        {
            Thing focus = target is Pawn pawn && pawn.Dead ? pawn.Corpse : target;
            if (focus != null && focus.Spawned && focus.Map == map)
            {
                lastCell = focus.Position;
                return new LocalTargetInfo(focus);
            }
            return new LocalTargetInfo(lastCell);
        }
    }
}
