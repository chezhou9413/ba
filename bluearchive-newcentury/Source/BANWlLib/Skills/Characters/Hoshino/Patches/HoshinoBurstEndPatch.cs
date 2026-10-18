using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //星野连射收尾补丁，在连射结束或发射失败时清除本轮强化身份。
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    public static class HoshinoBurstEndPatch
    {
        //更新本发的格子目标，保持原版连射与瞄准姿态不依赖目标存活。
        public static void Prefix(Verb __instance, ref LocalTargetInfo ___currentTarget)
        {
            var s = HoshinoWeaponUtility.State(__instance);
            if (s == null || !s.hoshino.empoweredBurst) return;
            Thing target = s.hoshino.empoweredTarget;
            if (target != null && target.Spawned && target.Map == s.pawn.Map && !(target is Pawn pawn && pawn.Dead))
                s.hoshino.empoweredAimCell = target.Position;
            ___currentTarget = new LocalTargetInfo(s.hoshino.empoweredAimCell);
        }

        //保留尚未成功开火的强化资格，仅清理已经结束的连射标记。
        public static void Postfix(Verb __instance)
        {
            if (__instance.Bursting) return;
            var s = HoshinoWeaponUtility.State(__instance);
            if (s == null) return;
            s.hoshino.empoweredBurst = false;
            s.hoshino.empoweredTarget = null;
        }
    }
}
