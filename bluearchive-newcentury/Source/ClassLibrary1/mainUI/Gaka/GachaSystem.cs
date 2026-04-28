using System.Collections.Generic;
using Verse;

namespace BANWlLib.mainUI.Gaka
{
    /// <summary>
    /// 抽卡系统静态工具类
    /// </summary>
    public static class GachaSystem
    {
        public static bool DebugGuarantee3Star = false;

        /// <summary>
        /// 纯随机单抽
        /// </summary>
        public static ThingDef Draw(List<ThingDef> p1, List<ThingDef> p2, List<ThingDef> p3, List<ThingDef> upList, float w1, float w2, float w3, float upRate)
        {
            if (DebugGuarantee3Star && Prefs.DevMode)
            {
                return GetTier3Result(p3, upList, upRate);
            }
            float totalWeight = w1 + w2 + w3;
            if (totalWeight <= 0) return null;

            float dice = Rand.Range(0, totalWeight);

            if (dice < w3)
            {
                return GetTier3Result(p3, upList, upRate);
            }

            if (dice < w3 + w2 && !p2.NullOrEmpty())
            {
                return p2.RandomElement();
            }

            if (!p1.NullOrEmpty()) return p1.RandomElement();

            return p2.NullOrEmpty() ? GetTier3Result(p3, upList, upRate) : p2.RandomElement();
        }

        /// <summary>
        /// FES池单抽 - 特殊概率分配
        /// </summary>
        public static ThingDef DrawFES(List<ThingDef> p1, List<ThingDef> p2, List<ThingDef> p3,
            List<ThingDef> fesUpList, List<ThingDef> fesOtherList)
        {
            if (DebugGuarantee3Star && Prefs.DevMode)
            {
                return GetFESTier3Result(p3, fesUpList, fesOtherList);
            }

            // FES池概率: 一星75.5%, 二星18.5%, 三星6%
            float totalWeight = 75.5f + 18.5f + 6f;
            float dice = Rand.Range(0, totalWeight);

            // 判定三星 (6%)
            if (dice < 6f)
            {
                return GetFESTier3Result(p3, fesUpList, fesOtherList);
            }

            // 判定二星 (18.5%)
            if (dice < 6f + 18.5f && !p2.NullOrEmpty())
            {
                return p2.RandomElement();
            }

            // 默认一星 (75.5%)
            if (!p1.NullOrEmpty()) return p1.RandomElement();

            return p2.NullOrEmpty() ? GetFESTier3Result(p3, fesUpList, fesOtherList) : p2.RandomElement();
        }

        /// <summary>
        /// FES池十连抽
        /// </summary>
        public static List<ThingDef> MultiDrawFES10(List<ThingDef> p1, List<ThingDef> p2, List<ThingDef> p3,
            List<ThingDef> fesUpList, List<ThingDef> fesOtherList)
        {
            List<ThingDef> results = new List<ThingDef>();
            if (DebugGuarantee3Star && Prefs.DevMode)
            {
                for (int i = 0; i < 10; i++)
                {
                    results.Add(GetFESTier3Result(p3, fesUpList, fesOtherList));
                }
                return results;
            }

            bool hasHighStar = false;

            // 前9次随机抽
            for (int i = 0; i < 9; i++)
            {
                ThingDef item = DrawFES(p1, p2, p3, fesUpList, fesOtherList);
                if (item != null && (p2.Contains(item) || p3.Contains(item) || fesUpList.Contains(item) || fesOtherList.Contains(item)))
                {
                    hasHighStar = true;
                }
                results.Add(item);
            }

            // 第10次保底
            if (hasHighStar)
            {
                results.Add(DrawFES(p1, p2, p3, fesUpList, fesOtherList));
            }
            else
            {
                results.Add(DrawFESGuaranteed(p2, p3, fesUpList, fesOtherList));
            }

            return results;
        }

