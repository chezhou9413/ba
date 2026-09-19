using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

//维护 UI 图片的 Unity 源目录、迁移和导入设置。
internal static class BAUIImageBuildSources
{
    internal static readonly string ModRoot = BABuildPaths.ModRoot;
    internal const string ManualRoot = "Assets/BAUIImages/Common/UIAssets";
    internal const string NativeRoot = "Assets/Data/Archive.NewWorld/Textures";
    private static readonly string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".dds" };

    //把模组图片复制到 Unity 的永久源目录，构建成功之前保留原文件。
    internal static void ImportModImages()
    {
        CopyImages("Common/UIAssets", ManualRoot);
        foreach (string folder in new[] { "Gacha", "UI", "Stone", "Ability" })
            CopyImages("Common/Textures/" + folder, NativeRoot + "/" + folder);
        string stone = Path.Combine(ModRoot, "Common/Textures/QinghuiStone.png");
        if (File.Exists(stone)) File.Copy(stone, NativeRoot + "/QinghuiStone.png", true);
        AssetDatabase.Refresh();
    }

    //复制指定目录的图片，不迁移配置文件或其他资源。
    private static void CopyImages(string relative, string target)
    {
        string source = Path.Combine(ModRoot, relative);
        if (!Directory.Exists(source)) return;
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories).Where(IsImage))
        {
            string output = target + "/" + file.Substring(source.Length + 1).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.Copy(file, output, true);
        }
    }

    //收集 Unity 工程中的永久图片源，并生成稳定的缓存键与包名。
    internal static List<BAUIImageBuildItem> Collect()
    {
        var items = new List<BAUIImageBuildItem>();
        AddFolder(items, ManualRoot, "Common/UIAssets", false);
        AddFolder(items, NativeRoot + "/Gacha", "Common/Textures/Gacha", false);
        AddFolder(items, NativeRoot + "/UI", "Common/Textures/UI", true);
        AddFolder(items, NativeRoot + "/Stone", "Common/Textures/Stone", true);
        AddFolder(items, NativeRoot + "/Ability", "Common/Textures/Ability", true);
        if (File.Exists(NativeRoot + "/QinghuiStone.png"))
            items.Add(CreateItem(NativeRoot + "/QinghuiStone.png", "Common/Textures/QinghuiStone.png", true));
        if (items.Count == 0) throw new InvalidOperationException("Unity 工程中没有 UI 图片源。");
        if (items.Select(i => i.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != items.Count)
            throw new InvalidOperationException("UI 图片存在同路径不同扩展名或大小写重复。");
        return items.OrderBy(i => i.Key, StringComparer.Ordinal).ToList();
    }

    //收集一个分类目录，保留文件名和子目录结构。
    private static void AddFolder(List<BAUIImageBuildItem> items, string root, string relative, bool shared)
    {
        if (!Directory.Exists(root)) return;
        foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories).Where(IsImage))
            items.Add(CreateItem(file.Replace('\\', '/'), relative + "/" + file.Substring(root.Length + 1).Replace('\\', '/'), shared));
    }

    //为大图生成独立包，小图按所属目录分包，共享图标保持原版可读路径。
    private static BAUIImageBuildItem CreateItem(string asset, string source, bool shared)
    {
        string key = source.Substring(0, source.Length - Path.GetExtension(source).Length);
        TextureImporter importer = AssetImporter.GetAtPath(asset) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("图片导入器不存在：" + asset);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        if (width > 8192 || height > 8192) throw new InvalidOperationException("UI 图片单边不得超过 8192 像素：" + asset);
        bool transient = key.Contains("/ManuaUI/Live/") || key.Contains("/ManuaUI/Bg/") || (long)width * height >= 512 * 1024;
        string group = transient ? key : Path.GetDirectoryName(key).Replace('\\', '/');
        string hash;
        using (MD5 md5 = MD5.Create()) hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(group.ToLowerInvariant()))).Replace("-", "").ToLowerInvariant();
        return new BAUIImageBuildItem
        {
            Asset = asset, Source = source, Key = key, Shared = shared, Transient = transient,
            Bundle = source.StartsWith("Common/Textures/Ability/", StringComparison.OrdinalIgnoreCase) ? "banw_textures_ability" :
                shared ? "banw_ui_shared_win" : "ui_" + hash + ".ab"
        };
    }

    //保留 UI 原始尺寸与透明度，关闭 mipmap 和 CPU 像素副本。
    internal static void Configure(List<BAUIImageBuildItem> items)
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (BAUIImageBuildItem item in items)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(item.Asset);
                importer.textureType = TextureImporterType.Default;
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 8192;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.assetBundleName = string.Empty;
                importer.ClearPlatformTextureSettings("Standalone");
                importer.SaveAndReimport();
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        AssetDatabase.Refresh();
    }

    //判断文件扩展名是否为本工具支持的图片格式。
    private static bool IsImage(string path)
    {
        return imageExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    }

    //成功打包后移除模组中的对应散图，删除前验证 Unity 源文件内容完全相同。
    internal static void RemoveMigratedImages(List<BAUIImageBuildItem> items)
    {
        string root = Path.GetFullPath(ModRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        int removed = 0;
        foreach (BAUIImageBuildItem item in items)
        {
            string path = Path.GetFullPath(Path.Combine(ModRoot, item.Source));
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("迁移路径超出模组目录：" + path);
            if (!File.Exists(path)) continue;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] sourceHash = sha.ComputeHash(File.ReadAllBytes(path));
                byte[] unityHash = sha.ComputeHash(File.ReadAllBytes(item.Asset));
                if (!sourceHash.SequenceEqual(unityHash)) throw new InvalidOperationException("Unity 图片源与模组散图不一致，停止迁移：" + path);
            }
            File.Delete(path);
            removed++;
        }
        Debug.Log("[BA UI图片] 已迁移 " + removed + " 张散图，永久源文件保存在 Unity 工程。");
    }
}
