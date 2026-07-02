using BANWlLib.DamageFontSystem.Comp;
using RimWorld;
using Verse;

namespace BANWlLib.DamageFontSystem
{
    // 伤害字体规则工具，负责统一读取旧暴击配置表并提供暴击和承伤系数判定。
    public static class DamageFontRuleUtility
    {
        // 判断指定伤害是否禁止暴击，负责兼容 FontDef.DisableCritical 配置。
        public static bool IsCriticalDisabled(DamageDef damageDef)
        {
            DisableCriticalComp comp = Current.Game?.GetComponent<DisableCriticalComp>();
            return ContainsDamageDef(comp?.DisableCritical, damageDef);
        }

        // 判断指定伤害是否强制暴击，负责兼容 FontDef.EnsureCritical 配置。
        public static bool IsCriticalEnsured(DamageDef damageDef)
        {
            DisableCriticalComp comp = Current.Game?.GetComponent<DisableCriticalComp>();
            return ContainsDamageDef(comp?.EnsureCritical, damageDef);
        }

        // 判断指定伤害是否跳过承伤系数，负责兼容 FontDef.DisableIncomingDamageFactorCritical 配置。
        public static bool ShouldSkipIncomingDamageFactor(DamageDef damageDef)
        {
            DisableCriticalComp comp = Current.Game?.GetComponent<DisableCriticalComp>();
            return ContainsDamageDef(comp?.DisableIncomingDamageFactorCritical, damageDef);
        }

        // 判断列表是否包含指定伤害，负责避免 Def 实例不一致时匹配失败。
        private static bool ContainsDamageDef(System.Collections.Generic.List<DamageDef> damageDefs, DamageDef damageDef)
        {
            if (damageDefs == null || damageDef == null)
            {
                return false;
            }

            for (int i = 0; i < damageDefs.Count; i++)
            {
                DamageDef candidate = damageDefs[i];
                if (candidate == damageDef || candidate?.defName == damageDef.defName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
