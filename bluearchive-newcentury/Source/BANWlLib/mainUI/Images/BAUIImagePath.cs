using System;
using System.IO;
using newpro;
using Verse;

namespace BANWlLib.mainUI.Images
{
    //统一配置路径、模组相对路径与磁盘路径，保持图片索引键稳定。
    internal static class BAUIImagePath
    {
        internal static readonly string[] Extensions = { ".png", ".jpg", ".jpeg", ".dds" };
        internal static string Root => string.IsNullOrEmpty(UiMapData.modRootPath)
            ? LoadedModManager.GetMod<BANWlLib.newpro>().Content.RootDir : UiMapData.modRootPath;

        //去掉图片扩展名，保留文件名中的其他点号。
        internal static string WithoutExtension(string path)
        {
            foreach (string extension in Extensions)
                if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    return path.Substring(0, path.Length - extension.Length);
            return path;
        }

        //把同一资源的相对路径和本模组绝对路径转换成同一个缓存键。
        internal static string Key(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            string key = path.Trim().Replace('\\', '/');
            string root = Path.GetFullPath(Root).Replace('\\', '/').TrimEnd('/') + "/";
            if (key.StartsWith(root, StringComparison.OrdinalIgnoreCase)) key = key.Substring(root.Length);
            key = WithoutExtension(key);
            if (Path.IsPathRooted(key)) return key;
            key = key.TrimStart('/');
            if (key.StartsWith("ManuaUI/", StringComparison.OrdinalIgnoreCase)) return "Common/UIAssets/" + key;
            if (key.StartsWith("Common/", StringComparison.OrdinalIgnoreCase) || key.StartsWith("1.6/", StringComparison.OrdinalIgnoreCase)) return key;
            return "Common/Textures/" + key;
        }

        //按显式扩展名或支持的图片扩展名定位散图文件。
        internal static string FindLoose(string path, string key)
        {
            string candidate = Path.IsPathRooted(key) ? key : Path.Combine(Root, key);
            string extension = Path.GetExtension(path);
            foreach (string known in Extensions)
                if (extension.Equals(known, StringComparison.OrdinalIgnoreCase) && File.Exists(candidate + extension))
                    return candidate + extension;
            foreach (string known in Extensions)
                if (File.Exists(candidate + known)) return candidate + known;
            return null;
        }

        //转换为原版图标路径，其他目录不参与原版资源查找。
        internal static string NativePath(string key)
        {
            const string prefix = "Common/Textures/";
            return key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? key.Substring(prefix.Length) : null;
        }
    }
}
