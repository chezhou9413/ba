using System.Collections.Generic;
using Verse;

namespace BANWlLib.KindStats
{
    // 生命倍率缓存，负责把 Pawn.HealthScale 的高频重复计算限制到每个 Pawn 每个游戏 tick 一次。
    public static class HealthScaleCache
    {
        private static readonly Dictionary<int, HealthScaleCacheEntry> CacheByPawnId = new Dictionary<int, HealthScaleCacheEntry>();
        private static int lastCleanupTick = -1;

        // 获取生命倍率，负责在同 tick 命中缓存时直接返回上次完整计算结果。
        public static float GetOrCalculate(Pawn pawn, float originalHealthScale)
        {
            if (pawn == null)
            {
                return originalHealthScale;
            }

            int tick = CurrentTick();
            int pawnId = pawn.thingIDNumber;
            HealthScaleCacheEntry entry;
            if (CacheByPawnId.TryGetValue(pawnId, out entry) && entry.tick == tick && entry.originalHealthScale == originalHealthScale)
            {
                return entry.value;
            }

            float value = BANWKindStatUtility.CalculateHealthScaleUncached(pawn, originalHealthScale);
            CacheByPawnId[pawnId] = new HealthScaleCacheEntry
            {
                tick = tick,
                originalHealthScale = originalHealthScale,
                value = value
            };
            CleanupOldEntries(tick);
            return value;
        }

        // 清除单个 Pawn 的缓存，负责在星级等主动变化时立即重新计算。
        public static void Invalidate(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            CacheByPawnId.Remove(pawn.thingIDNumber);
        }

        // 清理全部缓存，负责在批量状态变化或地图切换后移除旧数据。
        public static void Clear()
        {
            CacheByPawnId.Clear();
        }

        // 获取当前游戏 tick，负责在非游戏态下提供稳定缓存键。
        private static int CurrentTick()
        {
            return Current.Game?.tickManager?.TicksGame ?? -1;
        }

        // 清理旧缓存项，负责避免长时间游戏后字典保留已销毁 Pawn 的数据。
        private static void CleanupOldEntries(int tick)
        {
            if (tick < 0 || lastCleanupTick == tick || tick - lastCleanupTick < 2500)
            {
                return;
            }

            lastCleanupTick = tick;
            List<int> removeKeys = null;
            foreach (KeyValuePair<int, HealthScaleCacheEntry> pair in CacheByPawnId)
            {
                if (tick - pair.Value.tick > 2500)
                {
                    if (removeKeys == null)
                    {
                        removeKeys = new List<int>();
                    }

                    removeKeys.Add(pair.Key);
                }
            }

            if (removeKeys == null)
            {
                return;
            }

            for (int i = 0; i < removeKeys.Count; i++)
            {
                CacheByPawnId.Remove(removeKeys[i]);
            }
        }

        // 生命倍率缓存项，负责保存某个 Pawn 在某个 tick 的计算结果。
        private class HealthScaleCacheEntry
        {
            public int tick;
            public float originalHealthScale;
            public float value;
        }
    }
}
