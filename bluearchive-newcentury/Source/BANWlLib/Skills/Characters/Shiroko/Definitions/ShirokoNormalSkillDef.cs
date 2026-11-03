using System.Collections.Generic;
using Verse;

namespace BANWlLib.Skills
{
    //只在EX期间自动发动的手榴弹与无人机逐发连射。
    public class ShirokoNormalSkillDef : Def
    {
        public float range = 25f;
        public int cooldownTicks = 1800;
        public int warmupTicks = 30;
        public JobDef jobDef;
        public SoundDef warmupSound;
        public EffecterDef warmupEffecter;
        public SpecialAttackConfig grenadeAttack;
        public List<ShirokoDroneShot> droneShots = new List<ShirokoDroneShot>();

        //逐发配置不能再包含多发分摊或隐藏的时间偏移。
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors()) yield return error;
            if (range <= 0f || cooldownTicks <= 0 || warmupTicks < 0)
                yield return defName + " 的射程、冷却或前摇无效";
            if (jobDef?.driverClass != typeof(JobDriver_ShirokoNormalSkill))
                yield return defName + " 必须配置JobDriver_ShirokoNormalSkill";
            foreach (string error in AttackErrors(grenadeAttack)) yield return defName + " 手榴弹：" + error;
            if (grenadeAttack?.projectileDef?.thingClass != typeof(Projectile_ShirokoGrenade) || grenadeAttack.radius <= 0f)
                yield return defName + " 的手榴弹必须使用Projectile_ShirokoGrenade并配置爆炸半径";
            if (droneShots.NullOrEmpty()) { yield return defName + " 缺少droneShots"; yield break; }
            foreach (ShirokoDroneShot shot in droneShots)
            {
                if (shot == null) { yield return defName + " 存在空连射配置"; continue; }
                if (shot.delayTicks < 0) yield return defName + " 的delayTicks必须非负";
                foreach (string error in AttackErrors(shot.attack)) yield return defName + " 无人机：" + error;
            }
        }

        //每项是一发独立技能伤害，沿用统一伤害和弹丸配置。
        private static IEnumerable<string> AttackErrors(SpecialAttackConfig attack)
        {
            if (attack == null) { yield return "缺少attack"; yield break; }
            if (attack.shots != 1 || attack.shotTicks != null)
                yield return "每项必须为单发，时间由delayTicks配置";
            if (attack.damageDef == null || attack.basePower < 0f || attack.attackPowerRatio < 0f ||
                attack.penetration < 0f || attack.radius < 0f || attack.isHealing || attack.isShield)
                yield return "伤害配置无效";
            if (attack.isNormalAttack || attack.isExSkill)
                yield return "普通技能不使用武器普攻倍率或EX倍率";
            if (attack.projectileDef == null || !typeof(Projectile_SpecialSkill).IsAssignableFrom(attack.projectileDef.thingClass))
                yield return "必须配置Projectile_SpecialSkill实体弹丸";
        }
    }
}
