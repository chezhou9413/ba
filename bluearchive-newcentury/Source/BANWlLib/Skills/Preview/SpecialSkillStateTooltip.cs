using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Skills
{
    //状态说明职责：为没有主动按钮的专属普通技能显示真实配置公式，并说明蓄积伤害的口径。
    public static class SpecialSkillStateTooltip
    {
        //构建职责：按角色显示普通技能公式，已经确定的蓄积金额直接显示而不再次随机结算。
        public static string Build(Hediff_SpecialSkillState state)
        {
            var profile = state.profile;
            if (profile == null || !profile.showNormalBattleFormula) return "";
            switch (profile.role)
            {
                case SpecialSkillRole.Arisu:
                    return "\n普通技能（每" + profile.arisu.normalSkill.requiredHits + "次有效普攻命中）：" +
                        Formula(state.pawn, profile.arisu.normalSkill.attack);
                case SpecialSkillRole.Hoshino:
                    return state.stage == 0 ? "\n攻击普通技能（每" + profile.hoshino.normalRequiredShots + "发普攻）：" +
                        Formula(state.pawn, profile.normalAttack) : "";
                case SpecialSkillRole.Shiroko:
                    return "\n普通技能宿主攻击：" + Formula(state.pawn, profile.normalAttack) +
                        (state.Active ? "\n无人机连射：" + Formula(state.pawn, profile.burstAttack) : "");
                case SpecialSkillRole.Kei:
                    return "\n普通技能释放金额：" + UnityEngine.Mathf.Min(state.recorded, state.recordCap).ToString("0.##") +
                        "（按已蓄积金额释放，命中后仍处理护甲、闪避与护盾）";
                case SpecialSkillRole.Wakamo:
                    return "\n蓄积爆发采用目标标记中锁定的换算倍率和暴击结果，到期按标记显示金额释放。";
                default: return "";
            }
        }

        //公式职责：复用按钮的公式格式化工具，专属普通技能仍使用自己的实际攻击配置。
        private static string Formula(Pawn pawn, SpecialAttackConfig action)
        {
            return AbilityBattleTooltipUtility.BuildActionsTooltip(pawn, null,
                SpecialSkillBattlePreview.Expand(new[] { action }));
        }
    }
}
