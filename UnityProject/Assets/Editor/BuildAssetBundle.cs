using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//负责合并特效贴图到 Unity 素材目录，并按一张图一个包输出 Windows 资源包。
public static class BAEffectBundleBuilder
{
    private static readonly string SourceEffectRoot = Path.Combine(BABuildPaths.ModRoot, "Common/Textures/Effect");
    private const string UnityEffectRoot = "Assets/EffectImages";
    private static readonly string OutputEffectRoot = SourceEffectRoot;

    private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg"
    };

    // Unity 菜单入口，负责把 Mod 源目录中的特效贴图同步到 Unity 项目 Assets 目录。
    [MenuItem("RimWorldTools/BA特效/同步特效贴图")]
    public static void SyncEffectImagesMenu()
    {
        SyncEffectImages();
        ConfigureEffectTextureImporters();
        EditorUtility.DisplayDialog("完成", "BA 特效贴图同步完成。", "确定");
    }

    // Unity 菜单入口，负责同步贴图并打包全部 BA 特效 AssetBundle。
    [MenuItem("RimWorldTools/BA特效/一键打包全部特效AB")]
    public static void BuildAllMenu()
    {
        BuildAll();
        EditorUtility.DisplayDialog("完成", "BA 特效 AssetBundle 打包完成。", "确定");
    }

    // 命令行入口，负责让 Unity batchmode 调用完整同步和打包流程。
    public static void BuildAllFromCommandLine()
    {
        BuildAll();
    }

    // 完整构建流程，负责刷新源贴图、清理旧包并输出 Windows AssetBundle。
    public static void BuildAll()
    {
        SyncEffectImages();
        AssetDatabase.Refresh();
        ConfigureEffectTextureImporters();
        AssetDatabase.Refresh();

        List<string> assetPaths = CollectUnityImageAssetPaths();
        if (assetPaths.Count == 0)
        {
            throw new InvalidOperationException("没有找到可打包的 BA 特效贴图。");
        }

        EnsureDirectory(OutputEffectRoot);
        CleanGeneratedBundles(assetPaths);

        AssetBundleBuild[] buildMap = CreateBuildMap(assetPaths);
        EnsureBundleOutputDirectories(buildMap);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            OutputEffectRoot,
            buildMap,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.StandaloneWindows64);

        if (manifest == null)
        {
            throw new InvalidOperationException("BA 特效 AssetBundle 打包失败，Unity 没有返回 Manifest。");
        }

        Debug.Log($"[BA特效AB] 打包完成，共输出 {buildMap.Length} 个 Windows AssetBundle 到 {OutputEffectRoot}");
    }

    //负责合并模组特效散图，保留只在 Unity 中维护的永久源图。
    public static void SyncEffectImages()
    {
        if (!Directory.Exists(SourceEffectRoot))
        {
            throw new DirectoryNotFoundException($"BA 特效源目录不存在：{SourceEffectRoot}");
        }

        List<string> sourceImagePaths = CollectSourceImagePaths();
        if (sourceImagePaths.Count == 0)
        {
            Debug.LogWarning($"[BA特效AB] 源目录没有 PNG/JPG，跳过同步和过期删除，保留 Unity 项目内已有贴图：{SourceEffectRoot}");
            return;
        }

        EnsureDirectory(GetUnityEffectAbsoluteRoot());

        foreach (string sourcePath in sourceImagePaths)
        {
            string relativePath = GetRelativePath(SourceEffectRoot, sourcePath);
            string unityPath = ToUnityPath(Path.Combine(UnityEffectRoot, relativePath));
            string targetFilePath = ToProjectAbsolutePath(unityPath);

            EnsureDirectory(Path.GetDirectoryName(targetFilePath));
            if (ShouldCopyFile(sourcePath, targetFilePath))
            {
                File.Copy(sourcePath, targetFilePath, true);
            }
        }

        AssetDatabase.Refresh();
    }

    // 导入设置修正，负责让 AB 内贴图保持原始分辨率、关闭 mipmap 和压缩，避免特效发糊或长条图被缩放。
    public static void ConfigureEffectTextureImporters()
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
            changed |= SetImporterValue(importer.npotScale != TextureImporterNPOTScale.None, () => importer.npotScale = TextureImporterNPOTScale.None);
            changed |= SetImporterValue(importer.textureCompression != TextureImporterCompression.Uncompressed, () => importer.textureCompression = TextureImporterCompression.Uncompressed);
            changed |= SetImporterValue(importer.maxTextureSize < 8192, () => importer.maxTextureSize = 8192);
            changed |= SetImporterValue(importer.filterMode != FilterMode.Bilinear, () => importer.filterMode = FilterMode.Bilinear);
            changed |= SetImporterValue(importer.wrapMode != TextureWrapMode.Clamp, () => importer.wrapMode = TextureWrapMode.Clamp);
            changed |= SetImporterValue(!importer.alphaIsTransparency, () => importer.alphaIsTransparency = true);

            TextureImporterPlatformSettings standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
            bool standaloneChanged = false;
            standaloneChanged |= standaloneSettings.overridden != true;
            standaloneChanged |= standaloneSettings.maxTextureSize != 8192;
            standaloneChanged |= standaloneSettings.textureCompression != TextureImporterCompression.Uncompressed;
            standaloneChanged |= standaloneSettings.format != TextureImporterFormat.RGBA32;

            if (standaloneChanged)
            {
                standaloneSettings.overridden = true;
                standaloneSettings.maxTextureSize = 8192;
                standaloneSettings.textureCompression = TextureImporterCompression.Uncompressed;
                standaloneSettings.format = TextureImporterFormat.RGBA32;
                importer.SetPlatformTextureSettings(standaloneSettings);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
    }

    // 图片收集，负责读取 Mod 源目录下当前仍存在的原始贴图。
    private static List<string> CollectSourceImagePaths()
    {
        List<string> sourceImagePaths = new List<string>();
        foreach (string sourcePath in Directory.GetFiles(SourceEffectRoot, "*.*", SearchOption.AllDirectories))
        {
            if (IsImagePath(sourcePath))
            {
                sourceImagePaths.Add(sourcePath);
            }
        }

        sourceImagePaths.Sort(StringComparer.OrdinalIgnoreCase);
        return sourceImagePaths;
    }

    // 导入设置赋值，负责只在值确实变化时标记需要重新导入。
    private static bool SetImporterValue(bool shouldChange, Action applyChange)
    {
        if (!shouldChange)
        {
            return false;
        }

        applyChange();
        return true;
    }

    // 收集流程，负责按稳定顺序返回 Unity 项目内所有特效图片资源路径。
    private static List<string> CollectUnityImageAssetPaths()
    {
        List<string> assetPaths = new List<string>();
        string absoluteRoot = GetUnityEffectAbsoluteRoot();
        if (!Directory.Exists(absoluteRoot))
        {
            return assetPaths;
        }

        foreach (string filePath in Directory.GetFiles(absoluteRoot, "*.*", SearchOption.AllDirectories))
        {
            if (!IsImagePath(filePath))
            {
                continue;
            }

            assetPaths.Add(ToUnityAssetPath(filePath));
        }

        assetPaths.Sort(StringComparer.OrdinalIgnoreCase);
        return assetPaths;
    }

    // 构建映射，负责把每张图片转换成单独的 AssetBundleBuild 配置。
    private static AssetBundleBuild[] CreateBuildMap(List<string> assetPaths)
    {
        AssetBundleBuild[] buildMap = new AssetBundleBuild[assetPaths.Count];
        for (int index = 0; index < assetPaths.Count; index++)
        {
            string assetPath = assetPaths[index];
            buildMap[index] = new AssetBundleBuild
            {
                assetBundleName = GetBundleName(assetPath),
                assetNames = new[] { assetPath }
            };
        }

        return buildMap;
    }

    // 输出准备，负责提前创建每个嵌套包名对应的目录。
    private static void EnsureBundleOutputDirectories(AssetBundleBuild[] buildMap)
    {
        foreach (AssetBundleBuild build in buildMap)
        {
            string bundlePath = Path.Combine(OutputEffectRoot, build.assetBundleName.Replace('/', Path.DirectorySeparatorChar));
            string bundleDirectory = Path.GetDirectoryName(bundlePath);
            EnsureDirectory(bundleDirectory);
        }
    }

    // 清理流程，负责删除本工具会重新生成的旧 AB 和 Manifest，不触碰源图片。
    private static void CleanGeneratedBundles(List<string> assetPaths)
    {
        foreach (string assetPath in assetPaths)
        {
            string bundlePath = Path.Combine(OutputEffectRoot, GetBundleName(assetPath).Replace('/', Path.DirectorySeparatorChar));
            DeleteFileIfExists(bundlePath);
            DeleteFileIfExists(bundlePath + ".manifest");
        }
    }

    // 过期清理，负责删除 Unity 项目里源目录已经不存在的旧图片和对应 meta 文件。
    private static void RemoveStaleUnityImages(HashSet<string> expectedUnityPaths)
    {
        string absoluteRoot = GetUnityEffectAbsoluteRoot();
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

        if (root == Path.GetFullPath(UnityEffectRoot))
        {
            return;
        }

        if (Directory.GetFiles(root).Length == 0 && Directory.GetDirectories(root).Length == 0)
        {
            Directory.Delete(root);
        }
    }

    // 包名转换，负责把 Unity 资源路径转换成和原 texPath 对齐的相对 AB 路径。
    private static string GetBundleName(string assetPath)
    {
        string relativePath = assetPath.Substring(UnityEffectRoot.Length).TrimStart('/', '\\');
        string withoutExtension = Path.ChangeExtension(relativePath, ".ab");
        return ToUnityPath(withoutExtension);
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

    // 路径转换，负责得到 Unity 项目内特效图片目录的绝对路径。
    private static string GetUnityEffectAbsoluteRoot()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.GetFullPath(Path.Combine(projectRoot, UnityEffectRoot));
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
