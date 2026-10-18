using BANWlLib.BattleSystem;
using BANWlLib.Projectiles;
using Verse;

namespace BANWlLib.Skills
{
    //将被拦截弹丸的一次本体伤害送入星野的护盾结算，不引发爆炸或追加命中效果。
    public static class HoshinoInterceptDamage
    {
        //保留技能快照和普通武器公式，避免把技能弹的占位伤害当成真实扣盾量。
        public static void Apply(Projectile projectile, Pawn target)
        {
            if (projectile is Projectile_SpecialSkill special && special.attack != null)
            {
                SpecialCombatUtility.Damage(special.attack, target);
                return;
            }
            if (projectile is Projectile_PiercingArea piercing)
            {
                BattleStatUtility.ApplyAction(projectile.Launcher, target, piercing.BuildBattleAction());
                return;
            }
            ProjectileBattleContext.RegisterImpactTarget(projectile, target);
            try
            {
                //DamageAmount的现有补丁武器倍率、暴击与克制，并登记防重复计算的上下文。
                var damage = new DamageInfo(projectile.DamageDef, projectile.DamageAmount, projectile.ArmorPenetration,
                    projectile.ExactRotation.eulerAngles.y, projectile.Launcher, null, projectile.EquipmentDef,
                    DamageInfo.SourceCategory.ThingOrUnknown, target);
                target.TakeDamage(damage);
            }
            finally { ProjectileBattleContext.Clear(projectile); }
        }
    }
}
