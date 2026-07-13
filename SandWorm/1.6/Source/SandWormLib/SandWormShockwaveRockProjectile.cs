using RimWorld;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormShockwaveRockProjectile 负责保存一块冲击波飞石的贝塞尔轨迹、绘制表现和落地状态。
    public sealed class SandWormShockwaveRockProjectile : IExposable
    {
        private const string RockTexturePath = "Things/Mote/StoneBit";
        private const float DustScaleFactor = 2.5f;
        private static readonly Color SmokeTrailColor = new Color(0.50f, 0.45f, 0.38f, 0.88f);
        private static Material rockMaterial;

        private IntVec3 sourceCell;
        private IntVec3 targetCell;
        private Vector3 start;
        private Vector3 controlA;
        private Vector3 controlB;
        private Vector3 end;
        private int startTick;
        private int durationTicks;
        private float rotationSeed;
        private float rotationSpeed;
        private float scale;
        private bool landed;

        // SandWormShockwaveRockProjectile 负责让 Scribe 在读档时创建空实例。
        public SandWormShockwaveRockProjectile()
        {
        }

        // SandWormShockwaveRockProjectile 负责根据源点、目标格和随机外观参数创建一条飞石轨迹。
        public SandWormShockwaveRockProjectile(IntVec3 sourceCell, IntVec3 targetCell, int startTick, int durationTicks, float rotationSeed, float rotationSpeed, float scale)
        {
            this.sourceCell = sourceCell;
            this.targetCell = targetCell;
            this.startTick = startTick;
            this.durationTicks = Mathf.Max(1, durationTicks);
            this.rotationSeed = rotationSeed;
            this.rotationSpeed = rotationSpeed;
            this.scale = Mathf.Max(0.35f, scale);
            landed = false;
            BuildPath();
        }

        // SourceCell 负责提供飞石起飞时对应的沙虫身体源点。
        public IntVec3 SourceCell => sourceCell;

        // TargetCell 负责提供飞石最终砸落并尝试生成墙体的目标格。
        public IntVec3 TargetCell => targetCell;

        // Landed 负责向挑战状态报告飞石是否已经完成落地结算。
        public bool Landed => landed;

        // MarkLanded 负责标记飞石已经完成落地结算，避免重复生成墙体。
        public void MarkLanded()
        {
            landed = true;
        }

        // ReadyToLand 负责判断飞石是否已经到达目标格，可以进入落地结算。
        public bool ReadyToLand(int tick)
        {
            return !landed && tick - startTick >= durationTicks;
        }

        // PositionAt 负责按当前 tick 计算三次贝塞尔曲线上的世界坐标。
        public Vector3 PositionAt(int tick)
        {
            float progress = Mathf.Clamp01((tick - startTick) / (float)Mathf.Max(1, durationTicks));
            float easedProgress = progress * progress * (3f - 2f * progress);
            float inv = 1f - progress;
            Vector3 position =
                inv * inv * inv * start
                + 3f * inv * inv * progress * controlA
                + 3f * inv * progress * progress * controlB
                + progress * progress * progress * end;
            position.y += Mathf.Sin(easedProgress * Mathf.PI) * 0.9f;
            return position;
        }

        // Draw 负责绘制飞石本体、烟尘拖尾和飞行尘土。
        public void Draw(Map map, int tick)
        {
            if (landed || map == null || tick < startTick)
            {
                return;
            }

            Vector3 position = PositionAt(tick);
            IntVec3 cell = position.ToIntVec3();
            if (!cell.InBounds(map) || cell.Fogged(map))
            {
                return;
            }

            float progress = Mathf.Clamp01((tick - startTick) / (float)Mathf.Max(1, durationTicks));
            float visualScale = scale * Mathf.Lerp(0.95f, 1.55f, Mathf.Sin(progress * Mathf.PI));
            Quaternion rotation = Quaternion.AngleAxis(rotationSeed + rotationSpeed * (tick - startTick), Vector3.up);
            Vector3 drawScale = new Vector3(visualScale, 1f, visualScale);
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(position, rotation, drawScale), RockMaterial, 0);

            if ((tick + targetCell.x + targetCell.z) % 7 == 0)
            {
                FleckMaker.ThrowDustPuff(position + Gen.RandomHorizontalVector(0.50f), map, Rand.Range(0.80f, 1.20f) * DustScaleFactor);
            }

            DrawSmokeTrail(map, tick, visualScale);
        }

        // ExposeData 负责保存飞石的轨迹参数和落地状态，确保读档后可以继续飞行或正确清理。
        public void ExposeData()
        {
            Scribe_Values.Look(ref sourceCell, "sourceCell");
            Scribe_Values.Look(ref targetCell, "targetCell");
            Scribe_Values.Look(ref start, "start");
            Scribe_Values.Look(ref controlA, "controlA");
            Scribe_Values.Look(ref controlB, "controlB");
            Scribe_Values.Look(ref end, "end");
            Scribe_Values.Look(ref startTick, "startTick", 0);
            Scribe_Values.Look(ref durationTicks, "durationTicks", 1);
            Scribe_Values.Look(ref rotationSeed, "rotationSeed", 0f);
            Scribe_Values.Look(ref rotationSpeed, "rotationSpeed", 0f);
            Scribe_Values.Look(ref scale, "scale", 1f);
            Scribe_Values.Look(ref landed, "landed", defaultValue: false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                durationTicks = Mathf.Max(1, durationTicks);
                scale = Mathf.Max(0.35f, scale);
            }
        }

        // BuildPath 负责根据源点和落点构造一条带侧向弧度的贝塞尔飞行曲线。
        private void BuildPath()
        {
            Vector3 source = sourceCell.ToVector3Shifted();
            Vector3 target = targetCell.ToVector3Shifted();
            Vector3 forward = target - source;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }

            float distance = forward.magnitude;
            forward.Normalize();
            Vector3 side = new Vector3(-forward.z, 0f, forward.x);
            float sideOffset = Rand.Range(-2.4f, 2.4f);
            float lift = Mathf.Clamp(distance * 0.22f, 3.2f, 7.5f);

            start = source + Gen.RandomHorizontalVector(1.2f);
            end = target;
            controlA = Vector3.Lerp(source, target, 0.22f) + side * sideOffset + Vector3.up * lift;
            controlB = Vector3.Lerp(source, target, 0.76f) - side * sideOffset * 0.45f + Vector3.up * (lift * 0.42f);

            start.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            controlA.y = AltitudeLayer.MoteOverhead.AltitudeFor() + lift;
            controlB.y = AltitudeLayer.MoteOverhead.AltitudeFor() + lift * 0.55f;
            end.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        }

        // DrawSmokeTrail 负责用烟尘点列表现飞石拖尾，避免线段材质产生纸片感。
        private void DrawSmokeTrail(Map map, int tick, float visualScale)
        {
            if ((tick + targetCell.x * 3 + targetCell.z) % 3 != 0)
            {
                return;
            }

            float smokeScaleBase = Mathf.Max(0.80f, visualScale * 0.48f);
            for (int i = 1; i <= 4; i++)
            {
                Vector3 point = PositionAt(tick - i * 8);
                IntVec3 cell = point.ToIntVec3();
                if (!cell.InBounds(map) || cell.Fogged(map))
                {
                    continue;
                }

                // 越靠近飞石本体的烟越厚，越靠后的烟越散，形成沉重的抛射轨迹。
                float ageFactor = i / 4f;
                float scaleFactor = Mathf.Lerp(1.25f, 0.46f, ageFactor);
                Vector3 jitteredPoint = point + Gen.RandomHorizontalVector(0.24f + i * 0.10f);
                FleckMaker.ThrowDustPuffThick(jitteredPoint, map, smokeScaleBase * scaleFactor * DustScaleFactor, SmokeTrailColor);

                if (i <= 2 && Rand.Chance(0.55f))
                {
                    FleckMaker.ThrowDustPuff(jitteredPoint + Gen.RandomHorizontalVector(0.24f), map, smokeScaleBase * scaleFactor * 0.72f * DustScaleFactor);
                }
            }
        }

        // RockMaterial 负责缓存 Core 碎石贴图材质，避免每帧重复创建。
        private static Material RockMaterial
        {
            get
            {
                if (rockMaterial == null)
                {
                    rockMaterial = MaterialPool.MatFrom(RockTexturePath, ShaderDatabase.Transparent, Color.white);
                }

                return rockMaterial;
            }
        }

    }
}
