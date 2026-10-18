using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Announcements;

//沿用原公告的主菜单展示时机，由模组现有补丁入口注册。
[HarmonyPatch(typeof(VersionUpdateDialogMaker), nameof(VersionUpdateDialogMaker.CreateVersionUpdateDialogIfNecessary))]
internal static class AnnouncementPatch
{
    //只为未读版本创建窗口，避免每次返回主菜单重复弹出。
    private static void Postfix()
    {
        var mod = LoadedModManager.GetMod<newpro>();
        var settings = mod.GetSettings<AnnouncementSettings>();
        bool changed = false;
        foreach (var announcement in DefDatabase<UpdateAnnouncementDef>.AllDefsListForReading)
        {
            if (settings.history.TryGetValue(announcement.defName, out var key) && key == announcement.key) continue;
            Find.WindowStack.Add(new Dialog_MessageBox(announcement.LabelCap + "\n\n" + announcement.description));
            settings.history[announcement.defName] = announcement.key;
            changed = true;
        }
        if (changed) mod.WriteSettings();
    }
}
