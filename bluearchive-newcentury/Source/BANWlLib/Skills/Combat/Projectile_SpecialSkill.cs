using Verse;
using BANWlLib.Projectiles;

namespace BANWlLib.Skills
{
    //特殊技能实体弹丸，保存独立数值上下文并在实际命中时调用统一结算。
    public class Projectile_SpecialSkill : Projectile
    {
        public SpecialPendingAttack attack;
        public override int UpdateRateTicks => 1;

        //无人机和无伤害支援弹丸直接处理预定目标，治疗倒地队友不套用攻击偏弹。
        protected override void ImpactSomething()
        {
            if (attack != null && (attack.droneOrigin || attack.action.damageDef == null) &&
                attack.target != null && CanHit(attack.target))
                Impact(attack.target);
            else base.ImpactSomething();
        }

        //弹丸结束飞行时释放技能状态占用，包含拦截、越界和正常命中。
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            attack?.Complete();
            base.Destroy(mode);
        }

        //命中后执行单体或范围伤害，被拦截时仅销毁。
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (!blockedByShield && attack != null)
            {
                attack.impactDirection = DirectionalImpactEffectUtility.GetTravelDirection(this);
                SpecialCombatUtility.Impact(attack, hitThing, Position, Map);
            }
            base.Impact(hitThing, blockedByShield);
        }

        //保存飞行中伤害、快照和来源，不依赖临时静态缓存。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref attack, "specialAttack");
        }
    }
}
