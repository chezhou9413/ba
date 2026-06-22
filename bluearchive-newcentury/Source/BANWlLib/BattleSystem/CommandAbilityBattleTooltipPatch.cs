using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 技能按钮悬浮补丁，负责把BA战斗公式追加到地图 Ability 按钮 tooltip。
    [HarmonyPatch(typeof(Command_Ability), "get_Tooltip")]
    public static class CommandAbilityBattleTooltipPatch
    {
        // 追加公式文本，负责保留原版说明并只对配置了扩展的技能生效。
        public static void Postfix(Command_Ability __instance, ref string __result)
        {
            if (__instance?.Ability == null)
            {
                return;
            }

            string extraTooltip = AbilityBattleTooltipUtility.BuildTooltip(__instance.Ability);
            if (extraTooltip.NullOrEmpty())
            {
                return;
            }

            __result += extraTooltip;
        }
    }
}
