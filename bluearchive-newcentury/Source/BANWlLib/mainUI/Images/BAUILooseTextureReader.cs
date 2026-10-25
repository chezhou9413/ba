using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using System;
using System.IO;
using RimUnity;

namespace BANWlLib.mainUI.Images
{
    //解码散图并移除 CPU 像素副本，输出由 UI 缓存拥有的纹理。
    internal static class BAUILooseTextureReader
    {
        //读取 PNG、JPEG 或 DXT1/DXT5 DDS，失败时销毁已创建的对象并上报错误。
        internal static Texture2D Read(string path)
        {
            Texture2D texture = null;
            try
            {
                texture = Texture2D.LoadFile(path);
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
