using System.Collections.Generic;

namespace BANWlLib.Skills
{
    //若藻分段连射配置，每段独立声明伤害类型、倍率和弹丸。
    public class WakamoSkillConfig
    {
        public List<WakamoAttackStage> exStages;

        //检查各段时间和单体弹丸配置，整轮至少发射两发。
        public IEnumerable<string> ConfigErrors()
        {
            if (exStages == null || exStages.Count == 0)
            {
                yield return "若藻缺少EX伤害段";
                yield break;
            }
            int totalShots = 0;
            for (int i = 0; i < exStages.Count; i++)
            {
                WakamoAttackStage stage = exStages[i];
                string label = "若藻EX第" + (i + 1) + "段：";
                if (stage?.attack == null)
                {
                    yield return label + "缺少attack配置";
                    continue;
                }
                SpecialAttackConfig attack = stage.attack;
                if (stage.delayTicks < 0) yield return label + "delayTicks不能为负数";
                foreach (string error in attack.TimingErrors()) yield return label + error;
                if (attack.damageDef == null || attack.shots <= 0 || attack.shotIntervalTicks < 0)
                    yield return label + "缺少伤害类型或连射参数无效";
                if (attack.projectileDef == null ||
                    !typeof(Projectile_SpecialSkill).IsAssignableFrom(attack.projectileDef.thingClass) || attack.radius != 0f)
                    yield return label + "必须配置使用Projectile_SpecialSkill的单体弹丸，radius保持0";
                if (attack.basePower < 0 || attack.attackPowerRatio < 0)
                    yield return label + "伤害数值不能为负数";
                totalShots += attack.shots;
            }
            if (totalShots < 2) yield return "若藻EX各段合计必须至少发射两发";
        }
    }
}
