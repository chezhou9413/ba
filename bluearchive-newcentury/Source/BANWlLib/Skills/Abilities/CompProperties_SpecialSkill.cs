using RimWorld;

namespace BANWlLib.Skills
{
    //特殊技能按钮配置，负责把AbilityDef绑定到角色配置与具体操作。
    public class CompProperties_SpecialSkill : CompProperties_AbilityEffect
    {
        public SpecialSkillProfileDef profile;
        public SpecialSkillCommand command;

        //绑定对应的技能执行组件。
        public CompProperties_SpecialSkill() { compClass = typeof(CompAbilityEffect_SpecialSkill); }
    }
}

