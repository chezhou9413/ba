using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace BANWlLib.KindStats
{
    // Kind 属性 Harmony 补丁，负责把自定义 StatDef 和 PawnKind 扩展接入原版系统。
    public static class BANWKindStatPatches
    {
        // 通用 Verb 射程补丁，负责让技能和非发射类远程 Verb 获得最终射程加值。
        [HarmonyPatch(typeof(Verb), "get_EffectiveRange")]
        public static class Patch_Verb_EffectiveRange
        {
            // 后置处理射程，负责跳过发射类 Verb 以避免和专用补丁重复计算。
            public static void Postfix(Verb __instance, ref float __result)
            {
                if (__instance is Verb_LaunchProjectile)
                {
                    return;
                }

                BANWKindStatUtility.ApplyRangeOffset(__instance, ref __result);
            }
        }

        // 发射类 Verb 射程补丁，负责在原版武器倍率之后追加固定格数。
        [HarmonyPatch(typeof(Verb_LaunchProjectile), "get_EffectiveRange")]
        public static class Patch_VerbLaunchProjectile_EffectiveRange
        {
            // 后置处理射程，负责保证 +10 表示最终射程增加 10 格。
            public static void Postfix(Verb_LaunchProjectile __instance, ref float __result)
            {
                BANWKindStatUtility.ApplyRangeOffset(__instance, ref __result);
            }
        }

        // Pawn 生命值补丁，负责把 BA 生命值公式接入原版 HealthScale。
        [HarmonyPatch(typeof(Pawn), "get_HealthScale")]
        public static class Patch_Pawn_HealthScale
        {
            // 后置处理生命值倍率，负责执行基础固定生命值、升级倍率、升星倍率、固定加算和最终生命值加成。
            public static void Postfix(Pawn __instance, ref float __result)
            {
                __result = BANWKindStatUtility.GetHealthScale(__instance, __result);
            }
        }

        // 世界地图货物承载补丁，负责只影响商队和世界地图转移界面的容量计算。
        [HarmonyPatch(typeof(CollectionsMassCalculator), nameof(CollectionsMassCalculator.Capacity), typeof(List<ThingCount>), typeof(StringBuilder))]
        public static class Patch_CollectionsMassCalculator_Capacity
        {
            // 后置处理货物容量，负责把 PawnKind 配置的千克加值加到世界地图容量。
            public static void Postfix(List<ThingCount> thingCounts, StringBuilder explanation, ref float __result)
            {
                if (thingCounts == null)
                {
                    return;
                }

                float totalOffset = 0f;
                for (int i = 0; i < thingCounts.Count; i++)
                {
                    ThingCount thingCount = thingCounts[i];
                    if (thingCount.Count <= 0 || !(thingCount.Thing is Pawn pawn))
                    {
                        continue;
                    }

                    float offset = BANWKindStatUtility.GetWorldCargoCapacityOffset(pawn) * thingCount.Count;
                    if (Mathf.Approximately(offset, 0f))
                    {
                        continue;
                    }

                    totalOffset += offset;
                    AppendCargoExplanation(explanation, pawn, offset);
                }

                if (!Mathf.Approximately(totalOffset, 0f))
                {
                    __result = Mathf.Max(0f, __result + totalOffset);
                }
            }

            // 写入货物容量说明，负责让世界地图界面显示 Kind 配置来源。
            private static void AppendCargoExplanation(StringBuilder explanation, Pawn pawn, float offset)
            {
                if (explanation == null)
                {
                    return;
                }

                if (explanation.Length > 0)
                {
                    explanation.AppendLine();
                }

                explanation.Append("  - " + pawn.LabelShortCap + " Kind货物能力: " + offset.ToStringMassOffset());
            }
        }
    }
}
