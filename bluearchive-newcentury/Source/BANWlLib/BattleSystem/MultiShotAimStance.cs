using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    //多发投射物瞄准姿态，在延迟连射队列存在期间停留并绘制主武器瞄准。
    public class MultiShotAimStance : Stance_Busy
    {
        //读取存档姿态，让原版 Scribe 能创建连射瞄准姿态。
        public MultiShotAimStance()
        {
        }

        //保存目标，由原版装备绘制读取瞄准方向。
        public MultiShotAimStance(int ticks, LocalTargetInfo focusTarg)
            : base(ticks, focusTarg, null)
        {
        }

        //仅在工作明确隐藏原版武器时补画，避免携带姿态与瞄准姿态各画一把枪。
        public override void StanceDraw()
        {
            base.StanceDraw();
            Pawn pawn = stanceTracker?.pawn;
            if (pawn?.CurJob != null && pawn.CurJob.def.neverShowWeapon == false) return;
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
