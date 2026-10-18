using System;
using System.IO;
using System.Linq;
using RimUnity;
using RimUnity.Assets;
using Verse;

namespace BANWlLib.RimUnityMigration;

//使用 RimUnity 清单中的逻辑路径加载界面资源，不打开 Unity 资源包。
public sealed class BAUiAssets
{
    private static ModContentPack Mod => LoadedModManager.GetMod<newpro>().Content;
    public static BAUiAssets Open() => new();

    //保留源工程资源路径用于定位，清单中的类型目录区分 Sprite 与原始贴图。
    public T LoadAsset<T>(string path) where T : RimUnity.Object
    {
        string logical = typeof(T) == typeof(GameObject) ? "Prefabs/" + Path.GetFileNameWithoutExtension(path)
            : typeof(T) == typeof(RimUnity.U2D.SpriteAtlas) ? "Atlases/" + Path.GetFileNameWithoutExtension(path)
            : typeof(T) == typeof(Sprite) ? "Sprite/" + path + "/" + Path.GetFileNameWithoutExtension(path)
            : typeof(T).Name + "/" + path;
        return AssetLoader.Load<T>(Mod, logical);
    }
    public T[] LoadAssetWithSubAssets<T>(string path) where T : RimUnity.Object
    {
        if (typeof(T) != typeof(Sprite) || Path.GetFileName(path) != "CostDigitsAtlas.png")
            throw new NotSupportedException("未登记的 UI 子资源：" + path);
        return Enumerable.Range(0, 10).Select(i => AssetLoader.Load<T>(Mod, "Sprites/Cost/CostDigit_" + i)).ToArray();
    }
    public string[] GetAllAssetNames() => AssetLoader.GetPaths(Mod, "Texture2D")
        .Where(p => p.StartsWith("Texture2D/Assets/", StringComparison.OrdinalIgnoreCase)).Select(p => p[10..].ToLowerInvariant()).ToArray();
}
