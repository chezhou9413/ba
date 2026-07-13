using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormShockwaveVisualMaterials 负责缓存地脉冲击视觉用材质，避免绘制工具承担资源管理职责。
    public static class SandWormShockwaveVisualMaterials
    {
        private static readonly Color LaneFillColor = new Color(1f, 0.18f, 0.05f, 0.14f);
        private static readonly Color LaneCoreColor = new Color(1f, 0.62f, 0.20f, 0.84f);
        private static readonly Color LaneEdgeColor = new Color(1f, 0.26f, 0.08f, 0.92f);
        private static readonly Color LaneDarkCrackColor = new Color(0.20f, 0.06f, 0.02f, 0.78f);
        private static readonly Color LaneChevronColor = new Color(1f, 0.80f, 0.28f, 0.92f);
        private static readonly Color LaneSourceColor = new Color(1f, 0.48f, 0.12f, 0.78f);
        private static readonly Color SafeLineColor = new Color(0.18f, 1f, 0.55f, 0.92f);
        private static readonly Color SafeFillColor = new Color(0.12f, 0.78f, 0.42f, 0.18f);
        private static readonly Color ReleaseRingColor = new Color(1f, 0.64f, 0.18f, 0.42f);
        private static readonly Color ReleaseGhostRingColor = new Color(0.74f, 0.50f, 0.28f, 0.22f);
        private static readonly Color ReleaseCrackColor = new Color(1f, 0.32f, 0.08f, 0.60f);

        private static Material laneFillMaterial;
        private static Material laneCoreMaterial;
        private static Material laneEdgeMaterial;
        private static Material laneDarkCrackMaterial;
        private static Material laneChevronMaterial;
        private static Material laneSourceMaterial;
        private static Material safeLineMaterial;
        private static Material safeFillMaterial;
        private static Material releaseRingMaterial;
        private static Material releaseGhostRingMaterial;
        private static Material releaseCrackMaterial;

        // LaneFill 负责提供危险带半透明填充材质。
        public static Material LaneFill => laneFillMaterial ?? (laneFillMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, LaneFillColor));

        // LaneCore 负责提供危险带中心热裂隙材质。
        public static Material LaneCore => laneCoreMaterial ?? (laneCoreMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, LaneCoreColor));

        // LaneEdge 负责提供危险带边界材质。
        public static Material LaneEdge => laneEdgeMaterial ?? (laneEdgeMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, LaneEdgeColor));

        // LaneDarkCrack 负责提供危险带中心暗裂隙材质。
        public static Material LaneDarkCrack => laneDarkCrackMaterial ?? (laneDarkCrackMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, LaneDarkCrackColor));

        // LaneChevron 负责提供危险带流动箭头材质。
        public static Material LaneChevron => laneChevronMaterial ?? (laneChevronMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, LaneChevronColor));

        // LaneSource 负责提供冲击源点锁定材质。
        public static Material LaneSource => laneSourceMaterial ?? (laneSourceMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, LaneSourceColor));

        // SafeLine 负责提供掩体保护状态线材质。
        public static Material SafeLine => safeLineMaterial ?? (safeLineMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, SafeLineColor));

        // SafeFill 负责提供掩体保护状态填充材质。
        public static Material SafeFill => safeFillMaterial ?? (safeFillMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, SafeFillColor));

        // ReleaseRing 负责提供释放波前主环材质。
        public static Material ReleaseRing => releaseRingMaterial ?? (releaseRingMaterial = SolidColorMaterials.SimpleSolidColorMaterial(ReleaseRingColor));

        // ReleaseGhostRing 负责提供释放波前尾环材质。
        public static Material ReleaseGhostRing => releaseGhostRingMaterial ?? (releaseGhostRingMaterial = SolidColorMaterials.SimpleSolidColorMaterial(ReleaseGhostRingColor));

        // ReleaseCrack 负责提供释放裂缝材质。
        public static Material ReleaseCrack => releaseCrackMaterial ?? (releaseCrackMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, ReleaseCrackColor));
    }
}
