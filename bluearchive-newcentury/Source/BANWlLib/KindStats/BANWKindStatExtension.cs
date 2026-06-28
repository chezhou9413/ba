using Verse;

namespace BANWlLib.KindStats
{
    // PawnKind 属性扩展，负责让单个 PawnKind 配置世界地图货物承载能力。
    public class BANWKindStatExtension : DefModExtension
    {
        // 世界地图货物承载能力加值，单位为千克。
        public float worldCargoCapacityOffset;
    }
}
