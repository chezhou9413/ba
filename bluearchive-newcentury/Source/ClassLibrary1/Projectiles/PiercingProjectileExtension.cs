using Verse;

namespace BANWlLib.Projectiles
{
    /// <summary>
    /// 穿透抛射体配置扩展，负责从 XML 提供飞行伤害、范围和视觉参数。
    /// </summary>
    public class PiercingProjectileExtension : DefModExtension
    {
        /// <summary>
        /// 飞行过程中每多少 tick 对范围内敌人造成一次伤害。
        /// </summary>
        public int damageIntervalTicks = 1;

        /// <summary>
        /// 飞行方向垂直宽度，单位为格。
        /// </summary>
        public float damageWidth = 1f;

        /// <summary>
        /// 飞行方向长度，单位为格。
        /// </summary>
        public float damageLength = 1f;

        /// <summary>
        /// 是否免疫友军伤害。
        /// </summary>
        public bool immuneFriendlyFire = true;

        /// <summary>
        /// 飞行开始时的贴图缩放。
        /// </summary>
        public float startDrawScale = 1f;

        /// <summary>
        /// 飞行结束时的贴图缩放。
        /// </summary>
        public float endDrawScale = 1f;

        /// <summary>
        /// 淡入 tick 数，0 表示不淡入。
        /// </summary>
        public int fadeInTicks = 0;

        /// <summary>
        /// 淡出 tick 数，0 表示不淡出。
        /// </summary>
        public int fadeOutTicks = 0;
    }
}
