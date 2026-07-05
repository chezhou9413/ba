using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    //多发投射物瞄准姿态，负责在延迟连射队列存在期间停留并绘制主武器瞄准。
    public class MultiShotAimStance : Stance_Busy
    {
        //读取存档姿态，负责让原版 Scribe 能创建连射瞄准姿态。
        public MultiShotAimStance()
        {
            neverAimWeapon = true;
        }

        //创建短暂瞄准姿态，负责保存目标并关闭原版装备重复绘制。
        public MultiShotAimStance(int ticks, LocalTargetInfo focusTarg)
            : base(ticks, focusTarg, null)
        {
            neverAimWeapon = true;
        }

        //绘制姿态，负责复用原版武器瞄准绘制让小人朝目标举枪。
        public override void StanceDraw()
        {
            base.StanceDraw();
            Pawn pawn = stanceTracker?.pawn;
            ThingWithComps weapon = pawn?.equipment?.Primary;
            if (pawn == null || weapon == null || !focusTarg.IsValid)
            {
                return;
            }

            Vector3 targetPos = focusTarg.HasThing ? focusTarg.Thing.DrawPos : focusTarg.Cell.ToVector3Shifted();
            Vector3 drawPos = pawn.DrawPos;
            if ((targetPos - drawPos).MagnitudeHorizontalSquared() <= 0.001f)
            {
                return;
            }

            float aimAngle = (targetPos - drawPos).AngleFlat();
            float distanceFactor = pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;
            drawPos += new Vector3(0f, 0f, 0.4f + weapon.def.equippedDistanceOffset).RotatedBy(aimAngle) * distanceFactor;
            PawnRenderUtility.DrawEquipmentAiming(weapon, drawPos, aimAngle);
        }
    }
}
