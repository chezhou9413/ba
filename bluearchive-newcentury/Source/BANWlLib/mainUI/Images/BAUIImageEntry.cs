#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif

namespace BANWlLib.mainUI.Images
{
    //记录图片对象、所有权、显示引用数量与闲置时间。
    internal sealed class BAUIImageEntry
    {
        internal string Key;
        internal Sprite Sprite;
        internal Texture2D Texture;
        internal string Bundle;
        internal bool Borrowed;
        internal bool Transient;
        internal int References;
        internal float LastUse;
        internal long Bytes;
    }
}
