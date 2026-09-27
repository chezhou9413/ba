using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //妮露持续施法武器限制，负责阻止二段EX期间主武器继续发射普通子弹。
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class NeroChannelWeaponPatch
    {
        //覆盖前摇与整轮连射阶段，技能自身的独立弹丸不经过武器动词。
        public static bool Prefix(Verb_LaunchProjectile __instance, ref bool __result)
        {
            if (!__instance.CasterIsPawn || __instance.EquipmentSource == null ||
                __instance.EquipmentSource != __instance.CasterPawn.equipment?.Primary ||
                !(__instance.CasterPawn.jobs.curDriver is JobDriver_NeroExChannel)) return true;
            __result = false;
            return false;
        }
    }
}
