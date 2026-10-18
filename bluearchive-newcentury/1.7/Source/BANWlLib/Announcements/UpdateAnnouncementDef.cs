using Verse;

namespace BANWlLib.Announcements;

//沿用公告的标题、正文和版本键，不加载旧 Unity 公告程序集。
public sealed class UpdateAnnouncementDef : Def
{
    public string key;
}
