using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormShockwaveBehavior 负责让辛迪加挑战大沙虫进入蓄力冲击波状态、生成预警并结算伤害。
    public sealed class SandWormShockwaveBehavior : SandWormBehavior.SandWormBehaviorBase
    {
        private const float BaseSlowMoveSpeed = 0.0084f;
        private const float SafeMaxTurnPerTick = 0.01f;
        private const float TurnBlend = 0.08f;
        private const int WarningTicks = 480;
        private const int TotalTicks = 720;
        private const int WarningFleckIntervalTicks = 45;
        private const int MaxShockwaveSourceCells = 14;

        private int enterTick;
        private bool damageResolved;
        private bool finishNotified;
        private readonly List<IntVec3> sourceCells = new List<IntVec3>(MaxShockwaveSourceCells);

        public SandWormShockwaveBehavior()
            : base(new SandWormBehavior.SandWormBehaviorDef
            {
                Id = SandWormBehaviorIds.Shockwave,
                EnableSurfacePressure = true,
                DrawPath = true
            })
        {
        }

        // Enter 负责锁定沙虫朝向、清理冲锋状态，并请求挑战状态生成冲击波掩体。
        public override void Enter(SandWormBehaviorContext context)
        {
            enterTick = context.CurrentTick;
            damageResolved = false;
            finishNotified = false;
            sourceCells.Clear();
            context.CollectShockwaveSourceCells(sourceCells, MaxShockwaveSourceCells);
            context.Blackboard.CurrentTarget = null;
            context.Blackboard.ChargeLocked = false;
            context.Blackboard.ChargeDirection = Vector3.zero;
            context.PerformShockwaveAttack(sourceCells);
        }

        // Tick 负责维持慢速直行、播放预警特效，并在预警结束时只结算一次伤害。
        public override void Tick(SandWormBehaviorContext context)
        {
            if (!context.HasHeadTransform)
            {
                return;
            }

            int elapsed = context.CurrentTick - enterTick;

            if (elapsed < WarningTicks && elapsed % WarningFleckIntervalTicks == 0)
            {
                context.DrawShockwaveWarningFlecks(sourceCells);
            }

            if (!damageResolved && elapsed >= WarningTicks)
            {
                damageResolved = true;
                context.ResolveShockwaveDamage(sourceCells);
            }

            context.ApplyMovementAndVisuals(
                BaseSlowMoveSpeed * context.MoveSpeedScale,
                SafeMaxTurnPerTick,
                TurnBlend,
                0f,
                SandWormThing.HeightOffset,
                0f,
                0f,
                SandWormThing.BankLerp,
                10);
        }

        // EvaluateTransition 负责在冲击波释放表现结束后返回普通游荡状态。
        public override SandWormBehavior.SandWormBehaviorTransition EvaluateTransition(SandWormBehaviorContext context)
        {
            if (context.CurrentTick - enterTick < TotalTicks)
            {
                return SandWormBehavior.SandWormBehaviorTransition.None;
            }

            NotifyFinishedOnce(context);
            return new SandWormBehavior.SandWormBehaviorTransition(SandWormBehaviorIds.Wander);
        }

        // Exit 负责在状态被外部打断时也写入冷却，避免连续重复触发。
        public override void Exit(SandWormBehaviorContext context)
        {
            NotifyFinishedOnce(context);
        }

        // NotifyFinishedOnce 负责保证一次冲击波状态只通知一次冷却完成。
        private void NotifyFinishedOnce(SandWormBehaviorContext context)
        {
            if (finishNotified)
            {
                return;
            }

            finishNotified = true;
            context.NotifyShockwaveAttackFinished();
        }

    }
}
