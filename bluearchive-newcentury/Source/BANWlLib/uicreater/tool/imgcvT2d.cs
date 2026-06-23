using BANWlLib.BaDef;
using BANWlLib.mainUI.ManualUI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace newpro
{
    //图片转换工具负责把磁盘图片、手册图片和 RimWorld 原生贴图统一转换为 Unity UI Sprite。
    public class imgcvT2d
    {
        //获取 RimWorld 图片路径，负责保留原版贴图路径并兼容手册图片的真实磁盘路径。
        public static string getRimWorldImgPath(string Rimworldpath)
        {
            if (BAManualUIImageLoader.IsManualUIImagePath(Rimworldpath))
            {
                return BAManualUIImageLoader.GetFilePath(Rimworldpath);
            }

            return Rimworldpath;
        }

        //获取指定文件夹内所有 PNG 图像的路径映射表。
        public static Dictionary<string, string> GetPngMap(string folderPath)
        {
            Dictionary<string, string> pngMap = new Dictionary<string, string>();

            if (!Directory.Exists(folderPath))
            {
                return pngMap;
            }

            string[] files = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                pngMap[fileName] = filePath;
            }

            return pngMap;
        }
        
        //从指定路径加载图像，并转换为 Unity Sprite。
        public static Sprite LoadSpriteFromFile(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                if (BAManualUIImageLoader.IsManualUIImagePath(path) || IsManualUIAbsolutePath(path))
                {
                    return BAManualUIImageLoader.GetSprite(path);
                }

                if (TryLoadLocalSprite(path, out Sprite localSprite))
                {
                    return localSprite;
                }

                Sprite rimWorldSprite = BAUIRimWorldSpriteLoader.GetSprite(path);
                if (rimWorldSprite != null)
                {
                    return rimWorldSprite;
                }

                Debug.LogError("图片资源不存在（本地 PNG/DDS 与 RimWorld 贴图均未找到）：" + path);
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("加载 Sprite 时出错：" + ex.Message);
                return null;
            }
        }

        //尝试从磁盘 PNG 或 DDS 加载 Sprite，负责保持旧的绝对路径图片读取能力。
        private static bool TryLoadLocalSprite(string path, out Sprite sprite)
        {
            sprite = null;
            string localPath = path;
            if (!Path.IsPathRooted(localPath))
            {
                localPath = BuildCommonTexturePath(localPath);
            }

            string directory = Path.GetDirectoryName(localPath);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(localPath);
            string ext = Path.GetExtension(localPath);
            if (string.IsNullOrEmpty(fileNameNoExt))
            {
                return false;
            }

            string pngPath = !string.IsNullOrEmpty(ext) ? Path.Combine(directory ?? string.Empty, fileNameNoExt + ".png") : localPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? localPath : localPath + ".png";
            string ddsPath = !string.IsNullOrEmpty(ext) ? Path.Combine(directory ?? string.Empty, fileNameNoExt + ".dds") : localPath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) ? localPath : localPath + ".dds";

            if (File.Exists(pngPath))
            {
                sprite = CreateSpriteFromPng(pngPath);
                if (sprite != null)
                {
                    return true;
                }
            }

            if (File.Exists(ddsPath))
            {
                sprite = CreateSpriteFromDds(ddsPath);
                if (sprite != null)
                {
                    return true;
                }
            }

            return false;
        }

        //把 RimWorld 相对贴图路径转换为 Common/Textures 下的本地候选路径。
        private static string BuildCommonTexturePath(string path)
        {
            string relativePath = path.Replace('\\', '/').Trim('/');
            if (relativePath.StartsWith("Common/Textures/", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Substring("Common/Textures/".Length);
            }

            return Path.Combine(UiMapData.modRootPath, "Common", "Textures", relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        //判断路径是否已经是手册图片真实磁盘路径。
        private static bool IsManualUIAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.Replace('\\', '/').IndexOf("/Common/UIAssets/ManuaUI/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        //从 PNG 文件创建 Sprite。
        private static Sprite CreateSpriteFromPng(string pngPath)
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(pngPath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(imageData))
                {
                    Debug.LogError("加载 PNG 失败：" + pngPath);
                    return null;
                }
                texture.Apply();
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Vector2 pivot = new Vector2(0.5f, 0.5f);
                return Sprite.Create(texture, rect, pivot, 100f);
            }
            catch (Exception e)
            {
                Debug.LogError("加载 PNG 时异常：" + e.Message);
                return null;
            }
        }

        //从 DDS 文件创建 Sprite。
        private static Sprite CreateSpriteFromDds(string ddsPath)
        {
            try
            {
                Texture2D tex = LoadTextureFromDDS(ddsPath);
                if (tex == null)
                {
                    return null;
                }
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                Rect rect = new Rect(0, 0, tex.width, tex.height);
                Vector2 pivot = new Vector2(0.5f, 0.5f);
                return Sprite.Create(tex, rect, pivot, 100f);
            }
            catch (Exception e)
            {
                Debug.LogError("加载 DDS 时异常：" + e.Message);
                return null;
            }
        }

        //读取 DDS 文件为 Texture2D。
        private static Texture2D LoadTextureFromDDS(string ddsPath)
        {
            byte[] bytes = File.ReadAllBytes(ddsPath);
            if (bytes == null || bytes.Length < 128)
            {
                Debug.LogError("DDS 文件无效（长度不足）：" + ddsPath);
                return null;
            }

            //校验 DDS 文件魔数。
            if (!(bytes[0] == 0x44 && bytes[1] == 0x44 && bytes[2] == 0x53 && bytes[3] == 0x20))
            {
                Debug.LogError("DDS 魔数不匹配：" + ddsPath);
                return null;
            }

            try
            {
                //解析 DDS 头部字段。
                int height = BitConverter.ToInt32(bytes, 12);
                int width = BitConverter.ToInt32(bytes, 16);
                int mipMapCount = Math.Max(1, BitConverter.ToInt32(bytes, 28));
                int fourCC = BitConverter.ToInt32(bytes, 84);

                //根据 FourCC 判断压缩格式。
                TextureFormat textureFormat;
                const int FOURCC_DXT1 = 0x31545844;
                const int FOURCC_DXT5 = 0x35545844;

                if (fourCC == FOURCC_DXT1)
                {
                    textureFormat = TextureFormat.DXT1;
                }
                else if (fourCC == FOURCC_DXT5)
                {
                    textureFormat = TextureFormat.DXT5;
                }
                else
                {
                    Debug.LogError("不支持的 DDS FourCC（仅支持 DXT1/DXT5）：0x" + fourCC.ToString("X") + " 路径：" + ddsPath);
                    return null;
                }

                //提取 DDS 头部后面的像素数据。
                int dataOffset = 128;
                int dataSize = bytes.Length - dataOffset;
                if (dataSize <= 0)
                {
                    Debug.LogError("DDS 像素数据为空：" + ddsPath);
                    return null;
                }

                byte[] pixelData = new byte[dataSize];
                Buffer.BlockCopy(bytes, dataOffset, pixelData, 0, dataSize);

                //创建 Unity 纹理并写入 DDS 压缩数据。
                Texture2D tex = new Texture2D(width, height, textureFormat, mipMapCount > 1);
                tex.LoadRawTextureData(pixelData);
                tex.Apply(false, false);
                return tex;
            }
            catch (Exception e)
            {
                Debug.LogError("解析 DDS 失败：" + e.Message + " 路径：" + ddsPath);
                return null;
            }
        }
    }
}
