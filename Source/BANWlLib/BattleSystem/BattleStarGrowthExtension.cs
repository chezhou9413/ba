using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    public class BattleStarGrowthValue
    {
        public float perStar = 0f;
        public List<float> starValues;

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

    public class BattleStarGrowthExtension : DefModExtension
    {
        public BattleStarGrowthValue healthFlat = new BattleStarGrowthValue();
        public BattleStarGrowthValue healthPercent = new BattleStarGrowthValue();
        public BattleStarGrowthValue attackFlat = new BattleStarGrowthValue();
        public BattleStarGrowthValue attackPercent = new BattleStarGrowthValue();
        public BattleStarGrowthValue healFlat = new BattleStarGrowthValue();
        public BattleStarGrowthValue healPercent = new BattleStarGrowthValue();
    }
}
