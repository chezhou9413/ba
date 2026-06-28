using System.Globalization;
using System.Text;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 星级成长显示工具，负责生成当前星级带来的总成长说明并清理旧的独立显示状态。
    public static class StarGrowthDisplayUtility
    {
        // 清理旧版星级成长显示状态，负责让成长信息只显示在学生星级 Hediff 里面。
        public static void EnsureDisplayHediff(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null || HediffDefOf.BANW_StarGrowthDisplayStatus == null)
            {
                return;
            }

            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BANW_StarGrowthDisplayStatus);
            if (existing != null)
            {
                pawn.health.RemoveHediff(existing);
            }
        }

        // 判断是否需要显示星级成长状态，负责避免没有成长配置的 Pawn 出现空状态。
        public static bool ShouldDisplay(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            return BattleStatUtility.GetRankHealthFlat(pawn) != 0f ||
                   BattleStatUtility.GetRankHealthPercent(pawn) != 0f ||
                   BattleStatUtility.GetRankAttackFlat(pawn) != 0f ||
                   BattleStatUtility.GetRankAttackPercent(pawn) != 0f ||
                   BattleStatUtility.GetRankHealFlat(pawn) != 0f ||
                   BattleStatUtility.GetRankHealPercent(pawn) != 0f;
        }

        // 构建健康状态悬浮说明，负责向玩家展示当前星级已经生效的总成长加成。
        public static string BuildTooltip(Pawn pawn)
        {
            if (pawn == null || !ShouldDisplay(pawn))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("星级成长总加成：");
            AppendNonZeroLine(builder, "固定生命值", BattleStatUtility.GetRankHealthFlat(pawn));
            AppendNonZeroPercentLine(builder, "生命值倍率", BattleStatUtility.GetRankHealthPercent(pawn));
            AppendNonZeroLine(builder, "固定攻击力", BattleStatUtility.GetRankAttackFlat(pawn));
            AppendNonZeroPercentLine(builder, "攻击力倍率", BattleStatUtility.GetRankAttackPercent(pawn));
            AppendNonZeroLine(builder, "固定治愈力", BattleStatUtility.GetRankHealFlat(pawn));
            AppendNonZeroPercentLine(builder, "治愈力倍率", BattleStatUtility.GetRankHealPercent(pawn));
            return builder.ToString().TrimEnd();
        }

        // 追加非零固定加成行，负责让悬浮文本只显示真实生效的总固定值。
        private static void AppendNonZeroLine(StringBuilder builder, string label, float value)
        {
            if (value == 0f)
            {
                return;
            }

            builder.AppendLine("• " + label + "：" + FormatSignedNumber(value));
        }

        // 追加非零百分比加成行，负责让悬浮文本只显示真实生效的总百分比。
        private static void AppendNonZeroPercentLine(StringBuilder builder, string label, float value)
        {
            if (value == 0f)
            {
                return;
            }

            builder.AppendLine("• " + label + "：" + FormatSignedPercent(value));
        }

        // 格式化带符号普通数值，负责让正负固定加成显示清晰。
        private static string FormatSignedNumber(float value)
        {
            return value.ToString("+#,0.#;-#,0.#;0", CultureInfo.InvariantCulture);
        }

        // 格式化带符号百分比数值，负责把 0.2 显示为 +20%。
        private static string FormatSignedPercent(float value)
        {
            return value.ToString("+#,0%;-#,0%;0%", CultureInfo.InvariantCulture);
        }
    }
}
