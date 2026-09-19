using BANWlLib.mainUI.Images;

namespace BANWlLib.mainUI.ManualUI
{
    //提供手册界面的闲置图片清理入口，图片加载与引用由统一缓存管理。
    public static class BAManualUIImageLoader
    {
        //回收不再显示的图片，不影响仍被其他页面引用的对象。
        public static void ClearDetailImages()
        {
            BAUIImageCache.Trim(true);
        }

        //清理所有闲置手册缓存，活动引用由控件生命周期释放。
        public static void ClearAll()
        {
            BAUIImageCache.Trim(true);
        }
    }
}
