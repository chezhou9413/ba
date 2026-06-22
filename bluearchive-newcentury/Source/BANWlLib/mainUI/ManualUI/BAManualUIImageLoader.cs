using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace BANWlLib.mainUI.ManualUI
{
    //手册图片加载器负责把 Def 中的 ManuaUI 路径映射到非 Textures 目录，并按需创建和释放 UI Sprite。
    public static class BAManualUIImageLoader
    {
        private static readonly Dictionary<string, ManualSpriteEntry> SharedCache = new Dictionary<string, ManualSpriteEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ManualSpriteEntry> DetailCache = new Dictionary<string, ManualSpriteEntry>(StringComparer.OrdinalIgnoreCase);

        //判断路径是否属于手册 UI 图片目录。
        public static bool IsManualUIImagePath(string rimWorldPath)
        {
            if (string.IsNullOrEmpty(rimWorldPath))
            {
                return false;
            }

            string normalizedPath = NormalizeRelativePath(rimWorldPath);
            return normalizedPath.StartsWith("ManuaUI/", StringComparison.OrdinalIgnoreCase) ||
                   normalizedPath.IndexOf("/Common/UIAssets/ManuaUI/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        //把 Def 中的 ManuaUI 相对路径转换为实际文件路径。
        public static string GetFilePath(string rimWorldPath)
        {
            string relativePath = NormalizeRelativePath(rimWorldPath);
            if (Path.IsPathRooted(relativePath))
            {
                return EnsurePngExtension(relativePath);
            }

            if (!relativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                relativePath += ".png";
            }

            string pathAfterRoot = relativePath.Substring("ManuaUI/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(GetManualUIRootPath(), pathAfterRoot);
        }

        //按 ManuaUI 相对路径获取 Sprite，资源首次显示时才从磁盘读取。
        public static Sprite GetSprite(string rimWorldPath)
        {
            if (string.IsNullOrEmpty(rimWorldPath))
            {
                return null;
            }

            string normalizedPath = NormalizeRelativePath(rimWorldPath);
            Dictionary<string, ManualSpriteEntry> cache = IsDetailImage(normalizedPath) ? DetailCache : SharedCache;
            if (cache.TryGetValue(normalizedPath, out ManualSpriteEntry cachedEntry))
            {
                return cachedEntry.Sprite;
            }

            string filePath = GetFilePath(normalizedPath);
            ManualSpriteEntry entry = CreateSpriteEntry(filePath);
            cache[normalizedPath] = entry;
            return entry.Sprite;
        }

        //释放详情页大图，保留列表头像和小图缓存。
        public static void ClearDetailImages()
        {
            ClearCache(DetailCache);
        }

        //释放所有由手册图片加载器创建的 Sprite 和 Texture2D。
        public static void ClearAll()
        {
            ClearCache(DetailCache);
            ClearCache(SharedCache);
        }

        //标准化 Def 图片路径，统一斜杠和扩展名格式。
        private static string NormalizeRelativePath(string rimWorldPath)
        {
            string normalizedPath = rimWorldPath.Replace('\\', '/').Trim();
            if (normalizedPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - ".png".Length);
            }

            return normalizedPath;
        }

        //补齐 PNG 扩展名，避免调用方传入无扩展路径时找不到文件。
        private static string EnsurePngExtension(string path)
        {
            if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            return path + ".png";
        }

        //获取手册 UI 图片根目录，优先使用已经初始化的 Mod 根路径。
        private static string GetManualUIRootPath()
        {
            string modRootPath = UiMapData.modRootPath;
            if (string.IsNullOrEmpty(modRootPath))
            {
                modRootPath = LoadedModManager.GetMod<BANWlLib.newpro>().Content.RootDir;
            }

            return Path.Combine(modRootPath, "Common", "UIAssets", "ManuaUI");
        }

        //判断资源是否属于学生详情页的大图缓存。
        private static bool IsDetailImage(string normalizedPath)
        {
            return normalizedPath.StartsWith("ManuaUI/Live/", StringComparison.OrdinalIgnoreCase) ||
                   normalizedPath.IndexOf("/Common/UIAssets/ManuaUI/Live/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   normalizedPath.IndexOf("/Common/UIAssets/ManuaUI/Bg/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   normalizedPath.StartsWith("ManuaUI/Bg/", StringComparison.OrdinalIgnoreCase);
        }

        //从 PNG 文件创建一组 Sprite 和 Texture2D 缓存对象。
        private static ManualSpriteEntry CreateSpriteEntry(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error("[BA手册图片] 图片文件不存在：" + filePath);
                return ManualSpriteEntry.Empty;
            }

            byte[] imageData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(imageData))
            {
                UnityEngine.Object.Destroy(texture);
                Log.Error("[BA手册图片] PNG 解码失败：" + filePath);
                return ManualSpriteEntry.Empty;
            }

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply(false, true);

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            return new ManualSpriteEntry(sprite, texture);
        }

        //释放指定缓存中的所有 Unity 对象。
        private static void ClearCache(Dictionary<string, ManualSpriteEntry> cache)
        {
            foreach (ManualSpriteEntry entry in cache.Values)
            {
                entry.Destroy();
            }

            cache.Clear();
        }
    }

    //手册图片缓存项负责成对保存 Sprite 和底层 Texture2D。
    internal sealed class ManualSpriteEntry
    {
        public static readonly ManualSpriteEntry Empty = new ManualSpriteEntry(null, null);

        public readonly Sprite Sprite;
        private readonly Texture2D texture;

        //创建手册图片缓存项。
        public ManualSpriteEntry(Sprite sprite, Texture2D texture)
        {
            Sprite = sprite;
            this.texture = texture;
        }

        //销毁缓存项持有的 Unity 图片对象。
        public void Destroy()
        {
            if (Sprite != null)
            {
                UnityEngine.Object.Destroy(Sprite);
            }

            if (texture != null)
            {
                UnityEngine.Object.Destroy(texture);
            }
        }
    }
}
