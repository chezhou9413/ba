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

        // 获取生命值倍率，负责通过缓存降低 Pawn.HealthScale 高频访问开销。
        public static float GetHealthScale(Pawn pawn, float originalHealthScale)
        {
            return HealthScaleCache.GetOrCalculate(pawn, originalHealthScale);
        }

        // 计算生命值倍率，负责执行 BA 新生命值公式并返回给 Pawn.HealthScale。
        public static float CalculateHealthScaleUncached(Pawn pawn, float originalHealthScale)
        {
            if (pawn == null)
            {
                return originalHealthScale;
            }

            if (BattleStatUtility.GetBaseStatExtension(pawn) == null && BattleStatUtility.GetStarGrowthExtension(pawn) == null)
            {
                return originalHealthScale;
            }

            float baseHealth = GetBaseHealth(pawn);
            float levelMultiplier = GetHealthLevelMultiplier(pawn);
            float starMultiplier = GetHealthStarMultiplier(pawn);
            float flatBonus = GetHealthFlatBonus(pawn);
            float bonusMultiplier = GetHealthBonusMultiplier(pawn);
            float finalHealth = (baseHealth * levelMultiplier * starMultiplier + flatBonus) * bonusMultiplier;
            return Mathf.Max(0.01f, finalHealth);
        }

        // 获取初始生命值，负责从 PawnKind 和战斗 Stat 读取生命值公式的基础乘算项。
        private static float GetBaseHealth(Pawn pawn)
        {
            return Mathf.Max(0f, BattleStatUtility.GetInitialHealth(pawn) + GetPawnStatValue(pawn, BattleStatDefOf.BANW_InitialHealth));
        }

        // 获取升级生命值倍率，负责把等级、装备和状态提供的升级加成转换为乘区。
        private static float GetHealthLevelMultiplier(Pawn pawn)
        {
            float bonus = GetPawnStatValue(pawn, BattleStatDefOf.BANW_HealthLevelMultiplier);
            return Mathf.Max(0f, 1f + bonus);
        }

        // 获取升星生命值倍率，负责读取当前阶级对应的生命值成长。
        private static float GetHealthStarMultiplier(Pawn pawn)
        {
            float bonus = BattleStatUtility.GetRankHealthPercent(pawn);
            return Mathf.Max(0f, 1f + bonus);
        }

        // 获取固定生命值加算，负责把装备、状态和星级固定成长加入乘算后的生命值。
        private static float GetHealthFlatBonus(Pawn pawn)
        {
            float bonus = GetPawnStatValue(pawn, BattleStatDefOf.BANW_HealthFlatBonus);
            return Mathf.Max(0f, bonus);
        }

        // 获取生命值加成倍率，负责把最终百分比生命值加成转换为乘区。
        private static float GetHealthBonusMultiplier(Pawn pawn)
        {
            float bonus = GetPawnStatValue(pawn, BattleStatDefOf.BANW_HealthBonusMultiplier);
            return Mathf.Max(0f, 1f + bonus);
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
