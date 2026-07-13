using UnityEngine;
using Verse;

namespace SandWormLib.SandWormBehavior
{
    public sealed class SandWormChargeBehavior : SandWormBehaviorBase
    {
        // 基准值
        private const float BaseChargeMoveSpeed = 0.224f;
        private const float BaseChargeMaxTurnPerTick = 0.65f;
        private const float ChargeTurnBlend = 0.18f;
        private const float ChargeDistance = float.MaxValue;
        private const int ChargeBoundaryMargin = 8;
        private const int MaxChargeDurationTicks = 600;
        private const int MinChargeCommitTicks = 30;
        private const int ChargeCooldownTicks = 900;
        private const float LockAngleThreshold = 7f;
        private const float RecoveryTurnFactor = 0.06f;
        private const float BaseRecoveryMaxTurnPerTick = 0.95f;
        private const float RecoveryTargetInset = 16f;
        private const float WillLeaveLookaheadBase = 18f;

        private int enterTick;

        public SandWormChargeBehavior()
            : base(new SandWormBehaviorDef
            {
                Id = SandWormBehaviorIds.Charge,
                EnableSurfacePressure = true,
                DrawPath = true
            })
        {
        }

        public override void Enter(SandWormBehaviorContext context)
        {
            enterTick = context.CurrentTick;
            context.Blackboard.LastChargeTargetAcquireTick = context.CurrentTick;
            context.Blackboard.ChargeLocked = false;
            context.Blackboard.ChargeDirection = Vector3.zero;

            if (!context.HasValidChargeTarget(ChargeDistance))
            {
                context.Blackboard.CurrentTarget = context.FindPreferredChargeTarget(ChargeDistance);
            }
        }

