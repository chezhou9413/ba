using HarmonyLib;
using RimWorld;

namespace BANWlLib.Skills
{
    //角色在地图上获得本体技能时，同步安装技能运行状态。
    [HarmonyPatch(typeof(Pawn_AbilityTracker), nameof(Pawn_AbilityTracker.GainAbility))]
    public static class SpecialSkillAbilityGainPatch
    {
        //未入图的角色交给SpawnSetup处理，避免生成过程中提前改动形态与装备。
        public static void Postfix(Pawn_AbilityTracker __instance, AbilityDef def)
        {
            if (!__instance.pawn.Spawned) return;
            var profile = __instance.GetAbility(def).CompOfType<CompAbilityEffect_SpecialSkill>()?.Props.profile;
            if (profile != null) Hediff_SpecialSkillState.InstallNative(__instance.pawn, profile);
        }
    }
}
