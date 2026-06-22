using Verse;

namespace BANWlLib.KindStats
{
    /// <summary>
    /// PawnKind 属性扩展，负责让单个 PawnKind 覆盖生命值并增加世界地图货物承载能力。
    /// </summary>
    public class BANWKindStatExtension : DefModExtension
    {
        /// <summary>
        /// 覆盖种族基础生命值尺度，留空时继续使用种族配置。
        /// </summary>
        public float? healthScaleOverride;

        /// <summary>
        /// 世界地图货物承载能力加值，单位为千克。
        /// </summary>
        public float worldCargoCapacityOffset;
    }
}
