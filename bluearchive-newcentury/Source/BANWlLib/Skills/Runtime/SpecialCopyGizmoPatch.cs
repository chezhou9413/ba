using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //复制技能按钮补丁，负责为目标本体显示独立的一次性EX按钮。
    [HarmonyPatch(typeof(Ability), nameof(Ability.GetGizmos))]
    public static class SpecialCopyGizmoPatch
    {
        //提供带EX选择菜单的复制命令，消费后不再显示按钮。
        public static bool Prefix(Ability __instance, ref IEnumerable<Command> __result)
        {
            var link = RioSkills.Link(__instance);
            if (link == null) return true;
            __result = link.spent || (__instance.pawn.Drafted && !__instance.def.showWhenDrafted)
                ? new Command[0] : new Command[] { link.Command };
            return false;
        }
    }
}
