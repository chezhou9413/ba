using RimWorld;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormShockwaveDustUtility 负责播放地脉冲击的尘土、破土、命中和掩体保护反馈。
    public static class SandWormShockwaveDustUtility
    {
        private static readonly Color DustColor = new Color(0.74f, 0.53f, 0.31f, 0.94f);
        private static readonly Color DeepDustColor = new Color(0.48f, 0.36f, 0.26f, 0.92f);

        // ThrowWarningLaneDust 负责在危险带中喷出中心裂地尘和边界沙尘，增强预警带的地表扰动。
        public static void ThrowWarningLaneDust(Map map, SandWormShockwaveLane lane, float charge, float visualScale)
        {
            if (map == null || lane == null)
            {
                return;
            }

            Vector3 center = Vector3.Lerp(lane.Start, lane.End, Rand.Range(0.14f, 0.88f));
            center += lane.Side * Rand.Range(-lane.HalfWidth * 0.82f, lane.HalfWidth * 0.82f);
            ThrowDustAt(map, center, Rand.Range(1.45f, 2.45f) * visualScale, DustColor);

            if (Rand.Chance(0.55f + charge * 0.25f))
            {
                Vector3 edge = Vector3.Lerp(lane.Start, lane.End, Rand.Range(0.18f, 0.82f));
                edge += lane.Side * lane.HalfWidth * (Rand.Bool ? 1f : -1f);
                ThrowDustAt(map, edge, Rand.Range(0.90f, 1.55f) * visualScale, DeepDustColor);
            }
        }

        // ThrowReleaseLaneBurst 负责在释放瞬间沿危险带喷出短促尘爆，表现冲击真正穿过预警带。
        public static void ThrowReleaseLaneBurst(Map map, SandWormShockwaveLane lane, float visualScale)
        {
            if (map == null || lane == null)
            {
                return;
            }

            float distance = (lane.End - lane.Start).MagnitudeHorizontal();
            int count = Mathf.Clamp(Mathf.RoundToInt(distance / 12f), 4, 14);
            for (int i = 0; i < count; i++)
            {
                float progress = (i + Rand.Range(0.15f, 0.85f)) / count;
                Vector3 loc = Vector3.Lerp(lane.Start, lane.End, progress);
                loc += lane.Side * Rand.Range(-lane.HalfWidth * 0.62f, lane.HalfWidth * 0.62f);
                ThrowDustAt(map, loc, Rand.Range(2.1f, 3.9f) * visualScale, DustColor);
            }
        }

        // ThrowSourceReleaseBurst 负责在沙虫身体源点生成更重的破土、热尘和空气冲击反馈。
        public static void ThrowSourceReleaseBurst(Map map, IntVec3 sourceCell, float visualScale, FleckDef blastDry)
        {
            if (map == null || !sourceCell.InBounds(map) || sourceCell.Fogged(map))
            {
                return;
            }

            Vector3 center = sourceCell.ToVector3Shifted();
            if (blastDry != null && sourceCell.ShouldSpawnMotesAt(map))
            {
                FleckMaker.Static(center + Gen.RandomHorizontalVector(0.45f), map, blastDry, Rand.Range(2.2f, 3.3f) * visualScale);
            }

            FleckMaker.ThrowAirPuffUp(center, map);
            FleckMaker.ThrowTornadoDustPuff(center + Gen.RandomHorizontalVector(0.45f), map, Rand.Range(2.4f, 3.6f) * visualScale, DeepDustColor);
            for (int i = 0; i < 16; i++)
            {
                Vector3 loc = center + Gen.RandomHorizontalVector(Rand.Range(0.35f, 4.2f));
                ThrowDustAt(map, loc, Rand.Range(1.8f, 3.9f) * visualScale, i % 3 == 0 ? DeepDustColor : DustColor);
            }
        }

        // ThrowPawnShockwaveHitFeedback 负责在小人被冲击命中时播放横向扬尘，强化被波面扫过的方向感。
        public static void ThrowPawnShockwaveHitFeedback(Map map, Pawn pawn, SandWormShockwaveLane lane, float visualScale)
        {
            if (map == null || pawn == null || lane == null || !pawn.Spawned)
            {
                return;
            }

            Vector3 baseLoc = pawn.DrawPos;
            for (int i = 0; i < 7; i++)
            {
                Vector3 loc = baseLoc + lane.Direction * Rand.Range(-0.35f, 1.35f) + lane.Side * Rand.Range(-1.15f, 1.15f);
                ThrowDustAt(map, loc, Rand.Range(1.2f, 2.3f) * visualScale, DustColor);
            }
        }

        // ThrowProtectedFeedback 负责在掩体挡下冲击时播放偏绿的裂隙回流尘，突出安全状态。
        public static void ThrowProtectedFeedback(Map map, Pawn pawn, float visualScale)
        {
            if (map == null || pawn == null || !pawn.Spawned)
            {
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                FleckMaker.ThrowDustPuffThick(
                    pawn.DrawPos + Gen.RandomHorizontalVector(Rand.Range(0.35f, 1.45f)),
                    map,
                    Rand.Range(1.25f, 2.10f) * visualScale,
                    new Color(0.45f, 0.68f, 0.46f, 0.92f));
            }
        }

        // ThrowDustAt 负责在有效地图格播放厚尘，统一越界和迷雾检查。
        private static void ThrowDustAt(Map map, Vector3 loc, float scale, Color color)
        {
            IntVec3 cell = loc.ToIntVec3();
            if (!cell.InBounds(map) || cell.Fogged(map))
            {
                return;
            }

            FleckMaker.ThrowDustPuffThick(cell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.38f), map, scale, color);
        }
    }
}
