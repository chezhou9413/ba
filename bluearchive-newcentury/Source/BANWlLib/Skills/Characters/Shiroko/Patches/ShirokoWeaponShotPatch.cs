using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //白子技能期间锁定主武器，恢复普攻后按每发射击触发无人机追击。
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class ShirokoWeaponShotPatch
    {
        //保存追击目标，并在技能前摇和整轮连射期间阻止主武器出弹。
        public static bool Prefix(Verb_LaunchProjectile __instance, out Thing __state, ref bool __result)
        {
            __state = __instance.CurrentTarget.Thing;
            if (!__instance.CasterIsPawn || __instance.EquipmentSource == null ||
                __instance.EquipmentSource != __instance.CasterPawn.equipment?.Primary) return true;
            var state = Hediff_SpecialSkillState.Find(__instance.CasterPawn, SpecialSkillRole.Shiroko);
            if (state == null || (state.shiroko.normalJob == null && state.Now > state.shiroko.normalEndTick)) return true;
            //已投掷的连射独立完成，换工作或重新指定攻击也不能提前解除普攻锁定。
            __result = false;
            return false;
        }

        //每发主武器普攻安排一次独立追击，技能弹丸和无人机自身不会进入此入口。
        public static void Postfix(Verb_LaunchProjectile __instance, bool __result, Thing __state)
        {
            if (!__result || !__instance.CasterIsPawn || __instance.EquipmentSource == null ||
                __instance.EquipmentSource != __instance.CasterPawn.equipment?.Primary) return;
            var state = Hediff_SpecialSkillState.Find(__instance.CasterPawn, SpecialSkillRole.Shiroko);
            if (state != null) ShirokoSkills.ShotFired(state, __state);
        }
    }
}
