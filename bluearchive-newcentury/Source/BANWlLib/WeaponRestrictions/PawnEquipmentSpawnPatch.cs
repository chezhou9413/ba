using HarmonyLib;
using Verse;

namespace BANWlLib.WeaponRestrictions
{
    // Pawn 生成补丁，负责在离图学生重新进入地图时卸下旧存档中的违规武器。
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Notify_PawnSpawned))]
    public static class PawnEquipmentSpawnPatch
    {
        // Pawn 完成生成后检查其装备栏，使商队和运输中的学生返回地图时执行白名单。
        public static void Postfix(Pawn_EquipmentTracker __instance)
        {
            WeaponWhitelistUtility.DropDisallowedWeapons(__instance.pawn);
        }
    }
}
