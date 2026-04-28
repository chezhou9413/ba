using BANWlLib.BaDef;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace newpro
{
    public class imgcvT2d
    {
        public static string getRimWorldImgPath(string Rimworldpath)
        {
            string a = UiMapData.modRootPath + "/Common/Textures/" + Rimworldpath + ".png".Replace("/", "\\");
            return a;
        }
        /// <summary>
        /// 获取指定文件夹内所有 PNG 图像的路径映射表。
        /// </summary>
        /// <param name="folderPath">文件夹路径，可以是绝对路径或 Unity 支持的路径</param>
        /// <returns>文件名（无扩展名） -> 完整路径 的映射表</returns>
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
                string fileName = Path.GetFileNameWithoutExtension(filePath)+ "_race";
                pngMap[fileName] = filePath;
            }

            return pngMap;
        }
        
        /// <summary>
        /// 从指定路径加载图像，并转换为 Unity Sprite。
        /// </summary>
        /// <param name="path">图片路径（支持绝对路径或 StreamingAssets 相对路径）</param>
        /// <returns>返回生成的 Sprite，失败返回 null。</returns>
        public static Sprite LoadSpriteFromFile(string path)
        {
            try
            {
                
                string directory = Path.GetDirectoryName(path);
                string fileNameNoExt = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path);
                if (string.IsNullOrEmpty(fileNameNoExt))
                {
                    return null;
                }

                string pngPath = !string.IsNullOrEmpty(ext) ? Path.Combine(directory ?? string.Empty, fileNameNoExt + ".png") : path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? path : path + ".png";
                string ddsPath = !string.IsNullOrEmpty(ext) ? Path.Combine(directory ?? string.Empty, fileNameNoExt + ".dds") : path.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) ? path : path + ".dds";

                // 1) 先尝试 PNG
                if (File.Exists(pngPath))
                {
                    Sprite sp = CreateSpriteFromPng(pngPath);
                    if (sp != null)
                    {
                        return sp;
                    }
                    // PNG 存在但加载失败，继续尝试 DDS
                }

                // 2) 再尝试 DDS
                if (File.Exists(ddsPath))
                {
                    Sprite sp = CreateSpriteFromDds(ddsPath);
                    if (sp != null)
                    {
                        return sp;
                    }
                }

                // 3) 两者都没有
                Debug.LogError("图片文件不存在（PNG 与 DDS 均未找到）：" + pngPath + " | " + ddsPath);
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("加载 Sprite 时出错：" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 从 PNG 文件创建 Sprite（使用 Unity 内置解码）。
        /// </summary>
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

        /// <summary>
        /// 从 DDS 文件创建 Sprite（支持 DXT1/DXT5）。
        /// 流程解析：
        /// 1) 校验文件魔数 "DDS "；2) 解析宽高、像素格式 FourCC；3) 读取纹理数据并按压缩格式写入 Texture2D；4) 生成 Sprite。
        /// </summary>
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

        /// <summary>
        /// 读取 DDS 文件为 Texture2D（最小实现：支持 DXT1 / DXT5 压缩）。
        /// 注意：Unity 对压缩纹理格式的支持依赖平台；RimWorld Windows 环境支持 DXT1/DXT5。
        /// </summary>
        private static Texture2D LoadTextureFromDDS(string ddsPath)
        {
            byte[] bytes = File.ReadAllBytes(ddsPath);
            if (bytes == null || bytes.Length < 128)
            {
                Debug.LogError("DDS 文件无效（长度不足）：" + ddsPath);
                return null;
            }

            // 1) 魔数校验：前 4 字节应为 'D','D','S',' '
            if (!(bytes[0] == 0x44 && bytes[1] == 0x44 && bytes[2] == 0x53 && bytes[3] == 0x20))
            {
                Debug.LogError("DDS 魔数不匹配：" + ddsPath);
                return null;
            }

            try
            {
                // 2) 解析关键头部字段（以下偏移量均以文件起始为基准）
                int height = BitConverter.ToInt32(bytes, 12);   // dwHeight
                int width = BitConverter.ToInt32(bytes, 16);    // dwWidth
                int mipMapCount = Math.Max(1, BitConverter.ToInt32(bytes, 28)); // dwMipMapCount，最少 1
                int fourCC = BitConverter.ToInt32(bytes, 84);   // ddspf.fourCC

                // 3) 判断压缩格式
                TextureFormat textureFormat;
                bool isCompressed = true;
                const int FOURCC_DXT1 = 0x31545844; // 'DXT1'
                const int FOURCC_DXT5 = 0x35545844; // 'DXT5'

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
                    // 最小实现仅支持 DXT1/DXT5，其余格式直接报错
                    Debug.LogError("不支持的 DDS FourCC（仅支持 DXT1/DXT5）：0x" + fourCC.ToString("X") + " 路径：" + ddsPath);
                    return null;
                }

                // 4) 提取像素数据（头部固定 128 字节）
                int dataOffset = 128;
                int dataSize = bytes.Length - dataOffset;
                if (dataSize <= 0)
                {
                    Debug.LogError("DDS 像素数据为空：" + ddsPath);
                    return null;
                }

                byte[] pixelData = new byte[dataSize];
                Buffer.BlockCopy(bytes, dataOffset, pixelData, 0, dataSize);

                // 5) 创建 Texture2D 并写入压缩数据
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
