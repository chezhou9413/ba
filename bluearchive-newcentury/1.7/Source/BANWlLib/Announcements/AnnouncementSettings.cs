using System.Collections.Generic;
using Verse;

namespace BANWlLib.Announcements;

//记录已展示的公告版本。
public sealed class AnnouncementSettings : ModSettings
{
    public Dictionary<string, string> history = new();

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref history, "announcementHistory", LookMode.Value, LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && history == null) history = new();
    }
}
