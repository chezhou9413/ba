using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.pache
{
    // 远程攻击加成补丁，负责把 RemoteAttackBoost 状态伤害加成整合到主程序集。
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "DamageInfosToApply")]
    public static class RemoteAttackBoostPatch
    {
        private const float BoostFactor = 1.5f;

        // 调整近战攻击伤害结果，负责在攻击者拥有 RemoteAttackBoost 状态时提高伤害数值。
        public static IEnumerable<DamageInfo> Postfix(IEnumerable<DamageInfo> __result, Verb_MeleeAttackDamage __instance)
        {
            HediffDef boostDef = DefDatabase<HediffDef>.GetNamed("RemoteAttackBoost", false);
            if (boostDef == null)
            {
                Log.Error("[BANW] 未找到 RemoteAttackBoost HediffDef，远程攻击加成补丁无法生效。");
                foreach (DamageInfo damageInfo in __result)
                {
                    yield return damageInfo;
                }

                yield break;
            }

            Pawn caster = __instance.CasterPawn;
            bool shouldBoost = caster?.health?.hediffSet?.HasHediff(boostDef) == true;
            foreach (DamageInfo damageInfo in __result)
            {
                DamageInfo adjustedDamageInfo = damageInfo;
                if (shouldBoost)
                {
                    adjustedDamageInfo.SetAmount(adjustedDamageInfo.Amount * BoostFactor);
                }

                yield return adjustedDamageInfo;
            }
        }
    }
}
