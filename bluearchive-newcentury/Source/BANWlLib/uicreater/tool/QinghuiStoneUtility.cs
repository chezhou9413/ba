#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using RimWorld;
using System.Collections.Generic;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
using Verse;

namespace newpro
{
    //统计并扣除地图中玩家拥有的物品。
    public static class ItemUtility
    {
        //按 defName 缓存 ThingDef。
        private static readonly Dictionary<string, ThingDef> _defCache = new Dictionary<string, ThingDef>();

        //读取缓存的 ThingDef。
        private static ThingDef GetCachedDef(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return null;
            }

            if (!_defCache.TryGetValue(defName, out ThingDef def))
            {
                def = DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false);
                _defCache[defName] = def;
            }

            return def;
        }

        //获取当前地图中指定物品的总数
        public static int GetTotalItemCount(string defName)
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                return 0;
            }

            ThingDef itemDef = GetCachedDef(defName);
            if (itemDef == null)
            {
                return 0;
            }

            int total = 0;
            List<Thing> items = map.listerThings.ThingsOfDef(itemDef);

            for (int i = 0; i < items.Count; i++)
            {
                Thing t = items[i];
                if (t.Faction == Faction.OfPlayer || t.Faction == null)
                {
                    total += t.stackCount;
                }
            }

            return total;
        }

        //扣除指定物品的数量
        public static bool TryRemoveItem(string defName, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                return false;
            }

            ThingDef itemDef = GetCachedDef(defName);
            if (itemDef == null)
            {
                return false;
            }

            List<Thing> availableItems = new List<Thing>();
            int total = 0;

            List<Thing> allItems = map.listerThings.ThingsOfDef(itemDef);
            for (int i = 0; i < allItems.Count; i++)
            {
                Thing t = allItems[i];
                if (t.Faction == Faction.OfPlayer || t.Faction == null)
                {
                    availableItems.Add(t);
                    total += t.stackCount;
                }
            }

            if (total < amount)
            {
                return false;
            }

            availableItems.Sort((a, b) => b.stackCount.CompareTo(a.stackCount));

            int remaining = amount;
            for (int i = 0; i < availableItems.Count && remaining > 0; i++)
            {
                Thing thing = availableItems[i];
                int take = Mathf.Min(thing.stackCount, remaining);
                thing.SplitOff(take).Destroy(DestroyMode.Vanish);
                remaining -= take;
            }

            return true;
        }

        //清除 Def 缓存
        public static void ClearCache()
        {
            _defCache.Clear();
        }
    }

    //保存地图空投位置，定期更新 UI 资源数量。
    public class MapComponent_EveryFrame : MapComponent
    {
        //空投位置
        public IntVec3 dropCell;

        //上次更新资源显示的 Tick
        private int lastUpdateTick = 0;

        //缓存的 PoitSaveComponent 引用
        private PoitSaveComponent _cachedPoitComp;

        public MapComponent_EveryFrame(Map map) : base(map)
        {
        }

        //存档序列化
        public override void ExposeData()
        {
            Scribe_Values.Look(ref dropCell, "dropCell");
        }

        //按间隔刷新资源数量。
        public override void MapComponentUpdate()
        {
            // UI 关闭时跳过资源更新
            if (!UiMapData.uiclose)
            {
                return;
            }

            // 每秒更新一次资源显示（60 Tick）
            int currentTick = Find.TickManager.TicksGame;
            if (currentTick - lastUpdateTick < 60)
            {
                return;
            }
            lastUpdateTick = currentTick;

            UpdateResourceDisplay();
        }

        //更新 UI 上的资源数量显示
        private void UpdateResourceDisplay()
        {
            // 青辉石数量
            string qinghuiCount = ItemUtility.GetTotalItemCount("QinghuiStone").ToString();
            if (UiMapData.stoneText != null)
            {
                UiMapData.stoneText.text = qinghuiCount;
            }
            if (UiMapData.qinghuishitext1 != null)
            {
                UiMapData.qinghuishitext1.text = qinghuiCount;
            }

            // 积分数量
            string poitCount = (GetCachedPoitComp()?.poit ?? 0).ToString();
            if (UiMapData.poitText != null)
            {
                UiMapData.poitText.text = poitCount;
            }
            if (UiMapData.poitText2 != null)
            {
                UiMapData.poitText2.text = poitCount;
            }

            // 银币数量
            if (UiMapData.huangpiaotext1 != null)
            {
                UiMapData.huangpiaotext1.text = ItemUtility.GetTotalItemCount("Silver").ToString();
            }

            // 精粹数量
            if (UiMapData.jingcuixianshi != null)
            {
                UiMapData.jingcuixianshi.text = ItemUtility.GetTotalItemCount("Kami_Proplevel").ToString();
            }
        }

        //获取缓存的 PoitSaveComponent
        private PoitSaveComponent GetCachedPoitComp()
        {
            if (_cachedPoitComp == null && Current.Game != null)
            {
                _cachedPoitComp = Current.Game.GetComponent<PoitSaveComponent>();
            }
            return _cachedPoitComp;
        }
    }
}
