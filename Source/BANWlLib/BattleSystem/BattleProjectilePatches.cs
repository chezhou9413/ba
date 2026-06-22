using HarmonyLib;
using BANWlLib.Projectiles;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    [HarmonyPatch(typeof(Projectile), "get_DamageAmount")]
    public static class BattleProjectileDamagePatch
    {
        public static void Postfix(Projectile __instance, ref int __result)
        {
            if (__instance is Projectile_PiercingArea || !(__instance?.Launcher is Pawn pawn))
            {
                return;
            }

            __result = Mathf.Max(0, Mathf.RoundToInt(BattleStatUtility.ScaleDamageBase(pawn, __result)));
        }
    }
}
