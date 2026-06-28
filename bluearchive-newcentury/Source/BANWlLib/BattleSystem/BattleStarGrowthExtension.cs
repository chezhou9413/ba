using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 阶级成长数值配置，负责支持按每阶固定表或每阶线性增量计算成长值。
    public class BattleStarGrowthValue
    {
        // 每阶线性增量，负责在没有 starValues 时按阶级差计算成长。
        public float perStar = 0f;

        // 每阶固定值表，负责让每个阶级直接读取对应成长值。
        public List<float> starValues;

        // 计算当前阶级对应成长值，负责优先读取固定表并在缺少固定表时使用线性增量。
        public float Evaluate(int starLevel)
        {
            int resolvedStarLevel = Mathf.Max(1, starLevel);
            if (starValues != null && starValues.Count > 0)
            {
                int index = Mathf.Clamp(resolvedStarLevel - 1, 0, starValues.Count - 1);
                return starValues[index];
            }

            return Mathf.Max(0, resolvedStarLevel - 1) * perStar;
        }
    }

    // 阶级成长扩展，负责让 PawnKindDef 配置生命、攻击和治愈力的每阶成长。
    public class BattleStarGrowthExtension : DefModExtension
    {
        // 固定生命值成长，参与生命值公式的固定加算项，1 表示 100 点生命值。
        public BattleStarGrowthValue healthFlat = new BattleStarGrowthValue();

        // 升星生命值倍率成长，参与生命值公式的升星乘区。
        public BattleStarGrowthValue healthPercent = new BattleStarGrowthValue();

        // 基础攻击力平加成长，进入最终攻击力计算。
        public BattleStarGrowthValue attackFlat = new BattleStarGrowthValue();

        // 攻击力百分比成长，进入最终攻击力倍率计算。
        public BattleStarGrowthValue attackPercent = new BattleStarGrowthValue();

        // 升星固定治愈力成长，参与治愈力公式的固定加算项。
        public BattleStarGrowthValue healFlat = new BattleStarGrowthValue();

        // 升星治愈力倍率成长，参与治愈力公式的升星乘区。
        public BattleStarGrowthValue healPercent = new BattleStarGrowthValue();
    }
}
