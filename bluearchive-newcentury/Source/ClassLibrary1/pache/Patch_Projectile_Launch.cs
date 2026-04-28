using BANWlLib.Tool;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using static RimWorld.PsychicRitualRoleDef;
using static UnityEngine.Networking.UnityWebRequest;

namespace BANWlLib
{

    [HarmonyPatch(typeof(StatDrawEntry))]
    [HarmonyPatch(nameof(StatDrawEntry.GetExplanationText))]
    public static class Final_Detector_Patch
    {
        public static float setchuantou(Pawn pawn)
        {
            float a = 0f;
            if (pawn != null) {
                // 1. 获取自定义的穿透属性
                StatDef penetrationStat = StatDef.Named("BANW_RangedWeapon_Penetration");
                if (penetrationStat == null)
                {
                    // 如果StatDef不存在，提前退出，避免错误
                    return 0f;
                }

                // 2. 以 Pawn 的该项属性作为基础（可能来自特性、能力等）
                float penetrationBonus = pawn.GetStatValue(penetrationStat);

                // 2.1 再叠加装备（已穿戴服装）在该属性上的数值
                // 正确获取穿戴追踪器应为 pawn.apparel，而不是通过 ParentHolder
                Pawn_ApparelTracker apparelTracker = pawn.apparel;
                if (apparelTracker != null)
                {
                    StatDef apparelPenetrationStat = penetrationStat; // 缓存，避免循环内重复 Named 查找
                    foreach (var apparel in apparelTracker.WornApparel)
                    {
                        penetrationBonus += apparel.GetStatValue(apparelPenetrationStat);
                    }
                }
                a += penetrationBonus;
            }
            return a;
        }
        private static Pawn GetHolderPawn(Thing thing)
        {
            if (thing == null) return null;

            // 装备栏持有
            if (thing.ParentHolder is Pawn_EquipmentTracker eq)
            {
                return eq.pawn;
            }

            // 衣物栏持有
            if (thing.ParentHolder is Pawn_ApparelTracker ap)
            {
                return ap.pawn;
            }

            // 物品在容器里（比如背包），递归查找
            if (thing.ParentHolder is ThingOwner owner)
            {
                foreach (var t in owner)
                {
                    if (t is Pawn p) return p;
                }
            }

            return null;
        }

        public static void Prefix(StatDrawEntry __instance, StatRequest optionalReq)
        {
            if (__instance == null) return;

            Pawn pawn = null;
            if (optionalReq.HasThing && optionalReq.Thing != null)
            {
                pawn = GetHolderPawn(optionalReq.Thing);
            }
            if (__instance.LabelCap.Contains("护甲穿透"))
            {
                var field = AccessTools.Field(typeof(StatDrawEntry), "labelInt");
                if (field != null)
                {
                    string newLabel = pawn != null
                        ? "护甲穿透：" + "+" + setchuantou(pawn) * 100 + "%"
                        : "护甲穿透";
                    field.SetValue(__instance, newLabel);
                }      
            }
        }

        public static void Postfix(StatDrawEntry __instance, StatRequest optionalReq, ref string __result)
        {
            // 举例：针对护甲穿透，修改说明文字
            if (__instance.LabelCap.Contains("护甲穿透"))
            {
                Pawn pawn = null;
                if (optionalReq.HasThing && optionalReq.Thing != null)
                {
                    pawn = GetHolderPawn(optionalReq.Thing); // 你之前写的获取 Pawn 的方法
                }

                if (pawn != null)
                {
                    __result = $"基础穿甲数值："+ __instance.ValueString+"\n基于角色本身的加成：" + setchuantou(pawn) * 100 + "%";
                }
            }
        }
    }


    // 针对 Projectile 类的 ArmorPenetration 属性进行补丁
    [HarmonyPatch(typeof(Projectile), "get_ArmorPenetration")]
    public static class Projectile_ArmorPenetration_Patch
    {
        // 我们依然使用 Postfix 来修改最终结果
        public static void Postfix(Projectile __instance, ref float __result)
        {
            // 1. 从 Projectile 实例中获取 launcher 字段。
            //    通常会有一个公共属性 Launcher 来访问这个受保护的字段。
            Thing launcher = __instance.Launcher;
            // 确保发射者是一个Pawn，并且投掷物实例存在
            if (launcher is Pawn pawn && __instance != null)
            {
                // 1. 获取自定义的穿透属性
                StatDef penetrationStat = StatDef.Named("BANW_RangedWeapon_Penetration");
                if (penetrationStat == null)
                {
                    // 如果StatDef不存在，提前退出，避免错误
                    return;
                }

                // 2. 以 Pawn 的该项属性作为基础（可能来自特性、能力等）
                float penetrationBonus = pawn.GetStatValue(penetrationStat);

                // 2.1 再叠加装备（已穿戴服装）在该属性上的数值
                // 正确获取穿戴追踪器应为 pawn.apparel，而不是通过 ParentHolder
                Pawn_ApparelTracker apparelTracker = pawn.apparel;
                if (apparelTracker != null)
                {
                    StatDef apparelPenetrationStat = penetrationStat; // 缓存，避免循环内重复 Named 查找
                    foreach (var apparel in apparelTracker.WornApparel)
                    {
                        penetrationBonus += apparel.GetStatValue(apparelPenetrationStat);
                    }
                }
                __result += penetrationBonus;
            }
        }
    }
}
