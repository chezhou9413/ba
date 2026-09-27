using HarmonyLib;
using RimWorld;

namespace BANWlLib.Skills
{
    //复制能力可见性补丁，负责隐藏仍在维护持续效果的已消费实例。
    [HarmonyPatch(typeof(Ability), nameof(Ability.GizmosVisible))]
    public static class RioCopyVisibilityPatch
    {
        //保留原有阶段显隐规则，仅隐藏消费完毕的一次性能力。
        public static void Postfix(Ability __instance, ref bool __result)
        {
            if (RioSkills.Link(__instance)?.spent == true) __result = false;
        }
    }
}
