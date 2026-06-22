using HarmonyLib;
using Verse;

namespace BANWlLib.Effects.AssetBundles
{
    // 特效图形生命周期，负责在读档、新游戏和退出存档前释放 AB 缓存。
    internal static class BAEffectGraphicsLifecycle
    {
        // 重置运行时缓存，负责防止跨存档残留 AssetBundle 和 Unity 对象。
        public static void ResetBeforeGameSwap()
        {
            BAEffectBundleRegistry.ResetAll();
        }
    }

    // 读档生命周期补丁，负责在加载存档前释放旧特效 AB。
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    internal static class BAEffectGraphicsLifecycleLoadGamePatch
    {
        // 前置补丁，负责执行特效 AB 缓存重置。
        public static void Prefix()
        {
            BAEffectGraphicsLifecycle.ResetBeforeGameSwap();
        }
    }

    // 新游戏生命周期补丁，负责在创建新存档前释放旧特效 AB。
    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    internal static class BAEffectGraphicsLifecycleInitNewGamePatch
    {
        // 前置补丁，负责执行特效 AB 缓存重置。
        public static void Prefix()
        {
            BAEffectGraphicsLifecycle.ResetBeforeGameSwap();
        }
    }

    // 退出生命周期补丁，负责在销毁当前 Game 前释放特效 AB。
    [HarmonyPatch(typeof(Game), nameof(Game.Dispose))]
    internal static class BAEffectGraphicsLifecycleDisposePatch
    {
        // 前置补丁，负责执行特效 AB 缓存重置。
        public static void Prefix()
        {
            BAEffectGraphicsLifecycle.ResetBeforeGameSwap();
        }
    }
}
