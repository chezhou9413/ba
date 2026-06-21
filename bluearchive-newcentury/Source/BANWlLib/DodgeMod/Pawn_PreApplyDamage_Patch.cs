using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DodgeMod
{
    // 受伤前闪避补丁，负责在目标拥有闪避概率时拦截本次伤害。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Pawn_PreApplyDamage_Patch
    {
        // 处理受伤前判定，负责跳过带 IgnoreDodgeExtension 的伤害并在闪避成功时显示 MISS。
        public static bool Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            DamageDef damageDef = dinfo.Def;
            if (damageDef?.GetModExtension<IgnoreDodgeExtension>() != null)
            {
                return true;
            }

            float dodgeChance = DodgeCalculator.GetTotalDodgeChance(__instance);
            if (dodgeChance <= 0f || Rand.Value >= dodgeChance)
            {
                return true;
            }

            if (__instance.Map != null)
            {
                MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "MISS", Color.white, 3.9f);
            }

            dinfo.SetAmount(0f);
            return false;
        }
    }
}
