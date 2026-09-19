using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BANWlLib.mainUI.Images
{
    //管理 UI 专用资源包句柄，包内最后一张缓存图片释放时卸载整个包。
    internal static class BAUIImageBundles
    {
        private static readonly Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        internal static int Count => bundles.Count;

        //只加载本次需要的纹理；缺包允许调用方使用散图，损坏的包直接报错。
        internal static Texture2D Load(BAUIImageRecord record)
        {
            if (!bundles.TryGetValue(record.Bundle, out AssetBundle bundle))
            {
                string path = Path.Combine(BAUIImagePath.Root, "1.6/AssetBundles/UIImage", record.Bundle);
                if (!File.Exists(path)) return null;
                bundle = AssetBundle.LoadFromFile(path);
                if (bundle == null) throw new InvalidDataException("无法加载 UI 图片包：" + path);
                bundles.Add(record.Bundle, bundle);
                counts.Add(record.Bundle, 0);
            }
            Texture2D texture = bundle.LoadAsset<Texture2D>(record.Asset);
            if (texture == null)
            {
                if (counts[record.Bundle] == 0) Close(record.Bundle);
                throw new InvalidDataException("UI 图片包缺少资源：" + record.Bundle + " / " + record.Asset);
            }
            counts[record.Bundle]++;
            return texture;
        }

        //释放缓存纹理；仍有其他缓存图片时只卸载当前纹理。
        internal static void Release(string name, Texture2D texture)
        {
            counts[name]--;
            if (counts[name] == 0) Close(name);
            else Resources.UnloadAsset(texture);
        }

        //卸载零引用包并删除句柄登记。
        private static void Close(string name)
        {
            bundles[name].Unload(true);
            bundles.Remove(name);
            counts.Remove(name);
        }
    }
}
