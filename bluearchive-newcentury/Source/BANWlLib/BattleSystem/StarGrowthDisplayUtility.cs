using System.Globalization;
using System.Text;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 阶级成长显示工具，负责把当前阶级带来的成长数值同步为可查看的健康状态说明。
    public static class StarGrowthDisplayUtility
    {
        // 确保 Pawn 身上存在或移除阶级成长显示状态，负责让健康页只显示有效成长来源。
        public static void EnsureDisplayHediff(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null || HediffDefOf.BANW_StarGrowthDisplayStatus == null)
            {
                return;
            }

            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BANW_StarGrowthDisplayStatus);
            bool shouldDisplay = ShouldDisplay(pawn);
            if (shouldDisplay && existing == null)
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BANW_StarGrowthDisplayStatus, pawn);
                pawn.health.AddHediff(hediff);
                return;
            }

            if (!shouldDisplay && existing != null)
            {
                pawn.health.RemoveHediff(existing);
            }
        }

        // 判断是否需要显示阶级成长状态，负责避免没有成长配置的 Pawn 出现空状态。
        public static bool ShouldDisplay(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            return BattleStatUtility.GetBaseHealthPercent(pawn) != 0f ||
                   BattleStatUtility.GetBaseAttackPercent(pawn) != 0f ||
                   BattleStatUtility.GetBaseHealPercent(pawn) != 0f;
        }

        // 构建健康状态悬浮说明，负责向玩家展示当前 PawnKind 基础属性带来的加成。
        public static string BuildTooltip(Pawn pawn)
        {
            if (pawn == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("生命值百分比：" + FormatPercent(BattleStatUtility.GetBaseHealthPercent(pawn)));
            builder.AppendLine("攻击力百分比：" + FormatPercent(BattleStatUtility.GetBaseAttackPercent(pawn)));
            builder.AppendLine("治愈力百分比：" + FormatPercent(BattleStatUtility.GetBaseHealPercent(pawn)));
            builder.AppendLine();
            builder.Append("该状态只负责显示 PawnKind 基础属性，实际数值由战斗属性层统一结算。");
            return builder.ToString();
        }

        // 格式化百分比数值，负责把 0.2 显示为 20%。
        private static string FormatPercent(float value)
        {
            return value.ToString("P0", CultureInfo.InvariantCulture);
        }
    }
}
