using System.Collections.Generic;
using Verse;

namespace BANWlLib.Skills
{
    //爱丽丝EX配置，负责直线尺寸、三档对敌特效和两档自身充能表现。
    public class ArisuSkillConfig
    {
        public float lineLength = 25f;
        public float lineWidth = 3f;
        public int damageDelayTicks;
        public List<EffecterDef> attackEffecters;
        public List<EffecterDef> chargeEffecters;
        public List<EffecterDef> chargeStateEffecters;
        public SoundDef attackSound;
        public SoundDef chargeSound;
        public ArisuNormalSkillDef normalSkill;

        //检查阶段列表数量和直线尺寸，配置错误直接报告。
        public IEnumerable<string> ConfigErrors()
        {
            if (lineLength <= 0 || lineWidth <= 0 || damageDelayTicks < 0)
                yield return "直线长度、宽度或伤害延迟无效";
            if (attackEffecters == null || attackEffecters.Count != 3 || attackEffecters.Contains(null))
                yield return "对敌特效必须依次配置0、1、2档对应的三个EffecterDef";
            if (chargeEffecters == null || chargeEffecters.Count != 2 || chargeEffecters.Contains(null))
                yield return "自身充能特效必须依次配置第一档、第二档两个EffecterDef";
            if (chargeStateEffecters != null && (chargeStateEffecters.Count != 2 || chargeStateEffecters.Contains(null)))
                yield return "持续充能特效若配置，必须提供两档EffecterDef";
            if (normalSkill == null) yield return "缺少独立普通技能normalSkill";
        }
    }
}
