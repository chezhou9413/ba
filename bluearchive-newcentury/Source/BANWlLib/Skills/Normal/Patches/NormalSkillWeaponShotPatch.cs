using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //射击补丁职责：只把主武器成功发射的每发普攻发布给通用普通技能。
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class NormalSkillWeaponShotPatch
    {
        //通知职责：排除失败射击、非主武器和技能动词，保留星野强化射击的成功发射事件。
        public static void Postfix(Verb_LaunchProjectile __instance, bool __result)
        {
            if (!__result || !__instance.CasterIsPawn || __instance.EquipmentSource == null ||
                __instance.EquipmentSource != __instance.CasterPawn.equipment?.Primary) return;
            NormalSkillUtility.Notify(__instance.CasterPawn, NormalSkillCountMode.ShotFired);
        }
    }
}
