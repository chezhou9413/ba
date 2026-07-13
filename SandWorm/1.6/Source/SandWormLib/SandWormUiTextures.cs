using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormUiTextures 负责统一缓存和绘制沙海 UI 位图素材，避免每帧重复查找资源。
    public static class SandWormUiTextures
    {
        public const string ContractBasePath = "UI/SandWorm/Contract/";

        private static readonly Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>();

        // Get 负责按 RimWorld 贴图路径静默读取素材，缺失时返回 null 供调用方使用程序绘制兜底。
        public static Texture2D Get(string path)
        {
            if (path.NullOrEmpty())
            {
                return null;
            }

            if (!TextureCache.TryGetValue(path, out Texture2D texture))
            {
                texture = ContentFinder<Texture2D>.Get(path, false);
                TextureCache[path] = texture;
            }

            return texture;
        }

        // Draw 负责在指定矩形中绘制素材，并在结束后恢复 GUI.color。
        public static bool Draw(Rect rect, string path, Color color, ScaleMode scaleMode = ScaleMode.StretchToFill)
        {
            Texture2D texture = Get(path);
            if (texture == null || rect.width <= 0f || rect.height <= 0f)
            {
                return false;
            }

            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, texture, scaleMode);
            GUI.color = oldColor;
            return true;
        }
    }
}
