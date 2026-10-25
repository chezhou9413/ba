using System.Collections.Generic;
using UnityEngine;

namespace BANWlLib.Effects.AssetBundles
{
    //特效资源记录，保存RimUnity贴图、原版材质和最近使用时间。
    internal sealed class BAEffectBundleRecord
    {
        public string texPath;
        public RimUnity.Texture2D ownedTexture;
        public Texture2D texture;
        public int lastTouchTick;
        public readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
    }
}
