using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//负责维护 Unity 中的小人贴图源文件、合并 Mod 散图并生成压缩 AssetBundle。
public static class BAPawnBundleBuilder
{
    private static readonly string SourcePawnRoot = Path.Combine(BABuildPaths.ModRoot, "Common/Textures/Pawns");
    private const string LegacyUnityPawnRoot = "Assets/PawnImages";
    private const string UnityPawnRoot = "Assets/Data/Archive.NewWorld/Textures/Pawns";
    private static readonly string OutputBundleRoot = BABuildPaths.BundleRoot;
    private const string PawnBundleName = "banw_pawns_win";

    private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg"
    };

    // Unity 菜单入口，负责同步并配置小人贴图导入设置。
    [MenuItem("RimWorldTools/BA小人/同步小人贴图")]
    public static void SyncPawnImagesMenu()
    {
        SyncPawnImages();
        ConfigurePawnTextureImporters();
        EditorUtility.DisplayDialog("完成", "BA 小人贴图同步完成。", "确定");
    }

    // Unity 菜单入口，负责一键同步、配置并打包小人贴图 AB。
    [MenuItem("RimWorldTools/BA小人/一键打包小人AB")]
    public static void BuildAllMenu()
    {
        BuildAll();
        EditorUtility.DisplayDialog("完成", "BA 小人 AssetBundle 打包完成。", "确定");
    }

    // 命令行入口，负责让 Unity batchmode 调用完整打包流程。
    public static void BuildAllFromCommandLine()
    {
        BuildAll();
    }

    // 完整构建流程，负责同步源贴图、配置压缩导入设置并输出单个 Windows AB。
    public static void BuildAll()
    {
        SyncPawnImages();
        AssetDatabase.Refresh();
        ConfigurePawnTextureImporters();
        AssetDatabase.Refresh();

        List<string> assetPaths = CollectUnityImageAssetPaths();
        if (assetPaths.Count == 0)
        {
            throw new InvalidOperationException("没有找到可打包的 BA 小人贴图。");
        }

        EnsureDirectory(OutputBundleRoot);
        CleanGeneratedBundle();

        AssetBundleBuild[] buildMap = CreateBuildMap(assetPaths);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            OutputBundleRoot,
            buildMap,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.StandaloneWindows64);

        if (manifest == null)
        {
            throw new InvalidOperationException("BA 小人 AssetBundle 打包失败，Unity 没有返回 Manifest。");
        }

        int bundleAssetCount = CountBundleAssets();
        if (bundleAssetCount != assetPaths.Count)
        {
            throw new InvalidOperationException($"BA 小人 AssetBundle 资源数不一致：Unity资源 {assetPaths.Count}，AB资源 {bundleAssetCount}");
        }

        Debug.Log($"[BA小人AB] 打包完成，源贴图 {CountSourceImagesSafe()} 张，Unity资源 {assetPaths.Count} 张，AB资源 {bundleAssetCount} 张，输出 {Path.Combine(OutputBundleRoot, PawnBundleName)}");
    }

    //负责将 Mod 散图合并到 Unity 永久素材目录，仅在永久目录为空时迁移旧目录。
    public static void SyncPawnImages()
    {
        List<string> sourceImagePaths = CollectSourceImagePaths();
        if (sourceImagePaths.Count == 0)
        {
            if (CollectUnityImageAssetPaths().Count == 0)
            {
                SyncPawnImagesFromLegacyUnityRoot();
            }
            return;
        }

        EnsureDirectory(GetUnityPawnAbsoluteRoot());

        foreach (string sourcePath in sourceImagePaths)
        {
            string relativePath = GetRelativePath(SourcePawnRoot, sourcePath);
            string unityPath = ToUnityPath(Path.Combine(UnityPawnRoot, relativePath));
            string targetFilePath = ToProjectAbsolutePath(unityPath);

            EnsureDirectory(Path.GetDirectoryName(targetFilePath));
            if (ShouldCopyFile(sourcePath, targetFilePath))
            {
                File.Copy(sourcePath, targetFilePath, true);
            }
        }

        AssetDatabase.Refresh();
    }

    // 导入设置修正，负责把小人贴图限制到 512、关闭 mipmap 并启用 DXT5 压缩。
    public static void ConfigurePawnTextureImporters()
    {
        foreach (string assetPath in CollectUnityImageAssetPaths())
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            bool changed = false;
            changed |= SetImporterValue(importer.textureType != TextureImporterType.Default, () => importer.textureType = TextureImporterType.Default);
            changed |= SetImporterValue(importer.mipmapEnabled, () => importer.mipmapEnabled = false);
            changed |= SetImporterValue(importer.isReadable, () => importer.isReadable = false);
            changed |= SetImporterValue(importer.npotScale != TextureImporterNPOTScale.None, () => importer.npotScale = TextureImporterNPOTScale.None);
            changed |= SetImporterValue(importer.textureCompression != TextureImporterCompression.Compressed, () => importer.textureCompression = TextureImporterCompression.Compressed);
            changed |= SetImporterValue(importer.maxTextureSize != 512, () => importer.maxTextureSize = 512);
            changed |= SetImporterValue(importer.filterMode != FilterMode.Bilinear, () => importer.filterMode = FilterMode.Bilinear);
            changed |= SetImporterValue(importer.wrapMode != TextureWrapMode.Clamp, () => importer.wrapMode = TextureWrapMode.Clamp);
            changed |= SetImporterValue(!importer.alphaIsTransparency, () => importer.alphaIsTransparency = true);

            TextureImporterPlatformSettings standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
            bool standaloneChanged = false;
            standaloneChanged |= standaloneSettings.overridden != true;
            standaloneChanged |= standaloneSettings.maxTextureSize != 512;
            standaloneChanged |= standaloneSettings.textureCompression != TextureImporterCompression.Compressed;
            standaloneChanged |= standaloneSettings.format != TextureImporterFormat.DXT5;

            if (standaloneChanged)
            {
                standaloneSettings.overridden = true;
                standaloneSettings.maxTextureSize = 512;
                standaloneSettings.textureCompression = TextureImporterCompression.Compressed;
                standaloneSettings.format = TextureImporterFormat.DXT5;
                importer.SetPlatformTextureSettings(standaloneSettings);
                changed = true;
            }

            AssetImporter assetImporter = importer;
            if (assetImporter.assetBundleName != PawnBundleName)
            {
                assetImporter.assetBundleName = PawnBundleName;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
    }

    // 图片收集，负责读取 Mod 源目录下的小人原始贴图。
    private static List<string> CollectSourceImagePaths()
    {
        List<string> sourceImagePaths = new List<string>();
        if (!Directory.Exists(SourcePawnRoot))
        {
            return sourceImagePaths;
        }

        foreach (string sourcePath in Directory.GetFiles(SourcePawnRoot, "*.*", SearchOption.AllDirectories))
        {
            if (IsImagePath(sourcePath))
            {
                sourceImagePaths.Add(sourcePath);
            }
        }

        sourceImagePaths.Sort(StringComparer.OrdinalIgnoreCase);
        return sourceImagePaths;
    }

    //负责在永久素材目录为空时，从旧 Unity 目录迁移已有的小人贴图。
    private static void SyncPawnImagesFromLegacyUnityRoot()
    {
        string legacyRoot = ToProjectAbsolutePath(LegacyUnityPawnRoot);
        if (!Directory.Exists(legacyRoot))
        {
            Debug.LogWarning($"[BA小人AB] 源目录和旧 Unity 目录都没有 PNG/JPG：{SourcePawnRoot}");
            return;
        }

        List<string> legacyImagePaths = CollectImagePathsUnderRoot(legacyRoot);
        if (legacyImagePaths.Count == 0)
        {
            Debug.LogWarning($"[BA小人AB] 旧 Unity 目录没有 PNG/JPG：{LegacyUnityPawnRoot}");
            return;
        }

        EnsureDirectory(GetUnityPawnAbsoluteRoot());

        HashSet<string> expectedUnityPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string legacyPath in legacyImagePaths)
        {
            string relativePath = GetRelativePath(legacyRoot, legacyPath);
            string unityPath = ToUnityPath(Path.Combine(UnityPawnRoot, relativePath));
            string targetFilePath = ToProjectAbsolutePath(unityPath);
            expectedUnityPaths.Add(unityPath);

            EnsureDirectory(Path.GetDirectoryName(targetFilePath));
            if (ShouldCopyFile(legacyPath, targetFilePath))
            {
                File.Copy(legacyPath, targetFilePath, true);
            }
        }

        RemoveStaleUnityImages(expectedUnityPaths);
        AssetDatabase.Refresh();
        Debug.Log($"[BA小人AB] 源目录没有 PNG/JPG，已从旧 Unity 目录迁移 {legacyImagePaths.Count} 张：{LegacyUnityPawnRoot}");
    }

    // 数量统计，负责在日志中输出源目录仍存在时的图片数量。
    private static int CountSourceImagesSafe()
    {
        return CollectSourceImagePaths().Count;
    }

    // 导入设置赋值，负责只在值确实变化时标记重新导入。
    private static bool SetImporterValue(bool shouldChange, Action applyChange)
    {
        if (!shouldChange)
        {
            return false;
        }

        applyChange();
        return true;
    }

    // Unity 图片收集，负责按稳定顺序返回项目内所有小人图片资源路径。
    private static List<string> CollectUnityImageAssetPaths()
    {
        string absoluteRoot = GetUnityPawnAbsoluteRoot();
        List<string> imagePaths = CollectImagePathsUnderRoot(absoluteRoot);
        List<string> assetPaths = new List<string>();
        foreach (string filePath in imagePaths)
        {
            assetPaths.Add(ToUnityAssetPath(filePath));
        }

        assetPaths.Sort(StringComparer.OrdinalIgnoreCase);
        return assetPaths;
    }

    // 图片收集，负责读取指定根目录下的支持格式图片。
    private static List<string> CollectImagePathsUnderRoot(string absoluteRoot)
    {
        List<string> imagePaths = new List<string>();
        if (!Directory.Exists(absoluteRoot))
        {
            return imagePaths;
        }

        foreach (string filePath in Directory.GetFiles(absoluteRoot, "*.*", SearchOption.AllDirectories))
        {
            if (!IsImagePath(filePath))
            {
                continue;
            }

            imagePaths.Add(filePath);
        }

        imagePaths.Sort(StringComparer.OrdinalIgnoreCase);
        return imagePaths;
    }

    // 构建映射，负责把全部小人贴图放入同一个 AssetBundle。
    private static AssetBundleBuild[] CreateBuildMap(List<string> assetPaths)
    {
        return new[]
        {
            new AssetBundleBuild
            {
                assetBundleName = PawnBundleName,
                assetNames = assetPaths.ToArray()
            }
        };
    }

    // 包验证，负责读取生成后的 AB 并统计资源名数量。
    private static int CountBundleAssets()
    {
        string bundlePath = Path.Combine(OutputBundleRoot, PawnBundleName);
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null)
        {
            throw new InvalidOperationException($"BA 小人 AssetBundle 无法加载：{bundlePath}");
        }

        try
        {
            return bundle.GetAllAssetNames().Length;
        }
        finally
        {
            bundle.Unload(false);
        }
    }

    // 清理流程，负责删除旧的小人 AB 和 Manifest。
    private static void CleanGeneratedBundle()
    {
        string bundlePath = Path.Combine(OutputBundleRoot, PawnBundleName);
        DeleteFileIfExists(bundlePath);
        DeleteFileIfExists(bundlePath + ".manifest");
        DeleteFileIfExists(Path.Combine(OutputBundleRoot, "banw_pawns.ab"));
        DeleteFileIfExists(Path.Combine(OutputBundleRoot, "banw_pawns.ab.manifest"));
        DeleteFileIfExists(Path.Combine(OutputBundleRoot, Path.GetFileName(OutputBundleRoot)));
        DeleteFileIfExists(Path.Combine(OutputBundleRoot, Path.GetFileName(OutputBundleRoot) + ".manifest"));
    }

    // 过期清理，负责删除 Unity 项目里源目录已不存在的旧同步图片。
    private static void RemoveStaleUnityImages(HashSet<string> expectedUnityPaths)
    {
        string absoluteRoot = GetUnityPawnAbsoluteRoot();
        if (!Directory.Exists(absoluteRoot))
        {
            return;
        }

        foreach (string filePath in Directory.GetFiles(absoluteRoot, "*.*", SearchOption.AllDirectories))
        {
            if (!IsImagePath(filePath))
            {
                continue;
            }

            string unityPath = ToUnityAssetPath(filePath);
            if (expectedUnityPaths.Contains(unityPath))
            {
                continue;
            }

            DeleteFileIfExists(filePath);
            DeleteFileIfExists(filePath + ".meta");
        }

        RemoveEmptyDirectories(absoluteRoot);
    }

    // 目录清理，负责删除同步后留下的空目录。
    private static void RemoveEmptyDirectories(string root)
    {
        foreach (string directory in Directory.GetDirectories(root))
        {
            RemoveEmptyDirectories(directory);
        }

        if (Path.GetFullPath(root) == Path.GetFullPath(GetUnityPawnAbsoluteRoot()))
        {
            return;
        }

        if (Directory.GetFiles(root).Length == 0 && Directory.GetDirectories(root).Length == 0)
        {
            Directory.Delete(root);
        }
    }

    // 图片判断，负责限制同步和打包只处理支持的贴图扩展名。
    private static bool IsImagePath(string path)
    {
        return ImageExtensions.Contains(Path.GetExtension(path));
    }

    // 复制判断，负责避免没有变化的图片反复覆盖。
    private static bool ShouldCopyFile(string sourcePath, string targetPath)
    {
        if (!File.Exists(targetPath))
        {
            return true;
        }

        FileInfo sourceInfo = new FileInfo(sourcePath);
        FileInfo targetInfo = new FileInfo(targetPath);
        return sourceInfo.Length != targetInfo.Length || sourceInfo.LastWriteTimeUtc > targetInfo.LastWriteTimeUtc;
    }

    // 路径转换，负责得到相对路径，兼容当前 Unity 版本没有 Path.GetRelativePath 的情况。
    private static string GetRelativePath(string root, string path)
    {
        Uri rootUri = new Uri(AppendDirectorySeparator(Path.GetFullPath(root)));
        Uri pathUri = new Uri(Path.GetFullPath(path));
        return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
    }

    // 路径转换，负责把系统路径转换成 Unity 使用的正斜杠路径。
    private static string ToUnityPath(string path)
    {
        return path.Replace('\\', '/');
    }

    // 路径转换，负责把项目内绝对路径转换成 AssetDatabase 可识别的 Assets 相对路径。
    private static string ToUnityAssetPath(string absolutePath)
    {
        string fullPath = Path.GetFullPath(absolutePath);
        string assetsRoot = Application.dataPath;
        string relativePath = GetRelativePath(assetsRoot, fullPath);
        return "Assets/" + ToUnityPath(relativePath);
    }

    // 路径转换，负责得到 Unity 项目内小人图片目录的绝对路径。
    private static string GetUnityPawnAbsoluteRoot()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.GetFullPath(Path.Combine(projectRoot, UnityPawnRoot));
    }

    // 路径转换，负责把项目相对路径转换成当前 Unity 项目的绝对路径。
    private static string ToProjectAbsolutePath(string projectRelativePath)
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.GetFullPath(Path.Combine(projectRoot, projectRelativePath));
    }

    // 路径转换，负责确保目录 URI 末尾带分隔符。
    private static string AppendDirectorySeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }

    // 目录保障，负责在写入或输出前创建缺失目录。
    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    // 文件删除，负责安全删除指定文件。
    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
