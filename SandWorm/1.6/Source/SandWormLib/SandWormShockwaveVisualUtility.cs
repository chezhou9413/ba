using RimWorld;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormShockwaveVisualUtility 负责集中绘制地脉冲击的危险带、源点锁定、释放波前和尘土反馈。
    public static class SandWormShockwaveVisualUtility
    {
        private const float LaneFillAlphaWidthFactor = 2.05f;
        private const float LaneInnerWidth = 0.72f;
        private const float LaneEdgeInset = 0.34f;
        private const float LaneRibSpacing = 4.8f;
        private const float LaneRibLength = 0.72f;
        private const float ChevronLength = 1.10f;
        private const float ChevronHalfWidth = 0.46f;
        private const int MaxLaneChevrons = 14;
        private const int MaxPawnChevrons = 6;

        // DrawWarningLane 负责绘制一条带危险区域填充、裂隙中心线、边界刻度和流动箭头的地脉预警带。
        public static void DrawWarningLane(SandWormShockwaveLane lane, int ticksLeft, int totalTicks)
        {
            if (lane == null)
            {
                return;
            }

            Vector3 start = lane.Start;
            Vector3 end = lane.End;
            Vector3 direction = lane.Direction;
            Vector3 side = lane.Side;
            if (direction.sqrMagnitude < 0.001f || side.sqrMagnitude < 0.001f)
            {
                return;
            }

            float altitude = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.018f;
            start.y = altitude;
            end.y = altitude;
            float distance = (end - start).MagnitudeHorizontal();
            if (distance < 1.2f)
            {
                return;
            }

            float charge = ShockwaveCharge(ticksLeft, totalTicks);
            float pulse = 0.5f + 0.5f * Mathf.Sin(Find.TickManager.TicksGame * 0.11f);
            float halfWidth = lane.HalfWidth;
            Vector3 leftStart = WithAltitude(start + side * halfWidth, altitude);
            Vector3 leftEnd = WithAltitude(end + side * halfWidth, altitude);
            Vector3 rightStart = WithAltitude(start - side * halfWidth, altitude);
            Vector3 rightEnd = WithAltitude(end - side * halfWidth, altitude);
            Vector3 leftInnerStart = WithAltitude(start + side * Mathf.Max(0.2f, halfWidth - LaneEdgeInset), altitude);
            Vector3 leftInnerEnd = WithAltitude(end + side * Mathf.Max(0.2f, halfWidth - LaneEdgeInset), altitude);
            Vector3 rightInnerStart = WithAltitude(start - side * Mathf.Max(0.2f, halfWidth - LaneEdgeInset), altitude);
            Vector3 rightInnerEnd = WithAltitude(end - side * Mathf.Max(0.2f, halfWidth - LaneEdgeInset), altitude);

            GenDraw.DrawLineBetween(start, end, SandWormShockwaveVisualMaterials.LaneFill, Mathf.Max(1.2f, halfWidth * LaneFillAlphaWidthFactor));
            GenDraw.DrawLineBetween(start, end, SandWormShockwaveVisualMaterials.LaneDarkCrack, LaneInnerWidth + charge * 0.30f);
            GenDraw.DrawLineBetween(start, end, SandWormShockwaveVisualMaterials.LaneCore, 0.20f + pulse * 0.08f + charge * 0.08f);
            GenDraw.DrawLineBetween(leftStart, leftEnd, SandWormShockwaveVisualMaterials.LaneEdge, 0.24f + charge * 0.08f);
            GenDraw.DrawLineBetween(rightStart, rightEnd, SandWormShockwaveVisualMaterials.LaneEdge, 0.24f + charge * 0.08f);
            GenDraw.DrawLineBetween(leftInnerStart, leftInnerEnd, SandWormShockwaveVisualMaterials.LaneEdge, 0.08f);
            GenDraw.DrawLineBetween(rightInnerStart, rightInnerEnd, SandWormShockwaveVisualMaterials.LaneEdge, 0.08f);

            DrawLaneRibs(start, direction, side, distance, halfWidth, altitude, charge);
            DrawMovingChevrons(start, direction, side, distance, altitude, SandWormShockwaveVisualMaterials.LaneChevron, MaxLaneChevrons, 0.24f + charge * 0.05f, 0.46f);
            DrawSourceLock(start, side, direction, halfWidth, altitude, charge);
        }

        // DrawPawnThreatLink 负责绘制源点到小人的状态连线，危险时呈热色，受掩体保护时呈安全色。
        public static void DrawPawnThreatLink(Vector3 start, Vector3 end, bool covered)
        {
            Vector3 direction = end - start;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            direction.Normalize();
            float altitude = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.04f;
            start.y = altitude;
            end.y = altitude;
            float distance = (end - start).MagnitudeHorizontal();
            Material lineMaterial = covered ? SandWormShockwaveVisualMaterials.SafeLine : SandWormShockwaveVisualMaterials.LaneCore;
            Material fillMaterial = covered ? SandWormShockwaveVisualMaterials.SafeFill : SandWormShockwaveVisualMaterials.LaneFill;
            GenDraw.DrawLineBetween(start, end, fillMaterial, covered ? 0.62f : 0.52f);
            GenDraw.DrawLineBetween(start, end, lineMaterial, covered ? 0.16f : 0.20f);
            DrawMovingChevrons(start, direction, new Vector3(-direction.z, 0f, direction.x), distance, altitude, lineMaterial, MaxPawnChevrons, 0.16f, 0.35f);
            DrawEndpointMarker(end, covered ? SandWormShockwaveVisualMaterials.SafeLine : SandWormShockwaveVisualMaterials.LaneEdge, altitude, covered ? 0.55f : 0.68f);
        }

        // DrawReleaseWavefront 负责绘制释放后的低透明扩散波前，补足冲击波真正推出去的瞬间感。
        public static void DrawReleaseWavefront(Vector3 center, float radius, float progress)
        {
            if (radius <= 0.5f || progress <= 0f || progress >= 1f)
            {
                return;
            }

            center.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.025f;
            GenDraw.DrawCircleOutline(center, radius, progress < 0.82f ? SandWormShockwaveVisualMaterials.ReleaseRing : SandWormShockwaveVisualMaterials.ReleaseGhostRing);
            if (radius > 5f)
            {
                GenDraw.DrawCircleOutline(center, Mathf.Max(1f, radius - 3.2f), SandWormShockwaveVisualMaterials.ReleaseGhostRing);
            }

            if (progress < 0.45f)
            {
                float crackLength = Mathf.Lerp(2.5f, 8.5f, progress);
                DrawRadialCracks(center, radius, crackLength);
            }
        }

        // DrawLaneRibs 负责沿危险带边缘绘制短促刻度，让宽度范围一眼可读。
        private static void DrawLaneRibs(Vector3 start, Vector3 direction, Vector3 side, float distance, float halfWidth, float altitude, float charge)
        {
            int count = Mathf.Clamp(Mathf.RoundToInt(distance / LaneRibSpacing), 3, 28);
            float tickOffset = Mathf.Repeat(Find.TickManager.TicksGame * 0.045f, 1f);
            for (int i = 0; i < count; i++)
            {
                float fraction = (i + tickOffset) / count;
                Vector3 center = start + direction * Mathf.Lerp(1.3f, Mathf.Max(1.4f, distance - 1.2f), fraction);
                Vector3 leftEdge = WithAltitude(center + side * halfWidth, altitude);
                Vector3 rightEdge = WithAltitude(center - side * halfWidth, altitude);
                Vector3 leftInner = WithAltitude(leftEdge - side * LaneRibLength, altitude);
                Vector3 rightInner = WithAltitude(rightEdge + side * LaneRibLength, altitude);
                GenDraw.DrawLineBetween(leftEdge, leftInner, SandWormShockwaveVisualMaterials.LaneEdge, 0.12f + charge * 0.04f);
                GenDraw.DrawLineBetween(rightEdge, rightInner, SandWormShockwaveVisualMaterials.LaneEdge, 0.12f + charge * 0.04f);
            }
        }

        // DrawMovingChevrons 负责沿指定方向绘制流动箭头，表现地脉冲击的推进方向。
        private static void DrawMovingChevrons(Vector3 start, Vector3 direction, Vector3 side, float distance, float altitude, Material material, int maxCount, float lineWidth, float speed)
        {
            int count = Mathf.Clamp(Mathf.RoundToInt(distance / 8.5f), 2, maxCount);
            float tickOffset = Mathf.Repeat(Find.TickManager.TicksGame * speed / 60f, 1f);
            for (int i = 0; i < count; i++)
            {
                float progress = Mathf.Repeat(tickOffset + i / (float)count, 1f);
                float lineDistance = Mathf.Lerp(1.2f, Mathf.Max(1.3f, distance - 0.9f), progress);
                Vector3 tip = WithAltitude(start + direction * lineDistance, altitude);
                Vector3 left = WithAltitude(tip - direction * ChevronLength + side * ChevronHalfWidth, altitude);
                Vector3 right = WithAltitude(tip - direction * ChevronLength - side * ChevronHalfWidth, altitude);
                GenDraw.DrawLineBetween(left, tip, material, lineWidth);
                GenDraw.DrawLineBetween(right, tip, material, lineWidth);
            }
        }

        // DrawSourceLock 负责在危险带源点绘制双环和横向锁定线，明确冲击从沙虫身体段发出。
        private static void DrawSourceLock(Vector3 start, Vector3 side, Vector3 direction, float halfWidth, float altitude, float charge)
        {
            GenDraw.DrawCircleOutline(start, Mathf.Clamp(halfWidth * 0.48f + charge * 0.55f, 1.2f, 3.4f), SandWormShockwaveVisualMaterials.LaneSource);
            GenDraw.DrawCircleOutline(start, Mathf.Clamp(halfWidth * 0.88f, 2.0f, 5.2f), SandWormShockwaveVisualMaterials.ReleaseGhostRing);
            Vector3 left = WithAltitude(start - side * Mathf.Min(halfWidth, 3.4f), altitude);
            Vector3 right = WithAltitude(start + side * Mathf.Min(halfWidth, 3.4f), altitude);
            Vector3 forward = WithAltitude(start + direction * Mathf.Min(halfWidth, 3.2f), altitude);
            GenDraw.DrawLineBetween(left, right, SandWormShockwaveVisualMaterials.LaneSource, 0.15f);
            GenDraw.DrawLineBetween(start, forward, SandWormShockwaveVisualMaterials.LaneSource, 0.15f);
        }

        // DrawEndpointMarker 负责在受威胁小人位置绘制小型端点标记。
        private static void DrawEndpointMarker(Vector3 end, Material material, float altitude, float radius)
        {
            end.y = altitude;
            GenDraw.DrawCircleOutline(end, radius, material);
            GenDraw.DrawCircleOutline(end, radius * 1.45f, material);
        }

        // DrawRadialCracks 负责在释放波前附近绘制少量径向裂缝线。
        private static void DrawRadialCracks(Vector3 center, float radius, float crackLength)
        {
            int count = 8;
            float altitude = center.y;
            for (int i = 0; i < count; i++)
            {
                float angle = i * (360f / count) + Find.TickManager.TicksGame * 0.18f;
                Vector3 direction = Vector3Utility.HorizontalVectorFromAngle(angle);
                Vector3 outer = WithAltitude(center + direction * radius, altitude);
                Vector3 inner = WithAltitude(center + direction * Mathf.Max(0.5f, radius - crackLength), altitude);
                GenDraw.DrawLineBetween(inner, outer, SandWormShockwaveVisualMaterials.ReleaseCrack, 0.11f);
            }
        }

        // ShockwaveCharge 负责把剩余 tick 转换为预警蓄力比例。
        private static float ShockwaveCharge(int ticksLeft, int totalTicks)
        {
            if (totalTicks <= 0)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - ticksLeft / (float)totalTicks);
        }

        // WithAltitude 负责把世界坐标移动到覆盖层高度。
        private static Vector3 WithAltitude(Vector3 vector, float altitude)
        {
            vector.y = altitude;
            return vector;
        }
    }
}
