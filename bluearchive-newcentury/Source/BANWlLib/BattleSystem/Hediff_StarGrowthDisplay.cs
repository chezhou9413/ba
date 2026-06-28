using Verse;

namespace BANWlLib.BattleSystem
{
    // 星级成长显示状态，负责在健康页展示当前星级提供的生命、攻击和治疗成长。
    public class Hediff_StarGrowthDisplay : HediffWithComps
    {
        // 状态基础名称，负责让健康页显示稳定的状态名。
        public override string LabelBase
        {
            get { return "星级成长"; }
        }

        // 状态括号信息，负责直接显示当前星级。
        public override string LabelInBrackets
        {
            get { return BattleStatUtility.GetCurrentRankLevel(pawn) + "星"; }
        }

        // 状态悬浮说明，负责展示当前星级实际带来的各类成长。
        public override string TipStringExtra
        {
            get { return StarGrowthDisplayUtility.BuildTooltip(pawn); }
        }

        // 状态是否应被移除，负责在成长配置不存在时自动清理显示状态。
        public override bool ShouldRemove
        {
            get { return !StarGrowthDisplayUtility.ShouldDisplay(pawn); }
        }
    }
}