        public override void Tick(SandWormBehaviorContext context)
        {
            if (!context.HasHeadTransform)
            {
                return;
            }

            // 按尺寸缩放
            float moveSpeed = BaseChargeMoveSpeed * context.MoveSpeedScale;
            float maxTurn = BaseChargeMaxTurnPerTick * context.TurnRateScale;
            float recoveryMaxTurn = BaseRecoveryMaxTurnPerTick * context.TurnRateScale;

            if (context.IsOutsideMapBounds())
            {
                context.Blackboard.ChargeLocked = false;
                context.Blackboard.ChargeDirection = Vector3.zero;

                Vector3 recoveryTarget = context.GetRecoveryTarget(RecoveryTargetInset);
                float recoveryAngle = context.AngleTo(recoveryTarget);
                float recoveryTurn = Mathf.Clamp(
                    recoveryAngle * RecoveryTurnFactor,
                    -recoveryMaxTurn,
                    recoveryMaxTurn);

                context.HeadYawDeg += recoveryTurn;
                context.SetHeadRotation(context.HeadYawDeg);
                context.ApplyMovementAndVisuals(
                    moveSpeed,
                    recoveryMaxTurn,
                    ChargeTurnBlend,
                    recoveryTurn,
                    SandWormThing.HeightOffset,
                    SandWormThing.BankMaxDeg * 1.6f,
                    SandWormThing.ViewTiltXMaxDeg,
                    SandWormThing.BankLerp,
                    2);
                return;
            }

            Thing target = context.Blackboard.CurrentTarget;
            if (target != null)
            {
                Vector3 chargeDirection = context.Blackboard.ChargeDirection;
                if (!context.Blackboard.ChargeLocked)
                {
                    float angle = context.AngleTo(target.DrawPos);
                    float turn = Mathf.Clamp(angle * 0.04f, -maxTurn, maxTurn);
                    context.HeadYawDeg += turn;
                    context.SetHeadRotation(context.HeadYawDeg);

                    if (Mathf.Abs(angle) <= LockAngleThreshold)
                    {
                        float yawRad = context.HeadYawDeg * Mathf.Deg2Rad;
                        context.Blackboard.ChargeDirection = new Vector3(-Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
                        context.Blackboard.ChargeLocked = true;
                    }

                    // 追踪阶段：轻微震动
                    if (context.CurrentTick % 14 == 0)
                        TryShakeCamera(0.20f);

                    context.ApplyMovementAndVisuals(
                        moveSpeed,
                        maxTurn,
                        ChargeTurnBlend,
                        turn,
                        SandWormThing.HeightOffset,
                        SandWormThing.BankMaxDeg * 1.5f,
                        SandWormThing.ViewTiltXMaxDeg,
                        SandWormThing.BankLerp * 1.2f,
                        2);
                    return;
                }

                if (chargeDirection.sqrMagnitude < 0.001f)
                {
                    float yawRad = context.HeadYawDeg * Mathf.Deg2Rad;
                    chargeDirection = new Vector3(-Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
                    context.Blackboard.ChargeDirection = chargeDirection;
                }

                float lockedYaw = Mathf.Atan2(-chargeDirection.x, chargeDirection.z) * Mathf.Rad2Deg;
                context.HeadYawDeg = lockedYaw;
                context.SetHeadRotation(context.HeadYawDeg);

                // 锁定冲刺阶段：强烈震动，制造巨物压迫感
                if (context.CurrentTick % 7 == 0)
                    TryShakeCamera(0.60f);

                context.ApplyMovementAndVisuals(
                    moveSpeed,
                    maxTurn,
                    ChargeTurnBlend,
                    0.01f,
                    SandWormThing.HeightOffset,
                    SandWormThing.BankMaxDeg * 1.8f,
                    SandWormThing.ViewTiltXMaxDeg,
                    SandWormThing.BankLerp,
                    2);
                return;
            }

            context.Blackboard.ChargeLocked = false;
            context.Blackboard.ChargeDirection = Vector3.zero;
            context.ApplyMovementAndVisuals(
                moveSpeed,
                maxTurn,
                ChargeTurnBlend,
                0f,
                SandWormThing.HeightOffset,
                SandWormThing.BankMaxDeg,
                SandWormThing.ViewTiltXMaxDeg,
                SandWormThing.BankLerp,
                4);
        }

        private static void TryShakeCamera(float magnitude)
        {
            if (Current.ProgramState != ProgramState.Playing)
                return;
            Find.CameraDriver?.shaker?.DoShake(magnitude);
        }

        public override SandWormBehaviorTransition EvaluateTransition(SandWormBehaviorContext context)
        {
            int elapsed = context.CurrentTick - enterTick;
            if (elapsed < MinChargeCommitTicks)
            {
                return SandWormBehaviorTransition.None;
            }

            if (context.IsOutsideMapBounds())
            {
                return SandWormBehaviorTransition.None;
            }

            if (elapsed >= MaxChargeDurationTicks
                || !context.HasValidChargeTarget(ChargeDistance)
                || WillLeaveMapSoon(context))
            {
                context.Blackboard.CurrentTarget = null;
                context.Blackboard.ChargeLocked = false;
                context.Blackboard.ChargeDirection = Vector3.zero;
                int cooldownTicks = Mathf.Max(60, Mathf.RoundToInt(ChargeCooldownTicks * context.ChargeCooldownFactor));
                context.Blackboard.LastChargeTargetAcquireTick = context.CurrentTick + cooldownTicks;
                return new SandWormBehaviorTransition(SandWormBehaviorIds.Wander);
            }

            return SandWormBehaviorTransition.None;
        }

        private static bool WillLeaveMapSoon(SandWormBehaviorContext context)
        {
            if (context.Map == null || !context.Blackboard.ChargeLocked)
            {
                return false;
            }

            Vector3 direction = context.Blackboard.ChargeDirection;
            if (direction.sqrMagnitude < 0.001f)
            {
                return false;
            }

            direction.Normalize();
            // 大虫看得更远，小虫看得更近
            float lookahead = WillLeaveLookaheadBase * context.MoveSpeedScale;
            Vector3 futurePos = context.ExactPos + direction * lookahead;
            IntVec3 futureCell = futurePos.ToIntVec3();
            if (!futureCell.InBounds(context.Map))
            {
                return true;
            }

            return futureCell.x < ChargeBoundaryMargin
                || futureCell.z < ChargeBoundaryMargin
                || futureCell.x >= context.Map.Size.x - ChargeBoundaryMargin
                || futureCell.z >= context.Map.Size.z - ChargeBoundaryMargin;
        }
    }
}
