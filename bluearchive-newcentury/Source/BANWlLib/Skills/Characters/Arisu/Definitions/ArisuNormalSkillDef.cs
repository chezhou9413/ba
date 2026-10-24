using System.Collections.Generic;
using Verse;

namespace BANWlLib.Skills
{
    //爱丽丝自动普通技能定义，独立保存命中阈值、伤害、声音和表现，不提供主动按钮。
    public class ArisuNormalSkillDef : Def
    {
        public int requiredHits = 3;
        public int cooldownTicks = 1;
        public int warmupTicks = 60;
        public JobDef jobDef;
        public float range = 25f;
        public SoundDef castSound;
        public EffecterDef casterEffecter;
        public EffecterDef targetEffecter;
        public SpecialAttackConfig attack;

        //检查自动触发、前摇和伤害配置；是否应用EX乘区由攻击配置决定。
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors()) yield return error;
            if (requiredHits <= 0 || cooldownTicks < 1 || range <= 0 || warmupTicks < 0)
                yield return defName + " 的触发次数、冷却、前摇或范围无效";
            if (jobDef?.driverClass != typeof(JobDriver_ArisuNormalSkill))
                yield return defName + " 必须配置JobDriver_ArisuNormalSkill施法工作";
            if (attack == null || attack.damageDef == null || attack.shots <= 0 || attack.shotIntervalTicks < 0 ||
                attack.basePower < 0 || attack.attackPowerRatio < 0 || attack.radius < 0 || attack.penetration < 0)
                yield return defName + " 的普通技能伤害配置无效";
            if (attack?.projectileDef != null && !typeof(Projectile_SpecialSkill).IsAssignableFrom(attack.projectileDef.thingClass))
                yield return defName + " 的弹丸必须使用Projectile_SpecialSkill";
        }
    }
}
