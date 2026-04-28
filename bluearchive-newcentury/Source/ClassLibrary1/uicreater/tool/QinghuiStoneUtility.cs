using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace newpro
{
    /// <summary>
    /// 通用物品工具类
    /// 
    /// 职责：
    /// - 统计地图中玩家拥有的指定物品数量
    /// - 扣除指定数量的物品
    /// 
    /// 适用场景：
    /// - 抽卡消耗货币
    /// - 商店购买
    /// - 任何需要检查/消耗物品的系统
    /// </summary>
    public static class ItemUtility
    {
        /// <summary>
        /// ThingDef 缓存，避免重复查询 DefDatabase
        /// Key: defName, Value: ThingDef
        /// </summary>
        private static readonly Dictionary<string, ThingDef> _defCache = new Dictionary<string, ThingDef>();

        /// <summary>
        /// 获取缓存的 ThingDef（性能优化）
        /// </summary>
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

        /// <summary>
        /// 获取当前地图中指定物品的总数
        /// </summary>
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

        /// <summary>
        /// 扣除指定物品的数量
        /// </summary>
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

        /// <summary>
        /// 清除 Def 缓存
        /// </summary>
        public static void ClearCache()
        {
            _defCache.Clear();
        }
    }

    /// <summary>
    /// 地图级别组件：每帧更新
    /// 
    /// 职责：
    /// 1. 管理空投位置 (dropCell)
    /// 2. 更新 UI 显示的资源数量
    /// </summary>
    public class MapComponent_EveryFrame : MapComponent
    {
        /// <summary>
        /// 空投位置
        /// </summary>
        public IntVec3 dropCell;

        /// <summary>
        /// 上次更新资源显示的 Tick
        /// </summary>
        private int lastUpdateTick = 0;

        /// <summary>
        /// 缓存的 PoitSaveComponent 引用
        /// </summary>
        private PoitSaveComponent _cachedPoitComp;

        public MapComponent_EveryFrame(Map map) : base(map)
        {
        }

        /// <summary>
        /// 存档序列化
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref dropCell, "dropCell");
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
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

        /// <summary>
        /// 更新 UI 上的资源数量显示
        /// </summary>
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

        /// <summary>
        /// 获取缓存的 PoitSaveComponent
        /// </summary>
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