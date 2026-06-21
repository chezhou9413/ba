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


    // 投射物护穿补丁，负责让技能弹安全读取基础护穿并叠加角色护穿属性。
    [HarmonyPatch(typeof(Projectile), "get_ArmorPenetration")]
    public static class Projectile_ArmorPenetration_Patch
    {
        private static readonly FieldInfo EquipmentDefField = AccessTools.Field(typeof(Projectile), "equipmentDef");
        private static readonly FieldInfo ArmorPenetrationBaseField = AccessTools.Field(typeof(ProjectileProperties), "armorPenetrationBase");

        // 读取投射物基础护穿，负责让技能弹没有武器来源时也能安全取得 Def 配置值。
        public static bool Prefix(Projectile __instance, ref float __result)
        {
            if (__instance == null)
            {
                return true;
            }

            ThingDef equipmentDef = EquipmentDefField?.GetValue(__instance) as ThingDef;
            if (equipmentDef != null)
            {
                return true;
            }

            __result = Mathf.Max(0f, GetProjectileArmorPenetrationBase(__instance.def?.projectile));
            return false;
        }

        // 调整最终投射物护穿，负责叠加发射者身上的远程护穿属性。
        public static void Postfix(Projectile __instance, ref float __result)
        {
            ApplyPawnPenetrationBonus(__instance, ref __result);
        }

        // 应用角色护穿加成，负责从发射者和装备上汇总 BANW_RangedWeapon_Penetration。
        private static void ApplyPawnPenetrationBonus(Projectile __instance, ref float __result)
        {
            Thing launcher = __instance.Launcher;
            if (launcher is Pawn pawn && __instance != null)
            {
                StatDef penetrationStat = StatDef.Named("BANW_RangedWeapon_Penetration");
                if (penetrationStat == null)
                {
                    return;
                }

                float penetrationBonus = pawn.GetStatValue(penetrationStat);

                Pawn_ApparelTracker apparelTracker = pawn.apparel;
                if (apparelTracker != null)
                {
                    StatDef apparelPenetrationStat = penetrationStat;
                    foreach (var apparel in apparelTracker.WornApparel)
                    {
                        penetrationBonus += apparel.GetStatValue(apparelPenetrationStat);
                    }
                }
                __result += penetrationBonus;
            }
        }

        // 读取投射物 Def 基础护穿，负责绕过原版需要 weapon 参数的计算路径。
        private static float GetProjectileArmorPenetrationBase(ProjectileProperties projectileProperties)
        {
            if (projectileProperties == null || ArmorPenetrationBaseField == null)
            {
                return 0f;
            }

            return ArmorPenetrationBaseField.GetValue(projectileProperties) is float value ? value : 0f;
        }
    }
}
