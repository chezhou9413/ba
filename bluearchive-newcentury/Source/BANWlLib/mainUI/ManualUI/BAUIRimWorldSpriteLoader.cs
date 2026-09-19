using BANWlLib.mainUI.Images;

namespace BANWlLib.mainUI.ManualUI
{
    //提供原版共享图标包装对象的闲置缓存清理入口。
    public static class BAUIRimWorldSpriteLoader
    {
        //清理闲置精灵包装对象，原版纹理的生命周期仍由游戏管理。
        public static void ClearAll()
        {
            BAUIImageCache.Trim(true);
        }
    }
}
