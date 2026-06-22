using RimWorld;
using UnityEngine;
using Verse;

namespace DodgeMod
{
    // 闪避计算器，负责从 Pawn 当前属性中读取并限制最终闪避概率。
    public static class DodgeCalculator
    {
        // 获取总闪避概率，负责把 BANW_Miss StatDef 转成 0 到 1 的概率值。
        public static float GetTotalDodgeChance(Pawn pawn)
        {
            if (pawn == null || pawn.Destroyed)
            {
                return 0f;
            }

            return Mathf.Clamp01(pawn.GetStatValue(DodgeStatDefOf.BANW_Miss));
        }
    }
}
