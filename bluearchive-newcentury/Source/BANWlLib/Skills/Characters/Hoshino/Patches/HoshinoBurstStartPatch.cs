using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //星野连射起始补丁，仅在一轮射击开始时领取强化资格。
    [HarmonyPatch(typeof(Verb), nameof(Verb.WarmupComplete))]
    public static class HoshinoBurstStartPatch
    {
        //冻结整轮的强化身份，途中取得的强化资格留给下一轮。
        public static void Prefix(Verb __instance, ref LocalTargetInfo ___currentTarget)
        {
            var s = HoshinoWeaponUtility.State(__instance);
            if (s == null) return;
            s.hoshino.empoweredBurst = s.stage == 0 && s.castEndTick < s.Now && s.hoshino.empoweredReady;
            if (!s.hoshino.empoweredBurst) return;
            s.hoshino.empoweredTarget = ___currentTarget.Thing;
            s.hoshino.empoweredAimCell = ___currentTarget.Cell;
            //连射动词持有格子，角色目标死亡不会使剩余射击失效。
            ___currentTarget = new LocalTargetInfo(s.hoshino.empoweredAimCell);
        }
    }
}
