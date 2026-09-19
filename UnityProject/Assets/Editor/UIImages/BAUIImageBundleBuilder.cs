using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

//构建 UI 专用图片包、原版共享图标包与路径索引，只输出到模组工程。
public static class BAUIImageBundleBuilder
{
    private static readonly string Staging = Path.Combine(BABuildPaths.StagingRoot, "UIImages");

    //使用 Unity 工程内的图片源重新构建，保留模组侧外加散图。
    [MenuItem("RimWorldTools/BA界面图片/打包全部UI图片")]
    public static void Build()
    {
        BuildCore(false);
    }

    //把模组散图迁入 Unity，在成功构建后移除模组中的相同源文件。
    [MenuItem("RimWorldTools/BA界面图片/迁移模组散图并打包")]
    public static void MigrateAndBuild()
    {
        BAUIImageBuildSources.ImportModImages();
        BuildCore(true);
    }

    //按分组构建并核对包内路径，所有包成功后更新运行时索引。
    private static void BuildCore(bool removeSources)
    {
        List<BAUIImageBuildItem> items = BAUIImageBuildSources.Collect();
        BAUIImageBuildSources.Configure(items);
        AssetBundleBuild[] builds = items.GroupBy(i => i.Bundle).Select(group => new AssetBundleBuild
        {
            assetBundleName = group.Key,
            assetNames = group.Select(i => i.Asset).ToArray()
        }).ToArray();
        Directory.CreateDirectory(Staging);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(Staging, builds,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
        if (manifest == null) throw new InvalidOperationException("UI 图片包构建失败。");
        foreach (AssetBundleBuild build in builds)
        {
            if (manifest.GetAllDependencies(build.assetBundleName).Length != 0)
                throw new InvalidOperationException("UI 图片包不应依赖其他包：" + build.assetBundleName);
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Staging, build.assetBundleName));
            if (bundle == null) throw new InvalidOperationException("无法读取已构建的图片包：" + build.assetBundleName);
            try
            {
                var names = new HashSet<string>(bundle.GetAllAssetNames(), StringComparer.OrdinalIgnoreCase);
                if (names.Count != build.assetNames.Length || build.assetNames.Any(asset => !names.Contains(asset)))
                    throw new InvalidOperationException("图片包条目与构建清单不一致：" + build.assetBundleName);
            }
            finally { bundle.Unload(true); }
        }
        string output = Path.Combine(BAUIImageBuildSources.ModRoot, "1.6/AssetBundles/UIImage");
        Directory.CreateDirectory(output);
        foreach (AssetBundleBuild build in builds)
        {
            string destination = build.assetBundleName == "banw_textures_ability" ? Path.GetDirectoryName(output) : output;
            File.Copy(Path.Combine(Staging, build.assetBundleName), Path.Combine(destination, build.assetBundleName), true);
            File.Copy(Path.Combine(Staging, build.assetBundleName + ".manifest"), Path.Combine(destination, build.assetBundleName + ".manifest"), true);
        }
        var root = new XElement("UIImageBundles", new XAttribute("version", 1));
        foreach (BAUIImageBuildItem item in items)
            root.Add(new XElement("Image", new XAttribute("key", item.Key), new XAttribute("source", item.Source),
                new XAttribute("bundle", item.Bundle), new XAttribute("asset", item.Asset.ToLowerInvariant()),
                new XAttribute("shared", item.Shared), new XAttribute("transient", item.Transient)));
        string catalog = Path.Combine(BAUIImageBuildSources.ModRoot, "1.6/UI/images/bundles.xml");
        Directory.CreateDirectory(Path.GetDirectoryName(catalog));
        new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(catalog);
        RemoveStaleBundles(output, builds);
        if (removeSources) BAUIImageBuildSources.RemoveMigratedImages(items);
        Debug.Log($"[BA UI图片] 构建成功：{items.Count} 张图片，{builds.Length} 个包。输出：{output}");
    }

    //只清理本工具专属输出目录中已经不在清单内的包与清单文件。
    private static void RemoveStaleBundles(string output, AssetBundleBuild[] builds)
    {
        string fullOutput = Path.GetFullPath(output);
        string expected = Path.GetFullPath(Path.Combine(BAUIImageBuildSources.ModRoot, "1.6/AssetBundles/UIImage"));
        if (!string.Equals(fullOutput, expected, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("UI 图片输出目录不匹配。");
        var keep = new HashSet<string>(builds.SelectMany(b => new[] { b.assetBundleName, b.assetBundleName + ".manifest" }), StringComparer.OrdinalIgnoreCase);
        foreach (string file in Directory.GetFiles(fullOutput))
            if ((Path.GetFileName(file).StartsWith("ui_", StringComparison.Ordinal) || Path.GetFileName(file).StartsWith("banw_ui_shared_win", StringComparison.Ordinal)) && !keep.Contains(Path.GetFileName(file)))
                File.Delete(file);
    }
}
