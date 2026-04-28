using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RemoteAttackBoostMod
{
    public class RemoteAttackBoostMod : Mod
    {
        public RemoteAttackBoostMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.yourname.remoteattackboost");
            harmony.PatchAll();
        }
    }

    [StaticConstructorOnStartup]
    public static class RemoteAttackBoostPatches
    {
        static RemoteAttackBoostPatches()
        {
            // 初始化补丁
        }

        // Patch 方法：修改远程伤害计算
        [HarmonyPatch(typeof(Verb_MeleeAttack), "CalculateDamageTo")]
        public static class Verb_MeleeAttack_CalculateDamageTo_Patch
        {
            public static void Postfix(ref DamageInfo? __result, Thing ___pawn)
            {
                if (__result.HasValue && ___pawn is Pawn pawn)
                {
                    // 检查是否有远程攻击加成状态
                    if (pawn.health.hediffSet.HasHediff(HediffDef.Named("RemoteAttackBoost")))
                    {
                        DamageInfo damageInfo = __result.Value;
                        float boostFactor = 1.5f; // 远程伤害加成系数
                        damageInfo.SetAmount(damageInfo.Amount * boostFactor);
                        __result = damageInfo;
                    }
                }
            }
        }
    }
}