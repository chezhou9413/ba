using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using System;
using System.Collections.Generic;
using System.IO;
using RimUnity;

namespace BANWlLib.mainUI.Images
{
    //管理 UI 专用资源包句柄，包内最后一张缓存图片释放时卸载整个包。
    internal static class BAUIImageBundles
    {
        private static readonly Dictionary<string, int> counts = new(StringComparer.OrdinalIgnoreCase);
        internal static int Count => counts.Count;
        //标准清单路径与像素格式，本层继续持有可回收的独立纹理。
        internal static Texture2D Load(BAUIImageRecord record)
        {
            var texture = RimUnity.Assets.AssetLoader.LoadOwnedTexture(Verse.LoadedModManager.GetMod<BANWlLib.newpro>().Content, "UIImage/" + record.Asset);
            counts[record.Bundle] = counts.TryGetValue(record.Bundle, out int n) ? n + 1 : 1;
            return texture;
        }
        internal static void Release(string name, Texture2D texture)
        {
            RimUnity.Object.Destroy(texture);
            if (--counts[name] == 0) counts.Remove(name);
        }
    }
}
