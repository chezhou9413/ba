namespace BANWlLib.mainUI.Images
{
    //保存索引中的单张图片位置与共享属性，不持有 Unity 图片对象。
    internal sealed class BAUIImageRecord
    {
        internal string Bundle;
        internal string Asset;
        internal bool Shared;
        internal bool Transient;
    }
}
