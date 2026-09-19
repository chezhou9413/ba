using System.Linq;
using BANWlLib.KindStats;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //复制技能按钮标记，负责在原有标题上清楚注明一次性用途。
    [HarmonyPatch(typeof(Ability), nameof(Ability.GetGizmos))]
    public static class SpecialCopyGizmoPatch
    {
        //为独立复制能力添加测试标签，不改变共享AbilityDef。
        public static void Postfix(Ability __instance, ref System.Collections.Generic.IEnumerable<Command> __result)
        {
            if (!RioSkills.IsCopied(__instance)) return;
            var commands = __result.ToList();
            foreach (Command command in commands)
                command.defaultLabel = "测试新技能·复制：" + __instance.def.label;
            __result = commands;
        }
    }
}
