using BANWlLib.BattleSystem;
using BANWlLib.DamageFontSystem;
using BANWlLib.DamageFontSystem.Comp;
using BANWlLib.DamageFontSystem.Setting;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

public class DamageFontSystemPatche
{
    public static Dictionary<int, bool> CritState = new Dictionary<int, bool>();

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_Pawn_PreApplyDamage
    {
        // 受伤前处理，负责给原版伤害补上暴击与属性克制，同时跳过已走统一战斗层的手动结算。
        public static void Prefix(Pawn __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (__instance == null || dinfo.Instigator == null)
            {
                return;
            }

            if (BattleDamageDisplayState.TryConsumeManualCritState(__instance, out bool manualCrit))
            {
                if (manualCrit)
                {
                    CritState[__instance.thingIDNumber] = true;
                }
                else if (CritState.ContainsKey(__instance.thingIDNumber))
                {
                    CritState.Remove(__instance.thingIDNumber);
                }
                return;
            }

            DisableCriticalComp comp = Current.Game.GetComponent<DisableCriticalComp>();
            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker == null)
            {
                return;
            }

            string damageType = dinfo.Def.defName;
            bool disableCrit = comp != null && comp.DisableCritical.Any(p => p.defName == damageType);
            bool isForcedCrit = comp != null && comp.EnsureCritical.Any(p => p.defName == damageType);
            float finalAmount = dinfo.Amount;
            bool isCrit = false;
            if (!disableCrit)
            {
                float critChance = attacker.GetStatValue(CriticalRef.BANW_CriticalChance);
                float critMultiplier = attacker.GetStatValue(CriticalRef.BANW_CriticalDamage);
                isCrit = isForcedCrit || Rand.Value < critChance;
                if (isCrit)
                {
                    finalAmount *= critMultiplier;
                    CritState[__instance.thingIDNumber] = true;
                }
            }

            float affinityMultiplier = BattleStatUtility.GetAffinityMultiplier(attacker, __instance);
            if (Mathf.Abs(affinityMultiplier - 1f) > 0.0001f)
            {
                finalAmount *= affinityMultiplier;
            }

            if (isCrit)
            {
                dinfo.SetAmount(finalAmount);
            }
            else if (CritState.ContainsKey(__instance.thingIDNumber))
            {
                CritState.Remove(__instance.thingIDNumber);
                dinfo.SetAmount(finalAmount);
            }
            else if (Mathf.Abs(finalAmount - dinfo.Amount) > 0.0001f)
            {
                dinfo.SetAmount(finalAmount);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
    public static class Patch_Pawn_PostApplyDamage
    {
        public static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            if (!DamageFontMod.settings.enableDamageFloat)
            {
                return;
            }

            bool isCrit = false;
            if (CritState.TryGetValue(__instance.thingIDNumber, out bool state))
            {
                isCrit = state;
                CritState.Remove(__instance.thingIDNumber);
            }

            if (!isCrit || totalDamageDealt <= 0.01f)
            {
                return;
            }

            CriticalObjPool.showCriticalShow(totalDamageDealt, __instance);
        }
    }
}
