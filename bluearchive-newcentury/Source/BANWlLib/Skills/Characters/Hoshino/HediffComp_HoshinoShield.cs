using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Skills
{
    //星野个人护盾与前方拦截共用一个余额，破盾的一次伤害仍完整抵挡。
    public class HediffComp_HoshinoShield : HediffComp_BattleShield
    {
        private int absorbedDamageId = -1;

        //同一次伤害拆分到多个部位时，破盾后仍挡住该次伤害的其余部分。
        public bool AbsorbResolved(float amount)
        {
            int id = SpecialSkillEvents.CurrentDamage?.id ?? -1;
            bool absorbed = TryAbsorbDamage(amount);
            if (absorbed && id >= 0) absorbedDamageId = id;
            return absorbed || id >= 0 && absorbedDamageId == id;
        }

        //在减伤结算后消耗当前护盾，友军伤害沿用项目原有的穿盾规则。
        public static bool TryAbsorb(Pawn pawn, DamageInfo damage, float amount)
        {
            if (amount <= 0f || !HoshinoInterceptor.CanBlockDamage(pawn, damage) || damage.Instigator == pawn ||
                damage.Instigator?.Faction != null && damage.Instigator.Faction == pawn.Faction) return false;
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                var shield = hediff.TryGetComp<HediffComp_HoshinoShield>();
                if (shield != null && shield.AbsorbResolved(amount)) return true;
            }
            return false;
        }
    }
}
