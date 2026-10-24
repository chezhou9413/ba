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

        //从当前位置继续追踪目标与尸体，落点段保持指定位置。
        protected override void TickInterval(int delta)
        {
            if (attack != null && !attack.center.IsValid)
            {
                LocalTargetInfo target = SpecialBurstTarget.Resolve(attack.target, Map, ref attack.lastTargetCell);
                if (!target.IsValid) { base.TickInterval(delta); return; }
                Vector3 current = ExactPosition;
                origin = current.Yto0();
                destination = target.CenterVector3.Yto0();
                attack.impactDirection = (destination - origin).normalized;
                ticksToImpact = Mathf.Max(1, Mathf.CeilToInt(StartingTicksToImpact));
                lifetime = ticksToImpact;
            }
            base.TickInterval(delta);
        }
    }
}
