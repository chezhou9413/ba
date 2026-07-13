using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace SandWormLib
{
    // SandWormBehaviorContext 负责把行为状态机需要的沙虫数据、移动控制和挑战交互统一转发给拥有者。
    public sealed class SandWormBehaviorContext
    {
        private readonly SandWormThing owner;

        // SandWormBehaviorContext 负责绑定当前行为上下文对应的沙虫实体。
        public SandWormBehaviorContext(SandWormThing owner)
        {
            this.owner = owner;
        }

        public SandWormThing Owner => owner;

        public SandWormBlackboard Blackboard => owner.Blackboard;

        public Map Map => owner.Map;

        public int CurrentTick => Find.TickManager.TicksGame;

        public Vector3 ExactPos
        {
            get => owner.ExactPos;
            set => owner.ExactPos = value;
        }

        public Vector3 Velocity
        {
            get => owner.Velocity;
            set => owner.Velocity = value;
        }

        public float HeadYawDeg
        {
            get => owner.HeadYawDeg;
            set => owner.HeadYawDeg = value;
        }

        public float NoiseOffset => owner.NoiseOffset;

        public float NoiseTime
        {
            get => owner.NoiseTime;
            set => owner.NoiseTime = value;
        }

        public Vector3 MapCenter => owner.MapCenter;

        public float MapRadius => owner.MapRadius;

        // 当前沙虫的尺寸倍数（1.0 = 标准）。
        public float SizeMultiplier => owner.SizeMultiplier;

        // 转向缩放：大虫转向慢，按 1/sqrt(size) 缩放；小虫转向快。
        // 移动速度缩放：大虫迈步大，按 size 线性缩放。
        public float MoveSpeedScale => owner.MoveSpeedScale;
        public float TurnRateScale => 1f / Mathf.Sqrt(Mathf.Max(0.1f, owner.SizeMultiplier));
        public float ChargeCooldownFactor => owner.ChargeCooldownFactor;
        public bool ChallengeShockwaveEnabled => owner.ChallengeShockwaveEnabled;

        public bool HasHeadTransform => owner.HeadTransform != null;

        public bool SurfacePressureEnabled
        {
            get => owner.SurfacePressureEnabled;
            set => owner.SurfacePressureEnabled = value;
        }

        public bool IsOutsideMapBounds(float margin = 0f)
        {
            return owner.IsOutsideMapBounds(margin);
        }

        public Vector3 GetRecoveryTarget(float inset)
        {
            return owner.GetRecoveryTarget(inset);
        }

        public float AngleTo(Vector3 worldTarget)
        {
            return owner.GetSignedAngleTo(worldTarget);
        }

        public Thing FindPreferredChargeTarget(float maxDistance)
        {
            return owner.FindPreferredChargeTarget(maxDistance);
        }

        public bool IsValidChargeTarget(Thing target, float maxDistance)
        {
            return owner.IsValidChargeTarget(target, maxDistance);
        }

        public bool HasValidChargeTarget(float maxDistance)
        {
            return owner.IsValidChargeTarget(Blackboard.CurrentTarget, maxDistance);
        }

        public void SetHeadRotation(float yawDeg)
        {
            owner.SetHeadRotation(yawDeg);
        }

        public bool CanStartShockwaveAttack()
        {
            SandWormSyndicateChallengeState state = Current.Game?.GetComponent<SandWormSyndicateChallengeState>();
            return state != null && state.CanStartShockwave(owner);
        }

        public void NotifyShockwaveAttackFinished()
        {
            Current.Game?.GetComponent<SandWormSyndicateChallengeState>()?.NotifyShockwaveFinished(owner);
        }

        // CollectShockwaveSourceCells 负责从沙虫拥有者采样全身冲击波源点。
        public void CollectShockwaveSourceCells(List<IntVec3> outCells, int maxCount)
        {
            owner.CollectShockwaveSourceCells(outCells, maxCount);
        }

        // PerformShockwaveAttack 负责让挑战状态按全身源点启动冲击波预警和飞石掩体。
        public void PerformShockwaveAttack(List<IntVec3> sourceCells)
        {
            Current.Game?.GetComponent<SandWormSyndicateChallengeState>()?.PerformShockwaveAttack(owner, sourceCells);
        }

        // ResolveShockwaveDamage 负责让挑战状态在预警结束后结算冲击波伤害。
        public void ResolveShockwaveDamage(List<IntVec3> sourceCells)
        {
            Current.Game?.GetComponent<SandWormSyndicateChallengeState>()?.ResolveShockwaveDamage(owner, sourceCells);
        }

        // DrawShockwaveWarningFlecks 负责让挑战状态在预警阶段播放掩体和源点特效。
        public void DrawShockwaveWarningFlecks(List<IntVec3> sourceCells)
        {
            Current.Game?.GetComponent<SandWormSyndicateChallengeState>()?.DrawShockwaveWarningFlecks(owner, sourceCells);
        }

        public float GetReturnToCenterTurn(float thresholdFactor, float returnMargin, float returnForce)
        {
            return owner.GetReturnToCenterTurn(thresholdFactor, returnMargin, returnForce);
        }

        public float GetBiasTowardPlayerTurn(float biasForce, float maxBiasTurn)
        {
            return owner.GetBiasTowardPlayerTurn(biasForce, maxBiasTurn);
        }

        public void ApplyMovementAndVisuals(
            float moveSpeed,
            float maxTurnPerTick,
            float turnBlend,
            float totalTurn,
            float heightOffset,
            float bankMaxDeg,
            float viewTiltXMaxDeg,
            float bankLerp,
            int positionSyncIntervalTicks)
        {
            owner.ApplyMovementAndVisuals(
                moveSpeed,
                maxTurnPerTick,
                turnBlend,
                totalTurn,
                heightOffset,
                bankMaxDeg,
                viewTiltXMaxDeg,
                bankLerp,
                positionSyncIntervalTicks);
        }
    }
}
