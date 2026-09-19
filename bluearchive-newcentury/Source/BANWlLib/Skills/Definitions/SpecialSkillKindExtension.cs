using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //角色技能绑定，负责指定正式角色的特殊技能配置和可供复制的默认EX。
    public class SpecialSkillKindExtension : DefModExtension
    {
        public SpecialSkillProfileDef profile;
        public AbilityDef copyableEx;
    }
}
