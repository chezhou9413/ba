using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //EX期间只禁用配置指定的原普通技能，保留能力实例及其冷却。
    public static class ShirokoAbilityBlock
    {
        //按钮、排队和实际扣费入口使用同一条件。
        public static bool CanUse(Ability ability, out string reason)
        {
            var state = Hediff_SpecialSkillState.Find(ability.pawn, SpecialSkillRole.Shiroko);
            bool blocked = state != null && state.Active && ability.def == state.profile.shiroko.disabledNormalAbility;
            reason = blocked ? "EX期间由无人机普通技能替代，EX结束后恢复" : null;
            return !blocked;
        }

        //原版排队和AI施法都会读取CanCast。
        [HarmonyPatch(typeof(Ability), "get_CanCast")]
        private static class CanCastPatch
        {
            //附加白子当前阶段的禁用原因。
            public static void Postfix(Ability __instance, ref AcceptanceReport __result)
            {
                if (!CanUse(__instance, out string reason)) __result = reason;
            }
        }

        //即使原技能仍在冷却，也明确显示EX替换原因。
        [HarmonyPatch(typeof(Ability), nameof(Ability.GizmoDisabled))]
        private static class GizmoPatch
        {
            //保持按钮可见并显示当前不能施放的原因。
            public static void Postfix(Ability __instance, ref bool __result, ref string reason)
            {
                if (!CanUse(__instance, out string blockedReason))
                {
                    __result = true;
                    reason = blockedReason;
                }
            }
        }
    }
}
