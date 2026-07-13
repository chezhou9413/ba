using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SandWormLib
{
    // SandWormHitPointUtility 负责把设置中的血量倍率同步到所有沙虫和命中代理 Def。
    public static class SandWormHitPointUtility
    {
        private const string WormDefName = "SandWorm_Thing";
        private const string HitProxyDefName = "SandWorm_HitProxy";
        private const string SmallWormDefName = "SandWorm_SmallThing";
        private const string SmallHitProxyDefName = "SandWorm_SmallHitProxy";

        // SyncConfiguredMaxHitPoints 负责在设置变化或加载完成后刷新 Def 上的最大生命值。
        public static void SyncConfiguredMaxHitPoints()
        {
            SyncMaxHitPoints(WormDefName, HitProxyDefName, 50000);
            SyncMaxHitPoints(SmallWormDefName, SmallHitProxyDefName, 20000);
        }

        // SyncMaxHitPoints 负责同步一个沙虫 Thing 和它的命中代理 Def。
        private static void SyncMaxHitPoints(string wormDefName, string hitProxyDefName, int baseMaxHitPoints)
        {
            int maxHitPoints = SandWormThing.ScaledMaxHitPointsForBase(baseMaxHitPoints);
            SetMaxHitPoints(wormDefName, maxHitPoints);
            SetMaxHitPoints(hitProxyDefName, maxHitPoints);
        }

        // SetMaxHitPoints 负责写入或补齐指定 ThingDef 的 MaxHitPoints statBase。
        private static void SetMaxHitPoints(string defName, int maxHitPoints)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null)
            {
                return;
            }

            if (def.statBases == null)
            {
                def.statBases = new List<StatModifier>();
            }

            for (int i = 0; i < def.statBases.Count; i++)
            {
                if (def.statBases[i].stat == StatDefOf.MaxHitPoints)
                {
                    StatModifier modifier = def.statBases[i];
                    modifier.value = maxHitPoints;
                    def.statBases[i] = modifier;
                    return;
                }
            }

            def.statBases.Add(new StatModifier
            {
                stat = StatDefOf.MaxHitPoints,
                value = maxHitPoints
            });
        }
    }
}
