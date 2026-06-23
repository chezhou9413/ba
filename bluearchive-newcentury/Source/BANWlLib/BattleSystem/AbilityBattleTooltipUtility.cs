using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 技能战斗悬浮工具，负责把配置化战斗参数格式化为地图技能按钮的说明文本。
    public static class AbilityBattleTooltipUtility
    {
        private static readonly Color DamageColor = new Color(1f, 0.35f, 0.28f);
        private static readonly Color HealColor = new Color(0.35f, 1f, 0.45f);
        private static readonly Color ExColor = new Color(1f, 0.82f, 0.22f);
        private static readonly Color CritColor = new Color(1f, 0.58f, 0.18f);
        private static readonly Color AffinityColor = new Color(0.35f, 0.72f, 1f);
        private static readonly Color DisabledColor = ColorLibrary.Grey;

        // 构建技能公式悬浮文本，负责根据当前 Pawn 的实时属性给出预估数值。
        public static string BuildTooltip(Ability ability)
        {
            if (ability?.def == null || ability.pawn == null)
            {
                return string.Empty;
            }

            AbilityBattleTooltipExtension extension = ability.def.GetModExtension<AbilityBattleTooltipExtension>();
            if (extension == null || !extension.showBattleFormula || extension.previewActions.NullOrEmpty())
            {
                return string.Empty;
            }

            Pawn pawn = ability.pawn;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("BA战斗公式".Colorize(ColoredText.TipSectionTitleColor));
            AppendCasterStats(builder, pawn);

            List<BattleActionConfig> actions = extension.previewActions;
            for (int i = 0; i < actions.Count;)
            {
                BattleActionConfig action = actions[i];
                if (action == null)
                {
                    i++;
                    continue;
                }

                int repeatCount = CountSameActions(actions, i, action);
                builder.AppendLine();
                builder.AppendLine(FormatSegmentTitle(i, repeatCount).Colorize(ColoredText.TipSectionTitleColor));
                if (action.isHealing)
                {
                    AppendHealAction(builder, pawn, action);
                }
                else
                {
                    AppendDamageAction(builder, pawn, action);
                }

                i += repeatCount;
            }

            return builder.ToString();
        }

        //统计连续相同战斗段，负责让多段相同伤害在悬浮说明里合并显示。
        private static int CountSameActions(List<BattleActionConfig> actions, int startIndex, BattleActionConfig action)
        {
            int count = 1;
            for (int i = startIndex + 1; i < actions.Count; i++)
            {
                if (!IsSamePreviewAction(action, actions[i]))
                {
                    break;
                }

                count++;
            }

            return count;
        }

        //比较两个预览战斗段，负责判断它们是否可以在显示上合并。
        private static bool IsSamePreviewAction(BattleActionConfig left, BattleActionConfig right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            return left.baseAmount == right.baseAmount &&
                   left.attackPowerRatio == right.attackPowerRatio &&
                   left.healPowerRatio == right.healPowerRatio &&
                   left.damageDef == right.damageDef &&
                   left.penetration == right.penetration &&
                   left.isHealing == right.isHealing &&
                   left.canCrit == right.canCrit &&
                   left.applyAffinity == right.applyAffinity &&
                   left.isExSkill == right.isExSkill;
        }

        //格式化段落标题，负责把连续相同段压缩成短标题。
        private static string FormatSegmentTitle(int startIndex, int repeatCount)
        {
            if (repeatCount <= 1)
            {
                return "第" + (startIndex + 1) + "段";
            }

            return "第" + (startIndex + 1) + "-" + (startIndex + repeatCount) + "段（共" + repeatCount + "次）";
        }

        // 写入施法者属性，负责让玩家看到公式里的实时基础值。
        private static void AppendCasterStats(StringBuilder builder, Pawn pawn)
        {
            float attackFlat = BattleStatUtility.GetAttackFlatBonus(pawn);
            float attackPowerBase = BattleStatUtility.GetAttackPowerBaseMultiplier(pawn);
            float attackMultiplier = BattleStatUtility.GetAttackMultiplier(pawn);
            float finalAttack = BattleStatUtility.GetFinalAttackPower(pawn);
            float healFlat = BattleStatUtility.GetHealFlatBonus(pawn);
            float healMultiplier = BattleStatUtility.GetHealMultiplier(pawn);
            float finalHeal = BattleStatUtility.GetFinalHealPower(pawn);
            float exMultiplier = BattleStatUtility.GetExSkillMultiplier(pawn);

            builder.AppendLine("攻击力：" + FormatNumber(attackFlat) + " x " + FormatPercent(attackPowerBase) + " x " + FormatPercent(attackMultiplier) + " = " + FormatColor(FormatNumber(finalAttack), DamageColor));
            builder.AppendLine("治疗力：" + FormatNumber(healFlat) + " x " + FormatPercent(healMultiplier) + " = " + FormatColor(FormatNumber(finalHeal), HealColor));
            builder.AppendLine("EX技能倍率：" + FormatColor(FormatPercent(exMultiplier), ExColor));
        }

        // 写入伤害段公式，负责展示固定值、攻击力倍率、暴击、克制和 EX 倍率。
        private static void AppendDamageAction(StringBuilder builder, Pawn pawn, BattleActionConfig action)
        {
            BattleDamageResult result = BattleStatUtility.BuildDamageResult(new BattleDamageRequest
            {
                instigator = pawn,
                target = pawn,
                damageDef = action.damageDef,
                baseAmount = action.baseAmount,
                attackPowerRatio = action.attackPowerRatio,
                penetration = action.penetration,
                canCrit = false,
                applyAffinity = false,
                isExSkill = action.isExSkill
            });

            builder.AppendLine("类型：" + FormatColor("伤害", DamageColor));
            builder.AppendLine("基础值：" + FormatNumber(action.baseAmount));
            builder.AppendLine("攻击倍率：" + FormatColor(FormatPercent(action.attackPowerRatio), DamageColor));
            builder.AppendLine("暴击：" + FormatSwitch(action.canCrit, CritColor));
            builder.AppendLine("属性克制：" + FormatSwitch(action.applyAffinity, AffinityColor));
            builder.AppendLine("EX倍率：" + FormatEx(action.isExSkill, result.exSkillMultiplier));
            AppendDamageFormula(builder, action);
            builder.AppendLine("预估伤害：" + FormatColor(FormatNumber(result.finalAmount), DamageColor));
            builder.AppendLine(FormatFormulaHint(action.applyAffinity));
        }

        // 写入治疗段公式，负责展示固定值、治疗力倍率、暴击、受疗和 EX 倍率。
        private static void AppendHealAction(StringBuilder builder, Pawn pawn, BattleActionConfig action)
        {
            BattleHealResult result = BattleStatUtility.BuildHealResult(new BattleHealRequest
            {
                instigator = pawn,
                target = pawn,
                baseAmount = action.baseAmount,
                healPowerRatio = action.healPowerRatio,
                canCrit = false,
                isExSkill = action.isExSkill
            });

            builder.AppendLine("类型：" + FormatColor("治疗", HealColor));
            builder.AppendLine("基础值：" + FormatNumber(action.baseAmount));
            builder.AppendLine("治疗倍率：" + FormatColor(FormatPercent(action.healPowerRatio), HealColor));
            builder.AppendLine("暴击：" + FormatSwitch(action.canCrit, CritColor));
            builder.AppendLine("受回复倍率：" + FormatColor(FormatPercent(BattleStatUtility.GetHealReceivedMultiplier(pawn)), HealColor));
            builder.AppendLine("EX倍率：" + FormatEx(action.isExSkill, result.exSkillMultiplier));
            AppendHealFormula(builder, action);
            builder.AppendLine("预估治疗：" + FormatColor(FormatNumber(result.finalAmount), HealColor));
        }

        //写入伤害算法，负责用短行展示实际结算顺序，避免 tooltip 横向撑开。
        private static void AppendDamageFormula(StringBuilder builder, BattleActionConfig action)
        {
            builder.AppendLine("算法：");
            builder.AppendLine(FormatColor("  固定：基础值 x 基础攻倍 x 攻击加成", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  攻击：最终攻击力 x 技能倍率", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  合计：固定 + 攻击", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  修正：" + FormatFormulaModifiers(action.canCrit, action.applyAffinity, action.isExSkill), ColoredText.SubtleGrayColor));
        }

        //写入治疗算法，负责用短行展示实际结算顺序，避免 tooltip 横向撑开。
        private static void AppendHealFormula(StringBuilder builder, BattleActionConfig action)
        {
            builder.AppendLine("算法：");
            builder.AppendLine(FormatColor("  基础：(基础值 + 治疗力) x 治疗加成", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  技能：最终治疗力 x 技能倍率", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  合计：基础 + 技能，再乘受回复", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  修正：" + FormatFormulaModifiers(action.canCrit, false, action.isExSkill), ColoredText.SubtleGrayColor));
        }

        //格式化公式修正项，负责把暴击、克制和 EX 这些附加步骤压缩成一行。
        private static string FormatFormulaModifiers(bool canCrit, bool applyAffinity, bool isExSkill)
        {
            List<string> modifiers = new List<string>();
            if (canCrit)
            {
                modifiers.Add("暴击");
            }

            if (applyAffinity)
            {
                modifiers.Add("克制");
            }

            if (isExSkill)
            {
                modifiers.Add("EX");
            }

            return modifiers.Count > 0 ? string.Join("、", modifiers.ToArray()) : "无";
        }

        // 格式化属性克制说明，负责避免在没有目标时误报具体克制倍率。
        private static string FormatFormulaHint(bool applyAffinity)
        {
            if (!applyAffinity)
            {
                return FormatColor("属性克制未参与。", DisabledColor);
            }

            return FormatColor("命中目标后按目标护甲类型结算克制。", AffinityColor);
        }

        // 格式化开关文本，负责把启用和禁用状态染成不同颜色。
        private static string FormatSwitch(bool enabled, Color enabledColor)
        {
            return enabled ? FormatColor("参与", enabledColor) : FormatColor("不参与", DisabledColor);
        }

        // 格式化 EX 倍率文本，负责只在 EX 技能段高亮真实倍率。
        private static string FormatEx(bool isExSkill, float multiplier)
        {
            return isExSkill ? FormatColor(FormatPercent(multiplier), ExColor) : FormatColor("不参与", DisabledColor);
        }

        // 格式化数值，负责让 tooltip 中的计算结果保持短小稳定。
        private static string FormatNumber(float value)
        {
            return value.ToString("0.#");
        }

        // 格式化倍率，负责把 1.2 显示为 120%。
        private static string FormatPercent(float value)
        {
            return value.ToString("P0");
        }

        // 格式化彩色文本，负责统一 RimWorld 富文本颜色写法。
        private static string FormatColor(string text, Color color)
        {
            return text.Colorize(color);
        }
    }
}