        /// <summary>
        /// 十连抽：普通池保底逻辑
        /// </summary>
        public static List<ThingDef> MultiDraw10(List<ThingDef> p1, List<ThingDef> p2, List<ThingDef> p3, List<ThingDef> upList, float w1, float w2, float w3, float upRate)
        {
            List<ThingDef> results = new List<ThingDef>();
            if (DebugGuarantee3Star && Prefs.DevMode)
            {
                for (int i = 0; i < 10; i++)
                {
                    results.Add(GetTier3Result(p3, upList, upRate));
                }
                return results;
            }
            bool hasHighStar = false;

            for (int i = 0; i < 9; i++)
            {
                ThingDef item = Draw(p1, p2, p3, upList, w1, w2, w3, upRate);
                if (item != null && (p2.Contains(item) || p3.Contains(item) || upList.Contains(item)))
                {
                    hasHighStar = true;
                }
                results.Add(item);
            }

            if (hasHighStar)
            {
                results.Add(Draw(p1, p2, p3, upList, w1, w2, w3, upRate));
            }
            else
            {
                results.Add(DrawGuaranteed(p2, p3, upList, w2, w3, upRate));
            }

            return results;
        }

        /// <summary>
        /// FES三星层级抽取逻辑
        /// 当期FES: 0.7%, 其他FES: 0.3%, 常驻三星: 均分剩余概率
        /// </summary>
        private static ThingDef GetFESTier3Result(List<ThingDef> p3, List<ThingDef> fesUpList, List<ThingDef> fesOtherList)
        {
            // 总三星概率 6%
            // 当期FES: 0.7% = 0.7/6 ≈ 11.67%
            // 其他FES: 0.3% = 0.3/6 = 5%
            // 常驻三星: 5% = 5/6 ≈ 83.33%

            float fesUpChance = 0.7f / 6f;      // 约 0.1167
            float fesOtherChance = 0.3f / 6f;   // 0.05
            float normalChance = 5f / 6f;       // 约 0.8333

            float dice = Rand.Value;

            // 判定当期FES
            if (!fesUpList.NullOrEmpty() && dice < fesUpChance)
            {
                return fesUpList.RandomElement();
            }

            // 判定其他FES
            if (!fesOtherList.NullOrEmpty() && dice < fesUpChance + fesOtherChance)
            {
                return fesOtherList.RandomElement();
            }

            // 判定常驻三星
            if (!p3.NullOrEmpty())
            {
                return p3.RandomElement();
            }

            // 兜底逻辑
            if (!fesUpList.NullOrEmpty()) return fesUpList.RandomElement();
            if (!fesOtherList.NullOrEmpty()) return fesOtherList.RandomElement();
            return null;
        }

        /// <summary>
        /// 普通池三星层级内决定是UP还是普池
        /// </summary>
        private static ThingDef GetTier3Result(List<ThingDef> p3, List<ThingDef> upList, float upRate)
        {
            if (!upList.NullOrEmpty() && Rand.Value < upRate)
            {
                return upList.RandomElement();
            }

            if (!p3.NullOrEmpty())
            {
                return p3.RandomElement();
            }

            return upList.NullOrEmpty() ? null : upList.RandomElement();
        }

        /// <summary>
        /// FES池保底抽取
        /// </summary>
        private static ThingDef DrawFESGuaranteed(List<ThingDef> p2, List<ThingDef> p3,
            List<ThingDef> fesUpList, List<ThingDef> fesOtherList)
        {
            // 保底只在二星和三星之间,按 18.5 : 6 的比例
            float totalWeight = 18.5f + 6f;
            float dice = Rand.Range(0, totalWeight);

            if (dice < 6f)
            {
                return GetFESTier3Result(p3, fesUpList, fesOtherList);
            }

            return p2.NullOrEmpty() ? GetFESTier3Result(p3, fesUpList, fesOtherList) : p2.RandomElement();
        }

        /// <summary>
        /// 普通池保底抽取
        /// </summary>
        private static ThingDef DrawGuaranteed(List<ThingDef> p2, List<ThingDef> p3, List<ThingDef> upList, float w2, float w3, float upRate)
        {
            float totalWeight = w2 + w3;
            if (totalWeight <= 0) return p2.NullOrEmpty() ? GetTier3Result(p3, upList, upRate) : p2.RandomElement();

            float dice = Rand.Range(0, totalWeight);

            if (dice < w3)
            {
                return GetTier3Result(p3, upList, upRate);
            }

            return p2.NullOrEmpty() ? GetTier3Result(p3, upList, upRate) : p2.RandomElement();
        }
    }
}