using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //特殊技能实例，负责在原版扣费入口之前完成最终阶段和目标校验。
    public class Ability_SpecialSkill : Ability
    {
        //支持存档创建独立技能实例。
        public Ability_SpecialSkill() { }
        //支持原版按角色创建能力。
        public Ability_SpecialSkill(Pawn pawn) : base(pawn) { }
        //支持原版按角色和定义创建能力。
        public Ability_SpecialSkill(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        //拒绝阶段过期、非法目标和未就绪施放，避免失败时消耗COST。
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            var effect = CompOfType<CompAbilityEffect_SpecialSkill>();
            if (effect.GizmoDisabled(out string reason) || !effect.Valid(target, true))
            {
                if (!reason.NullOrEmpty()) Messages.Message(reason, MessageTypeDefOf.RejectInput, false);
                return false;
            }
            return base.Activate(target, dest);
        }
    }
}

