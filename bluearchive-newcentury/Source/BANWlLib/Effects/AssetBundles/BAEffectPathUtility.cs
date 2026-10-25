using System;

namespace BANWlLib.Effects.AssetBundles
{
    //识别交给RimUnity加载的特效贴图逻辑路径。
    internal static class BAEffectPathUtility
    {
        //特效资源在清单中使用Effects/Effect目录前缀。
        public static bool IsEffectTexPath(string texPath)
        {
            return !string.IsNullOrWhiteSpace(texPath)
                && texPath.Replace('\\', '/').StartsWith("Effect/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
