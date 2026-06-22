using Verse;

namespace BANWlLib.Effects.AssetBundles
{
    // 特效缓存组件，负责在游戏 Tick 中驱动 AB 特效缓存的延迟清理。
    public class BAEffectCacheGameComponent : GameComponent
    {
        public BAEffectCacheGameComponent(Game game)
        {
        }

        // 每个游戏 Tick 检查是否需要清理空闲特效 AB。
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            BAEffectBundleRegistry.CleanupIfNeeded();
        }
    }
}
