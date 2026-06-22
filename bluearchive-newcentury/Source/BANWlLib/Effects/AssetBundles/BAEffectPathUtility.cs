using System;
using System.IO;
using System.Linq;
using newpro;
using Verse;

namespace BANWlLib.Effects.AssetBundles
{
    // 特效路径工具，负责把 RimWorld texPath 映射到一张贴图一个 AB 的磁盘路径和包内资源路径。
    internal static class BAEffectPathUtility
    {
        private const string EffectPrefix = "Effect/";
        private const string BundleRootRelative = "Common/Textures/Effect";
        private const string UnityAssetRoot = "Assets/EffectImages";

        // 判断贴图路径是否属于本次迁移的 BA 特效资源。
        public static bool IsEffectTexPath(string texPath)
        {
            return !string.IsNullOrWhiteSpace(texPath)
                && texPath.Replace('\\', '/').StartsWith(EffectPrefix, StringComparison.OrdinalIgnoreCase);
        }

        // 生成 AB 磁盘路径，文件名按当前已打包结果使用小写。
        public static string GetBundlePath(string texPath)
        {
            string relative = GetEffectRelativePath(texPath).ToLowerInvariant() + ".ab";
            return Path.Combine(GetModRoot(), BundleRootRelative, relative.Replace('/', Path.DirectorySeparatorChar));
        }

        // 从 manifest 解析包内资源路径，严格要求 manifest 存在。
        public static string GetAssetPathFromManifest(string bundlePath)
        {
            string manifestPath = bundlePath + ".manifest";
            if (!File.Exists(manifestPath))
            {
                Log.Error("[BA特效AB] 缺少 Manifest：" + manifestPath);
                return null;
            }

            string assetLine = File.ReadLines(manifestPath)
                .Select(line => line.Trim())
                .FirstOrDefault(line => line.StartsWith("- Assets/EffectImages/", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(assetLine))
            {
                Log.Error("[BA特效AB] Manifest 中没有资源路径：" + manifestPath);
                return null;
            }

            return assetLine.Substring(2).Trim().Trim('"');
        }

        // 生成默认包内资源路径，供错误日志和校验时对照。
        public static string GetExpectedUnityAssetPath(string texPath, string extension)
        {
            return UnityAssetRoot + "/" + GetEffectRelativePath(texPath) + extension;
        }

        // 去掉 Effect/ 前缀，得到和 Common/Textures/Effect 下资源一致的相对路径。
        private static string GetEffectRelativePath(string texPath)
        {
            string normalized = texPath.Trim().Replace('\\', '/');
            if (normalized.StartsWith(EffectPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return normalized.Substring(EffectPrefix.Length).Trim('/');
            }

            return normalized.Trim('/');
        }

        // 获取当前 Mod 根目录，负责定位 Common/Textures/Effect。
        private static string GetModRoot()
        {
            return LoadedModManager.GetMod<newpro>().Content.RootDir;
        }
    }
}
