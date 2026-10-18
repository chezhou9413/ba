#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using System;
using System.IO;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif

namespace BANWlLib.mainUI.Images
{
    //解码散图并移除 CPU 像素副本，输出由 UI 缓存拥有的纹理。
    internal static class BAUILooseTextureReader
    {
        //读取 PNG、JPEG 或 DXT1/DXT5 DDS，失败时销毁已创建的对象并上报错误。
        internal static Texture2D Read(string path)
        {
#if !RW_1_7
            byte[] bytes = File.ReadAllBytes(path);
#endif
            Texture2D texture = null;
            try
            {
#if RW_1_7
                texture = Texture2D.LoadFile(path);
#else
                if (Path.GetExtension(path).Equals(".dds", StringComparison.OrdinalIgnoreCase))
                {
                    if (bytes.Length < 128 || BitConverter.ToUInt32(bytes, 0) != 0x20534444)
                        throw new InvalidDataException("DDS 文件头无效：" + path);
                    int fourCC = BitConverter.ToInt32(bytes, 84);
                    if (fourCC != 0x31545844 && fourCC != 0x35545844)
                        throw new InvalidDataException("DDS 仅支持 DXT1/DXT5：" + path);
                    texture = new Texture2D(BitConverter.ToInt32(bytes, 16), BitConverter.ToInt32(bytes, 12),
                        fourCC == 0x31545844 ? TextureFormat.DXT1 : TextureFormat.DXT5, BitConverter.ToInt32(bytes, 28) > 1);
                    byte[] pixels = new byte[bytes.Length - 128];
                    Buffer.BlockCopy(bytes, 128, pixels, 0, pixels.Length);
                    texture.LoadRawTextureData(pixels);
                    texture.Apply(false, true);
                }
                else
                {
                    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!texture.LoadImage(bytes, true)) throw new InvalidDataException("图片解码失败：" + path);
                }
#endif
                texture.name = path;
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                return texture;
            }
            catch
            {
                if (texture != null) Engine.Object.Destroy(texture);
                throw;
            }
        }
    }
}
