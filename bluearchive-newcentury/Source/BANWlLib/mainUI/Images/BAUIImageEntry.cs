using UnityEngine;

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
