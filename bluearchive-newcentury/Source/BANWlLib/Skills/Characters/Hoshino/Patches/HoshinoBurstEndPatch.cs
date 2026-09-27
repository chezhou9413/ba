using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //星野连射收尾补丁，负责在连射结束或发射失败时清除本轮强化身份。
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    public static class HoshinoBurstEndPatch
    {
        //保留尚未成功开火的强化资格，仅清理已经结束的连射标记。
        public static void Postfix(Verb __instance)
        {
            if (__instance.Bursting) return;
            var s = HoshinoWeaponUtility.State(__instance);
            if (s != null) s.hoshino.empoweredBurst = false;
        }
    }
}
