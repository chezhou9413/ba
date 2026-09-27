using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //白子武器射击通知，负责在普攻发射时触发无人机，不依赖弹丸的命中结果。
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class ShirokoWeaponShotPatch
    {
        //保存本发瞄准对象，射偏落点或后续伤害事件不能改变无人机追击目标。
        public static void Prefix(Verb_LaunchProjectile __instance, out Thing __state)
        {
            __state = __instance.CurrentTarget.Thing;
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
