using BANWlLib.BattleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.KindStats
{
    // Kind 属性计算工具，负责集中处理射程、生命值和世界地图载重的数值规则。
    public static class BANWKindStatUtility
    {
        // 获取 PawnKind 扩展，负责屏蔽空 Pawn 或空 Kind 的判断。
        public static BANWKindStatExtension GetKindExtension(Pawn pawn)
        {
            return pawn?.kindDef?.GetModExtension<BANWKindStatExtension>();
        }

        // 获取非近战射程加值，负责让状态、服装和武器通过原版 StatDef 系统提供射程。
        public static float GetRangeOffset(Pawn pawn)
        {
            if (pawn == null || BANWStatDefOf.BANW_RangedWeapon_RangeOffset == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, pawn.GetStatValue(BANWStatDefOf.BANW_RangedWeapon_RangeOffset));
        }

        // 追加非近战射程加值，负责避免近战和无效施放者被错误增强。
        public static void ApplyRangeOffset(Verb verb, ref float range)
        {
            if (verb == null || range <= 0f || verb.IsMeleeAttack || !(verb.Caster is Pawn pawn))
            {
                return;
            }

            float offset = GetRangeOffset(pawn);
            if (offset <= 0f)
            {
                return;
            }

            range = Mathf.Max(0f, range + offset);
            range = ApplyWeatherCap(verb, pawn, range);
        }

        // 应用天气最大射程限制，负责保留原版恶劣天气对远程攻击的限制。
        private static float ApplyWeatherCap(Verb verb, Pawn pawn, float range)
        {
            if (verb.EquipmentSource != null && verb.EquipmentSource.TryGetComp<CompUniqueWeapon>(out CompUniqueWeapon comp) && comp.IgnoreAccuracyMaluses)
            {
                return range;
            }

            Map map = pawn.MapHeld;
            if (map != null && map.weatherManager.CurWeatherMaxRangeCap >= 0f)
            {
                return Mathf.Min(range, map.weatherManager.CurWeatherMaxRangeCap);
            }

            return range;
        }

        // 获取生命值尺度，负责通过缓存降低 Pawn.HealthScale 高频访问开销。
        public static float GetHealthScale(Pawn pawn, float originalHealthScale)
        {
            return HealthScaleCache.GetOrCalculate(pawn, originalHealthScale);
        }

        // 计算生命值尺度，负责执行 Kind 覆盖、平加和百分比加成的固定公式。
        public static float CalculateHealthScaleUncached(Pawn pawn, float originalHealthScale)
        {
            if (pawn == null)
            {
                return originalHealthScale;
            }

            float baseScale = originalHealthScale;
            BANWKindStatExtension extension = GetKindExtension(pawn);
            if (extension != null && extension.healthScaleOverride.HasValue)
            {
                float lifeStageFactor = pawn.ageTracker?.CurLifeStage?.healthScaleFactor ?? 1f;
                baseScale = extension.healthScaleOverride.Value * lifeStageFactor;
            }

            float offset = GetPawnStatValue(pawn, BANWStatDefOf.BANW_HealthScaleOffset) + GetWornApparelStatValue(pawn, BANWStatDefOf.BANW_HealthScaleOffset);
            float percentOffset = GetPawnStatValue(pawn, BANWStatDefOf.BANW_HealthScalePercentOffset) + GetWornApparelStatValue(pawn, BANWStatDefOf.BANW_HealthScalePercentOffset);
            percentOffset += BattleStatUtility.GetBaseHealthPercent(pawn);
            return Mathf.Max(0.01f, (baseScale + offset) * Mathf.Max(0f, 1f + percentOffset));
        }

        // 获取小人属性值，负责在 StatDef 未加载或 Pawn 无效时返回安全默认值。
        private static float GetPawnStatValue(Pawn pawn, StatDef statDef)
        {
            if (pawn == null || statDef == null)
            {
                return 0f;
            }

            return pawn.GetStatValue(statDef);
        }

        //获取已穿戴装备上的自定义属性，负责让装备生命值加成进入所有身体部位的生命倍率。
        private static float GetWornApparelStatValue(Pawn pawn, StatDef statDef)
        {
            if (pawn?.apparel?.WornApparel == null || statDef == null)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
            {
                Apparel apparel = pawn.apparel.WornApparel[i];
                if (apparel == null)
                {
                    continue;
                }

                total += apparel.GetStatValue(statDef);
            }

            return total;
        }

        // 获取世界地图货物承载加值，负责按 PawnKind 配置返回千克数。
        public static float GetWorldCargoCapacityOffset(Pawn pawn)
        {
            BANWKindStatExtension extension = GetKindExtension(pawn);
            if (extension == null)
            {
                return 0f;
            }

            return extension.worldCargoCapacityOffset;
        }
    }
}
