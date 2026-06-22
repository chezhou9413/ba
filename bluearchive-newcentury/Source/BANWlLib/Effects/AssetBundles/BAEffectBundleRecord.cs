using System.Collections.Generic;
using UnityEngine;

namespace BANWlLib.Effects.AssetBundles
{
    // 特效 AB 记录，负责保存单个 AssetBundle 的贴图、材质和最近使用时间。
    internal sealed class BAEffectBundleRecord
    {
        public string texPath;
        public string bundlePath;
        public string assetPath;
        public AssetBundle bundle;
        public Texture2D texture;
        public int lastTouchTick;
        public readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
    }
}
