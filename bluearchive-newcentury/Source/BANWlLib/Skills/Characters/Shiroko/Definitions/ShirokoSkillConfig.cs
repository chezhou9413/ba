using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //白子的替换技能、无人机图形和保命状态配置。
    public class ShirokoSkillConfig
    {
        public AbilityDef disabledNormalAbility;
        public ShirokoNormalSkillDef normalSkill;
        public GraphicData droneGraphic;
        public HediffDef retirementBufferHediff;

        //检查白子专属资源与自动技能入口。
        public IEnumerable<string> ConfigErrors()
        {
            if (disabledNormalAbility == null) yield return "缺少disabledNormalAbility";
            if (normalSkill == null) yield return "缺少normalSkill";
            if (droneGraphic?.graphicClass != typeof(Graphic_Multi) || droneGraphic.texPath.NullOrEmpty())
                yield return "droneGraphic必须配置Graphic_Multi及四向贴图基础路径";
            if (retirementBufferHediff?.hediffClass != typeof(Hediff_ShirokoRetirementBuffer))
                yield return "retirementBufferHediff必须使用Hediff_ShirokoRetirementBuffer";
        }
    }
}
