using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //凯伊专属配置，负责指定真实场地来源和普通技能的前摇、语音与表现。
    public class KeiSkillConfig
    {
        public AbilityDef sourceExAbility;
        public HediffDef contributionHediff;
        public JobDef normalJob;
        public int warmupTicks = 60;
        public SoundDef warmupSound;
        public EffecterDef warmupEffecter;
        public SoundDef castSound;
        public EffecterDef casterEffecter;
        public EffecterDef targetEffecter;

        //读取来源EX的场地组件，保证测试EX共用同一份范围和时长配置。
        public CompProperties_AbilitySpawnBattleField FieldProperties => sourceExAbility.comps
            .OfType<CompProperties_AbilitySpawnBattleField>().Single();

        //检查场地来源和自动施法参数，配置错误直接交给日志报告。
        public IEnumerable<string> ConfigErrors()
        {
            if (sourceExAbility?.comps?.OfType<CompProperties_AbilitySpawnBattleField>().Count() != 1)
                yield return "凯伊sourceExAbility必须包含一个生成战斗场地的组件";
            else if (FieldProperties.fieldThingDef?.GetModExtension<BattleFieldControllerExtension>() == null)
                yield return "凯伊来源EX缺少有效的战斗场地配置";
            if (normalJob?.driverClass != typeof(JobDriver_KeiNormalSkill))
                yield return "凯伊normalJob必须使用JobDriver_KeiNormalSkill";
            if (warmupTicks <= 0) yield return "凯伊普通技能warmupTicks必须大于零";
        }
    }
}
