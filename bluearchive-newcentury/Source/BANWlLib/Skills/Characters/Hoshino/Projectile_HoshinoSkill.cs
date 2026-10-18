using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //星野EX实体弹丸逐帧更新飞行终点，仍经过原版墙体与护盾拦截。
    public class Projectile_HoshinoSkill : Projectile_SpecialSkill
    {
        //与整数飞行时间使用相同分母，避免逐帧重瞄时反复丢失不足一tick的路程。
        public override Vector3 ExactPosition =>
            Vector3.Lerp(origin, destination, 1f - (float)ticksToImpact / Mathf.Max(1, Mathf.CeilToInt(StartingTicksToImpact))).Yto0()
            + Vector3.up * def.Altitude;

        //从当前位置继续追踪存活目标，目标失效后沿最后一段轨迹飞行。
        protected override void TickInterval(int delta)
        {
            Thing target = attack?.target;
            if (target != null && target.Spawned && target.Map == Map && !(target is Pawn pawn && pawn.Dead))
            {
                Vector3 current = ExactPosition;
                origin = current.Yto0();
                destination = target.DrawPos.Yto0();
                ticksToImpact = Mathf.Max(1, Mathf.CeilToInt(StartingTicksToImpact));
                lifetime = ticksToImpact;
            }
            base.TickInterval(delta);
        }
    }
}
