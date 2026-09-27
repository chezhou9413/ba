using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //星野形态连射补丁，负责按角色形态提供发数而不修改共享武器定义或缓存。
    [HarmonyPatch(typeof(Verb), nameof(Verb.BurstShotCount), MethodType.Getter)]
    public static class HoshinoBurstCountPatch
    {
        //攻击形态返回攻击连射数，防御形态返回防御连射数。
        public static void Postfix(Verb __instance, ref int __result)
        {
            var s = HoshinoWeaponUtility.State(__instance);
            if (s != null) __result = s.stage == 0 ? s.profile.hoshino.outputBurstShots : s.profile.hoshino.tankBurstShots;
        }
    }
}
