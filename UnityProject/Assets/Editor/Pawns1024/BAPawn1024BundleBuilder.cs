using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//使用 Unity 中的小人源图构建独立的 1024 贴图包，完成后还原原有导入设置。
public static class BAPawn1024BundleBuilder
{
    private const string SourceRoot = "Assets/Data/Archive.NewWorld/Textures/Pawns";
    private const string BundleName = "banw_pawns_win";
    private static readonly string Staging = Path.Combine(BABuildPaths.StagingRoot, "Pawns1024");

    //菜单与命令行共用入口，构建独立包并保留现有 512 运行包。
    [MenuItem("RimWorldTools/BA小人/打包1024独立贴图包")]
    public static void Build()
    {
        BuildVariants(false);
    }

    //使用同一批源图同时更新主 Mod 的 512 包与独立的 1024 包。
    [MenuItem("RimWorldTools/BA小人/同步打包512与1024贴图包")]
    public static void BuildBoth()
    {
        BuildVariants(true);
    }

    //合并角色源图并执行所需分辨率的构建，结束后还原导入设置。
    private static void BuildVariants(bool update512)
    {
        string output = GetOutputDirectory();
        MergeModImages();
        List<string> assets = CollectAssets();
        var snapshots = assets.ToDictionary(path => path, path => File.ReadAllBytes(path + ".meta"));
        try
        {
            if (update512)
            {
                BuildAndPublish(assets, 512, BABuildPaths.BundleRoot);
            }
            BuildAndPublish(assets, 1024, output);
        }
        finally
        {
            RestoreImporters(snapshots);
        }
        Debug.Log($"[BA小人1024] 构建成功：{assets.Count} 张贴图，导入设置已还原。输出：{Path.Combine(output, BundleName)}");
    }

    //生成指定分辨率的包，核对资源完整性和尺寸后输出同名替换文件。
    private static void BuildAndPublish(List<string> assets, int resolution, string output)
    {
        ConfigureResolution(assets, resolution);
        string staging = resolution == 1024 ? Staging : Path.Combine(Path.GetDirectoryName(Staging), "Pawns512");
        Directory.CreateDirectory(staging);
        var builds = new[] { new AssetBundleBuild { assetBundleName = BundleName, assetNames = assets.ToArray() } };
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(staging, builds,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
        if (manifest == null) throw new InvalidOperationException(resolution + " 小人贴图包构建失败。");
        if (manifest.GetAllDependencies(BundleName).Length != 0)
            throw new InvalidOperationException("独立小人贴图包存在未包含的外部依赖。");
        ValidateBundle(assets, resolution, staging);
        Directory.CreateDirectory(output);
        File.Copy(Path.Combine(staging, BundleName), Path.Combine(output, BundleName), true);
        File.Copy(Path.Combine(staging, BundleName + ".manifest"), Path.Combine(output, BundleName + ".manifest"), true);
        Debug.Log($"[BA小人贴图] {resolution} 包已输出，资源 {assets.Count} 张：{Path.Combine(output, BundleName)}");
    }

    //把仓库仍保留的角色散图合入 Unity 源目录，不删除其他角色的永久源图。
    private static void MergeModImages()
    {
        string source = Path.Combine(BABuildPaths.ModRoot, "Common/Textures/Pawns");
        if (!Directory.Exists(source)) return;
        string[] extensions = { ".png", ".jpg", ".jpeg" };
        int merged = 0;
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            if (!extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase)) continue;
            string target = Path.Combine(SourceRoot, file.Substring(source.Length + 1));
            byte[] sourceBytes = File.ReadAllBytes(file);
            if (File.Exists(target) && sourceBytes.SequenceEqual(File.ReadAllBytes(target))) continue;
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.WriteAllBytes(target, sourceBytes);
            merged++;
        }
        AssetDatabase.Refresh();
        Debug.Log("[BA小人1024] 合入当前 Mod 角色源图：" + merged + " 张。");
    }

    //只收集 Unity 工程中的永久图片源，不执行旧式散图同步或清理。
    private static List<string> CollectAssets()
    {
        if (!Directory.Exists(SourceRoot)) throw new DirectoryNotFoundException("Unity 小人源图目录不存在：" + SourceRoot);
        string[] extensions = { ".png", ".jpg", ".jpeg" };
        List<string> assets = Directory.GetFiles(SourceRoot, "*", SearchOption.AllDirectories)
            .Where(path => extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Select(path => path.Replace('\\', '/')).OrderBy(path => path, StringComparer.Ordinal).ToList();
        if (assets.Count == 0) throw new InvalidOperationException("Unity 小人源图目录为空。");
        return assets;
    }

    //设置本次 Windows 构建的分辨率上限、DXT5 压缩和无 mipmap 导入参数。
    private static void ConfigureResolution(List<string> assets, int resolution)
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string path in assets)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("无法取得贴图导入器：" + path);
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = resolution;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.alphaIsTransparency = true;
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Standalone");
                settings.overridden = true;
                settings.maxTextureSize = resolution;
                settings.textureCompression = TextureImporterCompression.Compressed;
                settings.format = TextureImporterFormat.DXT5;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        AssetDatabase.Refresh();
    }

    //确认包内图片完整，并读取三张标准源图核对实际输出分辨率。
    private static void ValidateBundle(List<string> assets, int resolution, string staging)
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(staging, BundleName));
        if (bundle == null) throw new InvalidOperationException("无法读取生成的小人贴图包。");
        try
        {
            var names = new HashSet<string>(bundle.GetAllAssetNames(), StringComparer.OrdinalIgnoreCase);
            if (names.Count != assets.Count || assets.Any(path => !names.Contains(path)))
                throw new InvalidOperationException("小人贴图包中的资源路径或数量不完整。");
            int checkedCount = 0;
            foreach (string path in assets)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                if (width != 1024 || height != 1024) continue;
                Texture2D texture = bundle.LoadAsset<Texture2D>(path);
                if (texture == null || texture.width != resolution || texture.height != resolution)
                    throw new InvalidOperationException("打包后的贴图分辨率不是 " + resolution + " × " + resolution + "：" + path);
                Debug.Log("[BA小人贴图] 包内尺寸确认：" + path + " = " + texture.width + "×" + texture.height);
                Resources.UnloadAsset(texture);
                if (++checkedCount == 3) break;
            }
            if (checkedCount == 0) throw new InvalidOperationException("没有找到可核对尺寸的 1024 源图。");
        }
        finally { bundle.Unload(true); }
    }

    //还原每张图片的完整导入配置，避免独立包的分辨率影响后续常规构建。
    private static void RestoreImporters(Dictionary<string, byte[]> snapshots)
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (KeyValuePair<string, byte[]> snapshot in snapshots)
            {
                File.WriteAllBytes(snapshot.Key + ".meta", snapshot.Value);
                AssetDatabase.ImportAsset(snapshot.Key, ImportAssetOptions.ForceUpdate);
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        AssetDatabase.Refresh();
    }

    //读取构建脚本传入的输出目录，菜单调用时使用仓库内的默认目录。
    private static string GetOutputDirectory()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] != "-baPawn1024Output") continue;
            if (i + 1 >= args.Length) throw new ArgumentException("缺少 -baPawn1024Output 的目录参数。");
            return Path.GetFullPath(args[i + 1]);
        }
        return BABuildPaths.Pawn1024Root;
    }
}
