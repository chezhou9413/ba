using HarmonyLib;
using Verse;
using Verse.AI;

namespace BANWlLib.WeaponRestrictions
{
    // 装备任务补丁，负责在任务开始前再次阻止不符合 Kind 白名单的武器。
    [HarmonyPatch(typeof(JobDriver_Equip), nameof(JobDriver_Equip.TryMakePreToilReservations))]
    public static class JobDriverEquipPatch
    {
        // 校验装备任务的目标武器，避免已排队任务绕过统一装备判定。
        public static bool Prefix(JobDriver_Equip __instance, ref bool __result)
        {
            Thing targetThing = __instance.job.GetTarget(TargetIndex.A).Thing;
            if (WeaponWhitelistUtility.CanEquip(__instance.pawn, targetThing))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}
