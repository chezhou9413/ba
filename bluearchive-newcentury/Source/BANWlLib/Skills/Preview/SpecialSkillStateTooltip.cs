using System.Linq;
using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Skills
{
    //为没有主动按钮的专属普通技能显示伤害预览，并说明蓄积伤害的口径。
    public static class SpecialSkillStateTooltip
    {
        //按角色显示普通技能预览，已经确定的蓄积金额直接显示而不再次随机结算。
        public static string Build(Hediff_SpecialSkillState state)
        {
            var profile = state.profile;
            if (profile == null || !profile.showNormalBattleFormula) return "";
            switch (profile.role)
            {
                case SpecialSkillRole.Nero:
                    return "\n对决：双方标记有效时，对带有对决标记的敌人造成伤害×" +
                        profile.exMultiplier.ToString("0.##") + "（普攻与技能均生效；不提高目标的EX倍率）。";
                case SpecialSkillRole.Arisu:
                    return "\n普通技能（每" + profile.arisu.normalSkill.requiredHits + "次有效普攻命中）：" +
                        Formula(state.pawn, profile.arisu.normalSkill.attack);
                case SpecialSkillRole.Hoshino:
                    return state.stage == 0 ? "\n攻击普通技能（每" + profile.hoshino.normalRequiredShots + "发普攻）：" +
                        Formula(state.pawn, profile.normalAttack) : "";
                case SpecialSkillRole.Shiroko:
                    var normal = profile.shiroko.normalSkill;
                    return "\n\n普攻追击（每发主武器触发）\n" +
                        AbilityBattleTooltipUtility.BuildDamageSummary(state.pawn,
                            SpecialSkillBattlePreview.Expand(new[] { profile.droneAttack })) +
                        "\n\n普通技能·手榴弹（半径" + normal.grenadeAttack.radius.ToString("0.##") + "格）\n" +
                        AbilityBattleTooltipUtility.BuildDamageSummary(state.pawn,
                            SpecialSkillBattlePreview.Expand(new[] { normal.grenadeAttack })) +
                        "\n\n普通技能·无人机\n" +
                        AbilityBattleTooltipUtility.BuildDamageSummary(state.pawn,
                            SpecialSkillBattlePreview.Expand(normal.droneShots.Select(shot => shot.attack))) +
                        "\n\n预估未计暴击、克制与目标减伤。";
                case SpecialSkillRole.Kei:
                    return "\n普通技能攻击力伤害：" + Formula(state.pawn, profile.normalAttack) +
                        "\n普通技能记录伤害：" +
                        (UnityEngine.Mathf.Min(state.recorded, state.recordCap) * profile.burstAttack.attackPowerRatio).ToString("0.##") +
                        "（记录量×" + profile.burstAttack.attackPowerRatio.ToString("0.##") +
                        "，独立结算，命中后仍处理护甲、闪避与护盾）";
                case SpecialSkillRole.Wakamo:
                    return "\n蓄积爆发采用目标标记中锁定的换算倍率和暴击结果，到期按标记显示金额释放。";
                default: return "";
            }
        }

        //复用按钮的公式格式化工具，专属普通技能仍使用自己的实际攻击配置。
        private static string Formula(Pawn pawn, SpecialAttackConfig action)
        {
            return AbilityBattleTooltipUtility.BuildActionsTooltip(pawn, null,
                SpecialSkillBattlePreview.Expand(new[] { action }));
        }
    }
}
