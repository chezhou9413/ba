using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //星野连射起始补丁，负责仅在一轮射击开始时领取强化资格。
    [HarmonyPatch(typeof(Verb), nameof(Verb.WarmupComplete))]
    public static class HoshinoBurstStartPatch
    {
        //冻结整轮的强化身份，途中取得的强化资格留给下一轮。
        public static void Prefix(Verb __instance)
        {
            var s = HoshinoWeaponUtility.State(__instance);
            if (s == null) return;
            s.hoshino.empoweredBurst = s.stage == 0 && s.castEndTick < s.Now && s.hoshino.empoweredReady;
        }
    }
}
