using UnityEngine;
using SandWormLib.SandWormBehavior;
using Verse;

namespace SandWormLib
{
    public sealed class SandWormWanderBehavior : SandWormBehaviorBase
    {
        private const float BaseMoveSpeed = 0.042f;
        private const float BaseMaxTurnPerTick = 0.12f;
        private const float NoiseSpeed = 0.004f;
        private const float TurnBlend = 0.08f;
        private const float ReturnMargin = 18f;
        private const float ReturnForce = 0.28f;
        private const float ReturnThresholdFactor = 0.3f;
        private const float BiasTowardPlayerForce = 0.006f;
        private const float BiasTowardPlayerMaxTurn = 0.04f;
        private const int PositionSyncIntervalTicks = 10;
        private const int MinWanderBeforeChargeTicks = 240;

        public SandWormWanderBehavior()
            : base(new SandWormBehaviorDef
            {
                Id = SandWormBehaviorIds.Wander,
                EnableSurfacePressure = true,
                DrawPath = true
            })
        {
        }

        public override void Enter(SandWormBehaviorContext context)
        {
        }

        public override void Tick(SandWormBehaviorContext context)
        {
            if (!context.HasHeadTransform)
            {
                return;
            }

            context.NoiseTime += NoiseSpeed;

            float moveSpeed = BaseMoveSpeed * context.MoveSpeedScale;
            float maxTurnPerTick = BaseMaxTurnPerTick * context.TurnRateScale;

            float noiseTurn =
                (Mathf.PerlinNoise(context.NoiseTime + context.NoiseOffset, 0f) - 0.5f) * 2f * maxTurnPerTick;
            float returnTurn = context.GetReturnToCenterTurn(ReturnThresholdFactor, ReturnMargin, ReturnForce);
            float biasTurn = context.GetBiasTowardPlayerTurn(BiasTowardPlayerForce, BiasTowardPlayerMaxTurn);
            float totalTurn = noiseTurn + returnTurn + biasTurn;

            context.HeadYawDeg += totalTurn;
            context.SetHeadRotation(context.HeadYawDeg);

            context.ApplyMovementAndVisuals(
                moveSpeed,
                maxTurnPerTick,
                TurnBlend,
                totalTurn,
                SandWormThing.HeightOffset,
                SandWormThing.BankMaxDeg,
                SandWormThing.ViewTiltXMaxDeg,
                SandWormThing.BankLerp,
                PositionSyncIntervalTicks);
        }

        public override SandWormBehaviorTransition EvaluateTransition(SandWormBehaviorContext context)
        {
            int ticksInBehavior = context.CurrentTick - context.Blackboard.LastBehaviorChangeTick;
            if (ticksInBehavior >= MinWanderBeforeChargeTicks)
            {
                if (context.ChallengeShockwaveEnabled && context.CanStartShockwaveAttack())
                {
                    context.Blackboard.CurrentTarget = null;
                    context.Blackboard.ChargeLocked = false;
                    context.Blackboard.ChargeDirection = Vector3.zero;
                    return new SandWormBehaviorTransition(SandWormBehaviorIds.Shockwave);
                }

                Thing target = context.FindPreferredChargeTarget(float.MaxValue);
                if (target != null && context.CurrentTick >= context.Blackboard.LastChargeTargetAcquireTick)
                {
                    context.Blackboard.CurrentTarget = target;
                    context.Blackboard.ChargeLocked = false;
                    context.Blackboard.ChargeDirection = Vector3.zero;
                    return new SandWormBehaviorTransition(SandWormBehaviorIds.Charge);
                }
            }

            return SandWormBehaviorTransition.None;
        }

        public override void Exit(SandWormBehaviorContext context)
        {
        }
    }
}
