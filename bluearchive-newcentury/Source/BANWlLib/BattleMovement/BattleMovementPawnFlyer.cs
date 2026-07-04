using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleMovement
{
    // 战斗位移飞行器，负责复用原版 PawnFlyer 容器和落地流程，并按技能配置绘制平面或跳跃位移。
    public class BattleMovementPawnFlyer : PawnFlyer
    {
        private float battleMoveHeight = 2.5f;
        private float battleMoveSpeed = 0.25f;
        private Action onBattleMovementCompleted;

        // 当前飞行进度，负责把原版飞行 tick 转换成 0 到 1 的插值比例。
        private float Progress
        {
            get
            {
                return Mathf.Clamp01((float)ticksFlying / Mathf.Max(1, ticksFlightTime));
            }
        }

        // 当前地面位置，负责按原版进度曲线计算水平位移。
        private Vector3 GroundPosition
        {
            get
            {
                float progress = def.pawnFlyer.Worker.AdjustedProgress(Progress);
                return Vector3.Lerp(startVec, DestinationPos, progress);
            }
        }

        // 当前高度，负责按技能配置的最大高度生成抛物线弧度，平面冲刺传入 0 时保持贴地移动。
        private float BattleHeight
        {
            get
            {
                return GenMath.InverseParabola(Progress) * Mathf.Max(0f, battleMoveHeight);
            }
        }

        // 绘制位置，负责把水平位移和配置高度合成最终小人显示位置。
        public override Vector3 DrawPos
        {
            get
            {
                float height = BattleHeight;
                return GroundPosition + Altitudes.AltIncVect * height + Vector3.forward * height;
            }
        }

        // 创建战斗位移飞行器，负责走原版 MakeFlyer 并注入战斗位移专用配置。
        public static BattleMovementPawnFlyer MakeBattleFlyer(ThingDef flyerDef, Pawn pawn, IntVec3 destination, float height, float speed, Action onCompleted)
        {
            BattleMovementPawnFlyer flyer = PawnFlyer.MakeFlyer(flyerDef, pawn, destination, null, null) as BattleMovementPawnFlyer;
            if (flyer == null)
            {
                Log.Error("[BANW] 战斗位移飞行器 Def " + flyerDef.defName + " 的 thingClass 不是 BattleMovementPawnFlyer。");
                return null;
            }

            flyer.battleMoveHeight = Mathf.Max(0f, height);
            flyer.battleMoveSpeed = Mathf.Max(0.05f, speed);
            flyer.onBattleMovementCompleted = onCompleted;
            return flyer;
        }

        // 生成飞行器，负责复用原版初始化后按技能速度覆盖飞行持续时间。
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                float distance = Mathf.Max(1f, startVec.ToIntVec3().DistanceTo(DestinationPos.ToIntVec3()));
                ticksFlightTime = Mathf.Max(1, Mathf.CeilToInt(distance / Mathf.Max(0.05f, battleMoveSpeed)));
            }
        }

        // 落地复原 Pawn，负责先执行原版落地流程，再通知战斗位移组件结算完成效果。
        protected override void RespawnPawn()
        {
            base.RespawnPawn();
            onBattleMovementCompleted?.Invoke();
            onBattleMovementCompleted = null;
        }

        // 动态绘制阶段，负责让飞行中的 Pawn 按自定义平面或弧线位置绘制。
        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            if (FlyingPawn != null)
            {
                FlyingPawn.DynamicDrawPhaseAt(phase, DrawPos);
            }
            else
            {
                FlyingThing?.DynamicDrawPhaseAt(phase, DrawPos);
            }

            if (phase == DrawPhase.Draw)
            {
                DrawAt(drawLoc, flip);
            }
        }

        // 绘制飞行附加内容，负责按自定义高度显示阴影和携带物。
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            DrawBattleShadow(GroundPosition, BattleHeight);
            if (CarriedThing != null && FlyingPawn != null)
            {
                PawnRenderUtility.DrawCarriedThing(FlyingPawn, DrawPos, CarriedThing);
            }
        }

        // 绘制跳跃阴影，负责用原版 PawnFlyer 阴影材质随高度缩放。
        private void DrawBattleShadow(Vector3 drawLoc, float height)
        {
            Material shadowMaterial = def.pawnFlyer.ShadowMaterial;
            if (shadowMaterial == null)
            {
                return;
            }

            float scale = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(height / Mathf.Max(1f, battleMoveHeight)));
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawLoc, Quaternion.identity, new Vector3(scale, 1f, scale));
            Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
        }

        // 保存和读取飞行器状态，负责让飞行中的位移在存档读档后保留高度和速度。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref battleMoveHeight, "battleMoveHeight", 2.5f);
            Scribe_Values.Look(ref battleMoveSpeed, "battleMoveSpeed", 0.25f);
        }
    }
}
