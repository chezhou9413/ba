using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //星野专属配置，负责分段EX、普攻触发、强化连射和防御普通技能参数。
    public class HoshinoSkillConfig
    {
        public List<HoshinoAttackStage> exStages;
        public int normalRequiredShots = 30;
        public int outputBurstShots = 6;
        public int tankBurstShots = 1;
        public AbilityDef tankNormalAbility;
        public SpecialAttackConfig empoweredAttack;
        public EffecterDef empoweredShotEffecter;
        public float effectSpeed = 20f;
        public float effectOffsetForward = 0.6f;
        public float effectOffsetUp;

        //检查专属必填字段，阻止连射、分段或特效配置错误被静默忽略。
        public IEnumerable<string> ConfigErrors()
        {
            if (normalRequiredShots <= 0 || outputBurstShots <= 0 || tankBurstShots <= 0)
                yield return "普攻触发次数和形态连射数必须大于零";
            if (tankNormalAbility == null) yield return "缺少防御普通技能按钮";
            if (empoweredShotEffecter == null) yield return "缺少强化普攻定向特效";
            if (empoweredAttack == null || empoweredAttack.damageDef == null || empoweredAttack.radius <= 0 ||
                empoweredAttack.shots != 1 || empoweredAttack.projectileDef != null || empoweredAttack.attackPowerRatio < 0)
                yield return "强化普攻必须配置有伤害类型的单次直接范围伤害，不能配置弹丸";
            if (exStages == null || exStages.Count == 0) { yield return "缺少EX伤害段"; yield break; }
            foreach (HoshinoAttackStage stage in exStages)
            {
                SpecialAttackConfig attack = stage.attack;
                if (stage.delayTicks < 0 || attack == null || attack.damageDef == null || attack.shots <= 0 ||
                    attack.shotIntervalTicks < 0 || attack.attackPowerRatio < 0 || attack.basePower < 0 || attack.radius < 0)
                    yield return "EX伤害段时间或伤害配置无效";
                if (stage.targetLocation && attack != null && attack.radius <= 0)
                    yield return "落点伤害段必须配置范围";
                if (attack?.projectileDef == null || !typeof(Projectile_SpecialSkill).IsAssignableFrom(attack.projectileDef.thingClass))
                    yield return "EX弹丸必须使用Projectile_SpecialSkill";
            }
        }
    }
}
