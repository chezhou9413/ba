using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 武器特殊显示属性补丁，负责替换原版 ThingDef.SpecialDisplayStats 直接生成的远程伤害字符串行。
    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats))]
    public static class WeaponDamageSpecialDisplayStatsPatch
    {
        private static readonly FieldInfo OverrideReportTextField = AccessTools.Field(typeof(StatDrawEntry), "overrideReportText");

        // 包装原版统计行枚举，负责把武器信息卡左侧“伤害”行替换为 BA 修正后的数值。
        public static void Postfix(ThingDef __instance, StatRequest req, ref IEnumerable<StatDrawEntry> __result)
        {
            if (__instance == null || __result == null || !__instance.IsRangedWeapon)
            {
                return;
            }

            if (!req.HasThing || req.Thing == null)
            {
                return;
            }

            if (!WeaponDamageDisplayUtility.TryGetFinalWeaponDamage(req.Thing, out float finalDamage))
            {
                return;
            }

            __result = ReplaceDamageEntry(__result, finalDamage);
        }

        // 替换远程武器伤害行，负责命中原版字符串构造函数生成的 Damage 行。
        private static IEnumerable<StatDrawEntry> ReplaceDamageEntry(IEnumerable<StatDrawEntry> entries, float finalDamage)
        {
            foreach (StatDrawEntry entry in entries)
            {
                if (IsRangedDamageEntry(entry))
                {
                    yield return new StatDrawEntry(
                        entry.category,
                        entry.LabelCap,
                        finalDamage.ToString("0.#", CultureInfo.InvariantCulture),
                        GetOriginalReportText(entry),
                        entry.DisplayPriorityWithinCategory);
                    continue;
                }

                yield return entry;
            }
        }

        // 读取原版说明文本，负责避免调用 GetExplanationText 触发武器修正说明重复追加。
        private static string GetOriginalReportText(StatDrawEntry entry)
        {
            return OverrideReportTextField?.GetValue(entry) as string ?? string.Empty;
        }

        // 判断统计行是否是原版远程武器伤害行，负责避开护甲穿透、射程和近战伤害。
        private static bool IsRangedDamageEntry(StatDrawEntry entry)
        {
            if (entry == null || entry.category != StatCategoryDefOf.Weapon_Ranged)
            {
                return false;
            }

            string label = entry.LabelCap;
            if (label.NullOrEmpty())
            {
                return false;
            }

            string lowerLabel = label.ToLowerInvariant();
            return label == "伤害" || lowerLabel == "damage";
        }
    }
}
