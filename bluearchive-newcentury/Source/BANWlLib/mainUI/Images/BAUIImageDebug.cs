using BANWlLib.Dev.Menus;
using Verse;
using LudeonTK;

namespace BANWlLib.mainUI.Images
{
    //提供 UI 图片缓存统计与手动回收入口。
    internal static class BAUIImageDebug
    {
        //输出显示、闲置和原版共享纹理的内存统计。
        [BADebugAction("UI内存", "输出图片内存", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Print()
        {
            Log.Message(BAUIImageCache.Describe());
        }

        //释放所有闲置图片，仍在显示的图片保持引用。
        [BADebugAction("UI内存", "释放闲置图片", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ClearIdle()
        {
            BAUIImageCache.Trim(true);
            Log.Message(BAUIImageCache.Describe());
        }
    }
}
