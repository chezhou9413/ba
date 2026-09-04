using System.Collections.Generic;

namespace BANWlLib.CostSystem
{
    //COST规则负责描述一张地图的费用上限与基础回复倍率。
    public class BACostRules
    {
        public int maximumCost = 10;
        public float recoveryMultiplier = 1f;

        //检查任务中的COST配置是否处于系统支持范围。
        public IEnumerable<string> ConfigErrors(string ownerDefName)
        {
            if (maximumCost != 10 && maximumCost != 20)
            {
                yield return ownerDefName + " 的 maximumCost 只能为 10 或 20。";
            }

            if (recoveryMultiplier <= 0f)
            {
                yield return ownerDefName + " 的 recoveryMultiplier 必须大于 0。";
            }
        }
    }
}
