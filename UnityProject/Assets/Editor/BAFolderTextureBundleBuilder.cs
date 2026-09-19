using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//负责合并通用贴图到 Unity 素材目录，并按顶层目录生成原版可读取的资源包。
public static class BAFolderTextureBundleBuilder
{
    private static readonly string SourceTextureRoot = Path.Combine(BABuildPaths.ModRoot, "Common/Textures");
    private const string UnityTextureRoot = "Assets/Data/Archive.NewWorld/Textures";
    private static readonly string OutputBundleRoot = BABuildPaths.BundleRoot;
    private const string BundleNamePrefix = "banw_textures_";

    private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg"
    };

    // Unity 菜单入口，负责同步 Common\Textures 贴图并设置导入压缩参数。
    [MenuItem("RimWorldTools/BA通用贴图/同步通用贴图")]
    public static void SyncTexturesMenu()
    {
        SyncTextureImages();
        AssetDatabase.Refresh();
        ConfigureTextureImporters();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", "BA 通用贴图同步完成。", "确定");
    }

    // Unity 菜单入口，负责按 Common\Textures 顶层目录一键打包 AssetBundle。
    [MenuItem("RimWorldTools/BA通用贴图/一键按目录打包贴图AB")]
    public static void BuildAllMenu()
    {
        BuildAll();
        EditorUtility.DisplayDialog("完成", "BA 通用贴图 AssetBundle 打包完成。", "确定");
    }

    // 命令行入口，负责让 Unity batchmode 调用完整打包流程。
    public static void BuildAllFromCommandLine()
    {
        BuildAll();
    }

    // 完整构建流程，负责同步、压缩、按顶层目录构建并验证资源数量。
    public static void BuildAll()
    {
        SyncTextureImages();
        AssetDatabase.Refresh();
        ConfigureTextureImporters();
        AssetDatabase.Refresh();

        Dictionary<string, List<string>> groupedAssetPaths = CollectUnityImageAssetPathsByTopFolder();
        if (groupedAssetPaths.Count == 0)
        {
            throw new InvalidOperationException("没有找到可打包的 BA 通用贴图。");
        }

        EnsureDirectory(OutputBundleRoot);
        CleanGeneratedBundles(groupedAssetPaths.Keys);

        AssetBundleBuild[] buildMap = CreateBuildMap(groupedAssetPaths);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            OutputBundleRoot,
            buildMap,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.StandaloneWindows64);

        if (manifest == null)
        {
            throw new InvalidOperationException("BA 通用贴图 AssetBundle 打包失败，Unity 没有返回 Manifest。");
        }

        ValidateBundles(groupedAssetPaths);
        Debug.Log($"[BA通用贴图AB] 打包完成，输出 {groupedAssetPaths.Count} 个目录包，资源 {groupedAssetPaths.Sum(pair => pair.Value.Count)} 张，目录：{OutputBundleRoot}");
    }

    //负责合并模组散图到 Unity 永久素材目录，保留只在 Unity 中维护的图片。
    public static void SyncTextureImages()
    {
        if (!Directory.Exists(SourceTextureRoot))
        {
            throw new DirectoryNotFoundException($"BA 通用贴图源目录不存在：{SourceTextureRoot}");
        }

        List<string> sourceImagePaths = CollectSourceImagePaths();
        if (sourceImagePaths.Count == 0)
        {
            Debug.LogWarning($"[BA通用贴图AB] 源目录没有 PNG/JPG，跳过同步：{SourceTextureRoot}");
            return;
        }

        EnsureDirectory(GetUnityTextureAbsoluteRoot());

        foreach (string sourcePath in sourceImagePaths)
        {
            string relativePath = GetRelativePath(SourceTextureRoot, sourcePath);
            string unityPath = ToUnityPath(Path.Combine(UnityTextureRoot, relativePath));
            string targetFilePath = ToProjectAbsolutePath(unityPath);

            EnsureDirectory(Path.GetDirectoryName(targetFilePath));
            if (ShouldCopyFile(sourcePath, targetFilePath))
            {
                File.Copy(sourcePath, targetFilePath, true);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[BA通用贴图AB] 同步完成，源贴图 {sourceImagePaths.Count} 张。");
    }

    // 导入设置修正，负责把通用贴图限制到 512、关闭 mipmap 并使用 DXT5 压缩。
    public static void ConfigureTextureImporters()
    {
        foreach (List<string> assetPaths in CollectUnityImageAssetPathsByTopFolder().Values)
        {
            foreach (string assetPath in assetPaths)
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

                string bundleName = GetBundleNameForAssetPath(assetPath);
                AssetImporter assetImporter = importer;
                if (assetImporter.assetBundleName != bundleName)
                {
                    assetImporter.assetBundleName = bundleName;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
        }
    }

    // 图片收集，负责读取 Common\Textures 下当前仍存在的贴图文件。
    private static List<string> CollectSourceImagePaths()
    {
        List<string> sourceImagePaths = new List<string>();
        foreach (string sourcePath in Directory.GetFiles(SourceTextureRoot, "*.*", SearchOption.AllDirectories))
        {
            if (IsImagePath(sourcePath))
            {
                sourceImagePaths.Add(sourcePath);
            }
        }

        sourceImagePaths.Sort(StringComparer.OrdinalIgnoreCase);
        return sourceImagePaths;
    }

    // Unity 图片分组，负责按 Common\Textures 的顶层目录组织包体资源。
    private static Dictionary<string, List<string>> CollectUnityImageAssetPathsByTopFolder()
    {
        Dictionary<string, List<string>> groupedPaths = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string absoluteRoot = GetUnityTextureAbsoluteRoot();
        if (!Directory.Exists(absoluteRoot))
        {
            return groupedPaths;
        }

        HashSet<string> managedTopFolders = CollectManagedTopFolders();
        foreach (string filePath in Directory.GetFiles(absoluteRoot, "*.*", SearchOption.AllDirectories))
        {
            if (!IsImagePath(filePath))
            {
                continue;
            }

            string assetPath = ToUnityAssetPath(filePath);
            string topFolder = GetTopFolderFromAssetPath(assetPath);
            if (string.IsNullOrEmpty(topFolder) || !managedTopFolders.Contains(topFolder))
            {
                continue;
            }

            if (!groupedPaths.TryGetValue(topFolder, out List<string> assetPaths))
            {
                assetPaths = new List<string>();
                groupedPaths[topFolder] = assetPaths;
            }

            assetPaths.Add(assetPath);
        }

        foreach (List<string> assetPaths in groupedPaths.Values)
        {
            assetPaths.Sort(StringComparer.OrdinalIgnoreCase);
        }

        return groupedPaths;
    }

    // 构建映射，负责把每个顶层目录转换成一个 AssetBundleBuild。
    private static AssetBundleBuild[] CreateBuildMap(Dictionary<string, List<string>> groupedAssetPaths)
    {
        List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();
        foreach (KeyValuePair<string, List<string>> pair in groupedAssetPaths.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            buildMap.Add(new AssetBundleBuild
            {
                assetBundleName = GetBundleName(pair.Key),
                assetNames = pair.Value.ToArray()
            });
        }

        return buildMap.ToArray();
    }

    // 包验证，负责确认每个目录包中的资源数和 Unity 资源数一致。
    private static void ValidateBundles(Dictionary<string, List<string>> groupedAssetPaths)
    {
        foreach (KeyValuePair<string, List<string>> pair in groupedAssetPaths)
        {
            string bundleName = GetBundleName(pair.Key);
            string bundlePath = Path.Combine(OutputBundleRoot, bundleName);
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                throw new InvalidOperationException($"BA 通用贴图 AssetBundle 无法加载：{bundlePath}");
            }

            try
            {
                int bundleAssetCount = bundle.GetAllAssetNames().Length;
                if (bundleAssetCount != pair.Value.Count)
                {
                    throw new InvalidOperationException($"BA 通用贴图目录 {pair.Key} 资源数不一致：Unity资源 {pair.Value.Count}，AB资源 {bundleAssetCount}");
                }
            }
            finally
            {
                bundle.Unload(false);
            }
        }
    }

    // 过期清理，负责只删除本次源目录已不存在的通用同步图片，不触碰 Pawns 等其他工具管理的目录。
    private static void RemoveStaleSyncedImages(HashSet<string> expectedUnityPaths)
    {
        string absoluteRoot = GetUnityTextureAbsoluteRoot();
        if (!Directory.Exists(absoluteRoot))
        {
            return;
        }

        HashSet<string> managedTopFolders = CollectManagedTopFolders();
        foreach (string filePath in Directory.GetFiles(absoluteRoot, "*.*", SearchOption.AllDirectories))
        {
            if (!IsImagePath(filePath))
            {
                continue;
            }

            string unityPath = ToUnityAssetPath(filePath);
            string topFolder = GetTopFolderFromAssetPath(unityPath);
            if (!managedTopFolders.Contains(topFolder))
            {
                continue;
            }

            if (expectedUnityPaths.Contains(unityPath))
            {
                continue;
            }

            DeleteFileIfExists(filePath);
            DeleteFileIfExists(filePath + ".meta");
        }

        foreach (string topFolder in managedTopFolders)
        {
            string folderPath = Path.Combine(GetUnityTextureAbsoluteRoot(), topFolder);
            if (Directory.Exists(folderPath))
            {
                RemoveEmptyDirectories(folderPath);
            }
        }
    }

    // 顶层目录收集，负责把 Common\Textures 当前真实存在的目录作为本工具管理范围。
    private static HashSet<string> CollectManagedTopFolders()
    {
        HashSet<string> topFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string sourcePath in CollectSourceImagePaths())
        {
            string relativePath = GetRelativePath(SourceTextureRoot, sourcePath);
            string topFolder = GetFirstPathPart(relativePath);
            if (!string.IsNullOrEmpty(topFolder))
            {
                topFolders.Add(topFolder);
            }
        }

        return topFolders;
    }

    // 清理流程，负责删除本工具会重新生成的目录包和 Manifest。
    private static void CleanGeneratedBundles(IEnumerable<string> topFolders)
    {
        foreach (string topFolder in topFolders)
        {
            string bundlePath = Path.Combine(OutputBundleRoot, GetBundleName(topFolder));
            DeleteFileIfExists(bundlePath);
            DeleteFileIfExists(bundlePath + ".manifest");
        }
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

    // 包名转换，负责把顶层目录转成稳定的小写包名。
    private static string GetBundleName(string topFolder)
    {
        return BundleNamePrefix + topFolder.ToLowerInvariant();
    }

    // 包名转换，负责从 Unity 资源路径推导所属包名。
    private static string GetBundleNameForAssetPath(string assetPath)
    {
        return GetBundleName(GetTopFolderFromAssetPath(assetPath));
    }

    // 路径解析，负责从 Unity 资源路径提取 Textures 下的顶层目录。
    private static string GetTopFolderFromAssetPath(string assetPath)
    {
        string relativePath = assetPath.Substring(UnityTextureRoot.Length).TrimStart('/', '\\');
        return GetFirstPathPart(relativePath);
    }

    // 路径解析，负责提取路径的第一个目录名。
    private static string GetFirstPathPart(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return string.Empty;
        }

        string normalizedPath = ToUnityPath(relativePath);
        int slashIndex = normalizedPath.IndexOf('/');
        if (slashIndex < 0)
        {
            return Path.GetFileNameWithoutExtension(normalizedPath);
        }

        return normalizedPath.Substring(0, slashIndex);
    }

    //图片判断，限制通用工具范围，界面图片及其永久源文件由 UI 图片工具管理。
    private static bool IsImagePath(string path)
    {
        string normalized = path.Replace('\\', '/');
        foreach (string folder in new[] { "UI", "Gacha", "Stone", "Ability" })
        {
            if (normalized.IndexOf("/Textures/" + folder + "/", StringComparison.OrdinalIgnoreCase) >= 0) return false;
        }
        if (normalized.EndsWith("/Textures/QinghuiStone.png", StringComparison.OrdinalIgnoreCase)) return false;
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

    // 路径转换，负责得到 Unity 项目内通用贴图目录的绝对路径。
    private static string GetUnityTextureAbsoluteRoot()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.GetFullPath(Path.Combine(projectRoot, UnityTextureRoot));
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

    // 目录清理，负责删除同步后留下的空目录。
    private static void RemoveEmptyDirectories(string root)
    {
        foreach (string directory in Directory.GetDirectories(root))
        {
            RemoveEmptyDirectories(directory);
        }

        if (Directory.GetFiles(root).Length == 0 && Directory.GetDirectories(root).Length == 0)
        {
            Directory.Delete(root);
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
