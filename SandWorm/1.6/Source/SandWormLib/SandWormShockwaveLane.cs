using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormShockwaveLane 负责保存一次地脉冲击预警带的方向、宽度和命中判定数据。
    public sealed class SandWormShockwaveLane : IExposable
    {
        private const float EndMargin = 8f;
        private const float MinimumLength = 12f;

        private IntVec3 sourceCell;
        private IntVec3 aimCell;
        private float directionX;
        private float directionZ;
        private float sideX;
        private float sideZ;
        private float length;
        private float halfWidth;

        // SandWormShockwaveLane 负责让 Scribe 在读档时创建空实例。
        public SandWormShockwaveLane()
        {
        }

        // SandWormShockwaveLane 负责根据沙虫源点和瞄准格创建延伸到地图边缘的危险带。
        public SandWormShockwaveLane(Map map, IntVec3 sourceCell, IntVec3 aimCell, float halfWidth)
        {
            this.sourceCell = sourceCell;
            this.aimCell = aimCell;
            this.halfWidth = Mathf.Max(0.5f, halfWidth);
            BuildDirectionAndLength(map);
        }

        // SourceCell 负责提供危险带从哪个沙虫身体源点释放。
        public IntVec3 SourceCell => sourceCell;

        // AimCell 负责提供危险带锁定时参考的小人位置。
        public IntVec3 AimCell => aimCell;

        // HalfWidth 负责提供危险带中心线两侧的判定宽度。
        public float HalfWidth => halfWidth;

        // Start 负责提供危险带起点的世界坐标。
        public Vector3 Start => sourceCell.ToVector3Shifted();

        // End 负责提供危险带终点的世界坐标。
        public Vector3 End => Start + Direction * length;

        // Direction 负责提供从源点指向地图边缘的水平单位方向。
        public Vector3 Direction => new Vector3(directionX, 0f, directionZ);

        // Side 负责提供危险带横向宽度使用的水平单位方向。
        public Vector3 Side => new Vector3(sideX, 0f, sideZ);

        // ContainsCell 负责判断指定格是否落在危险带范围内。
        public bool ContainsCell(IntVec3 cell)
        {
            if (!sourceCell.IsValid || !cell.IsValid || length <= 0f)
            {
                return false;
            }

            Vector3 offset = cell.ToVector3Shifted() - Start;
            offset.y = 0f;
            float along = Vector3.Dot(offset, Direction);
            if (along < 1.0f || along > length)
            {
                return false;
            }

            return Mathf.Abs(Vector3.Dot(offset, Side)) <= halfWidth;
        }

        // LateralFraction 负责计算目标距离中心线的归一化横向偏移，用于区分正中命中和擦边命中。
        public float LateralFraction(IntVec3 cell)
        {
            if (!cell.IsValid || halfWidth <= 0f)
            {
                return 1f;
            }

            Vector3 offset = cell.ToVector3Shifted() - Start;
            offset.y = 0f;
            return Mathf.Clamp01(Mathf.Abs(Vector3.Dot(offset, Side)) / halfWidth);
        }

        // ExposeData 负责保存危险带的几何参数，保证预警期间读档后仍能继续显示和结算。
        public void ExposeData()
        {
            Scribe_Values.Look(ref sourceCell, "sourceCell");
            Scribe_Values.Look(ref aimCell, "aimCell");
            Scribe_Values.Look(ref directionX, "directionX", 0f);
            Scribe_Values.Look(ref directionZ, "directionZ", 1f);
            Scribe_Values.Look(ref sideX, "sideX", -1f);
            Scribe_Values.Look(ref sideZ, "sideZ", 0f);
            Scribe_Values.Look(ref length, "length", MinimumLength);
            Scribe_Values.Look(ref halfWidth, "halfWidth", 3f);
        }

        // BuildDirectionAndLength 负责把源点到瞄准格的方向延伸到地图边缘。
        private void BuildDirectionAndLength(Map map)
        {
            Vector3 source = sourceCell.ToVector3Shifted();
            Vector3 aim = aimCell.ToVector3Shifted();
            Vector3 direction = aim - source;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            direction.Normalize();
            Vector3 side = new Vector3(-direction.z, 0f, direction.x);
            directionX = direction.x;
            directionZ = direction.z;
            sideX = side.x;
            sideZ = side.z;

            float aimDistance = Mathf.Max(MinimumLength, (aim - source).MagnitudeHorizontal() + EndMargin);
            length = Mathf.Max(aimDistance, DistanceToMapEdge(map, source, direction));
        }

        // DistanceToMapEdge 负责计算危险带沿当前方向穿过地图所需的距离。
        private static float DistanceToMapEdge(Map map, Vector3 source, Vector3 direction)
        {
            if (map == null)
            {
                return MinimumLength;
            }

            float best = float.MaxValue;
            UpdateDistance(ref best, source.x, direction.x, 2f);
            UpdateDistance(ref best, source.x, direction.x, map.Size.x - 3f);
            UpdateDistance(ref best, source.z, direction.z, 2f);
            UpdateDistance(ref best, source.z, direction.z, map.Size.z - 3f);

            if (best == float.MaxValue)
            {
                return Mathf.Max(map.Size.x, map.Size.z);
            }

            return Mathf.Clamp(best, MinimumLength, Mathf.Max(map.Size.x, map.Size.z) * 1.5f);
        }

        // UpdateDistance 负责把当前方向与一条地图边界的正向交点写入最短距离。
        private static void UpdateDistance(ref float best, float current, float direction, float boundary)
        {
            if (Mathf.Abs(direction) < 0.001f)
            {
                return;
            }

            float distance = (boundary - current) / direction;
            if (distance > 0f && distance < best)
            {
                best = distance;
            }
        }
    }
}
