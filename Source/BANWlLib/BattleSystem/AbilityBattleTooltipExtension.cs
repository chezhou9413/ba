using System.Collections.Generic;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 技能战斗悬浮配置，负责声明地图技能按钮需要展示的伤害或治疗公式。
    public class AbilityBattleTooltipExtension : DefModExtension
    {
        public bool showBattleFormula = true;
        public List<BattleActionConfig> previewActions = new List<BattleActionConfig>();
    }
}
