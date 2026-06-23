using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace BANWlLib.mainUI.ManualUI
{
    //RimWorld UI 贴图加载器负责把原版 texPath 转成 Unity UI 可用的 Sprite。
    public static class BAUIRimWorldSpriteLoader
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        //按 RimWorld 原生贴图路径获取 Sprite，负责让 ContentFinder 自己处理普通贴图和 AB 贴图。
        public static Sprite GetSprite(string rimWorldPath)
        {
            string texPath = NormalizeTexPath(rimWorldPath);
            if (string.IsNullOrEmpty(texPath))
            {
                return null;
            }

            if (SpriteCache.TryGetValue(texPath, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Texture2D texture = ContentFinder<Texture2D>.Get(texPath, false);
            if (texture == null)
            {
                Log.Error("[BA UI贴图] RimWorld 贴图未找到：" + texPath);
                return null;
            }

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            SpriteCache[texPath] = sprite;
            return sprite;
        }

        //释放 Sprite 包装对象，负责在 UI 重建和全局重置时清空 UI 层缓存。
        public static void ClearAll()
        {
            foreach (Sprite sprite in SpriteCache.Values)
            {
                if (sprite != null)
                {
                    UnityEngine.Object.Destroy(sprite);
                }
            }

            SpriteCache.Clear();
        }

        //标准化 texPath，负责兼容 Def 相对路径、Common/Textures 路径和旧绝对磁盘路径。
        public static string NormalizeTexPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            string normalized = path.Trim().Replace('\\', '/');
            int commonTexturesIndex = normalized.IndexOf("/Common/Textures/", StringComparison.OrdinalIgnoreCase);
            if (commonTexturesIndex >= 0)
            {
                normalized = normalized.Substring(commonTexturesIndex + "/Common/Textures/".Length);
            }
            else if (normalized.StartsWith("Common/Textures/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("Common/Textures/".Length);
            }

            string extension = Path.GetExtension(normalized);
            if (!string.IsNullOrEmpty(extension) &&
                (extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                 extension.Equals(".dds", StringComparison.OrdinalIgnoreCase)))
            {
                normalized = normalized.Substring(0, normalized.Length - extension.Length);
            }

            return normalized.Trim('/');
        }
    }
}
