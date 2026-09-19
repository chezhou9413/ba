using System.Linq;
using BANWlLib.KindStats;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //成功施法补丁，负责只在能力确实发动后消费复制机会。
    [HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
    public static class SpecialSkillActivatedPatch
    {
        //发布成功施法通知，失败和取消均不消耗复制机会。
        public static void Postfix(Ability __instance, bool __result)
        {
            if (__result) SpecialSkillEvents.Activated(__instance);
        }
    }
}
