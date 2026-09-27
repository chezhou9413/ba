using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //复制技能按钮，负责显示一次性用途并允许选择目标本体的其他COST技能。
    public class Command_RioCopiedAbility : Command_Ability
    {
        //沿用原版技能瞄准、费用与前摇工作，实际施法者始终是目标本体。
        public Command_RioCopiedAbility(Ability ability, Pawn pawn) : base(ability, pawn)
        {
            groupable = false;
            hotKey = null;
        }

        public override string Label => "复制·" + ability.def.LabelCap;
        public override string Tooltip => base.Tooltip + "\n\n由" + Pawn.LabelShort +
            "本体使用，沿用当前阶段、充能、层数与属性。仅可成功使用一次，基础COST减1。右键选择其他EX。";

        //独立复制机会不能与原有EX或其他复制机会合并按钮。
        public override bool GroupsWith(Gizmo other) => false;

        //列出当前阶段可见的全部COST技能，不限制为七个测试角色。
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                var link = RioSkills.Link(ability);
                if (link == null || link.spent || ability.Casting) yield break;
                foreach (Ability source in RioExResolver.Candidates(Pawn))
                {
                    AbilityDef selected = source.def;
                    yield return new FloatMenuOption(selected.LabelCap, () =>
                    {
                        if (!link.spent && !link.copy.Casting) link.SetAbility(selected);
                    });
                }
            }
        }
    }
}
