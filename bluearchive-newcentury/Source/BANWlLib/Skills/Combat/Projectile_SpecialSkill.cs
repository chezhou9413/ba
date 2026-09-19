using Verse;

namespace BANWlLib.Skills
{
    //特殊技能实体弹丸，负责保存独立数值上下文并在实际命中时调用统一结算。
    public class Projectile_SpecialSkill : Projectile
    {
        public SpecialPendingAttack attack;
        public override int UpdateRateTicks => 1;

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
                SpecialCombatUtility.Impact(attack, hitThing, Position, Map);
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
