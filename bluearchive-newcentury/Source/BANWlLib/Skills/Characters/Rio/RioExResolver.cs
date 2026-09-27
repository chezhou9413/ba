using System.Collections.Generic;
using System.Linq;
using BANWlLib.CostSystem;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //EX解析器，负责按COST组件识别目标当前阶段可见的原生技能。
    public static class RioExResolver
    {
        //收集所有带COST配置的可见原生技能，不以当前费用、冷却或角色白名单过滤。
        public static List<Ability> Candidates(Pawn pawn)
        {
            if (pawn?.abilities == null) return new List<Ability>();
            return pawn.abilities.AllAbilitiesForReading
                .Where(a => AbilityCostCalculator.FindCostComp(a) != null && !RioSkills.IsCopied(a) && a.GizmosVisible())
                .GroupBy(a => a.def).Select(g => g.First()).ToList();
        }

        //优先选择当前角色专属技能的可见EX，其余带COST的技能仍可从复制菜单选择。
        public static AbilityDef Default(Pawn pawn)
        {
            var profile = pawn?.kindDef.GetModExtension<SpecialSkillKindExtension>()?.profile;
            return Candidates(pawn).OrderByDescending(a => profile != null &&
                a.CompOfType<CompAbilityEffect_SpecialSkill>()?.Props.profile == profile).FirstOrDefault()?.def;
        }

        //在相同专属技能配置中查找当前阶段，普通EX则保持原定义。
        public static AbilityDef CurrentPhase(Pawn pawn, AbilityDef source)
        {
            var profile = source.comps?.OfType<CompProperties_SpecialSkill>().FirstOrDefault()?.profile;
            var choices = Candidates(pawn);
            if (profile == null) return choices.FirstOrDefault(a => a.def == source)?.def;
            return choices.FirstOrDefault(a => a.CompOfType<CompAbilityEffect_SpecialSkill>()?.Props.profile == profile)?.def;
        }
    }
}
