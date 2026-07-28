using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.WeaponRestrictions
{
    // 原版装备判定补丁，负责把 Kind 武器白名单接入右键装备和 AI 自动拾取入口。
    [HarmonyPatch]
    public static class EquipmentUtilityCanEquipPatch
    {
        // 定位包含 out 拒绝原因参数的原版装备判定重载。
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(EquipmentUtility),
                nameof(EquipmentUtility.CanEquip),
                new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) });
        }

        // 在原版判定通过后检查 Kind 白名单，并向浮动菜单返回明确的拒绝原因。
        public static void Postfix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            if (!__result || WeaponWhitelistUtility.CanEquip(pawn, thing))
            {
                return;
            }

            __result = false;
            cantReason = "BANW_WeaponNotInKindWhitelist".Translate();
        }
    }
}
