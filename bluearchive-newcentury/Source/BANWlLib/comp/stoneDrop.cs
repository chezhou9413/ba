using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace BANWlLib.comp
{
    // XML 配置：最小/最大/掉落物/掉落概率
    public class stoneDrop : Def
    {
        public int min = 2;
        public int max = 5;
        public float maxDropRate = 0.3f; // 掉落概率，0.0-1.0之间
        public ThingDef thingToDrop; // 在 XML 里写物品的 defName
        public List<string> RaceRemove = new List<string>(); // 在 XML 里写物品的 defName
    }

    // 全局读取
    public static class GreenstoneDrop
    {
        private const string DefName = "BioGreenstone_DropCount";
        private static stoneDrop _def;
        // 改为 public，供外部访问
        public static stoneDrop Def
        {
            get
            {
                if (_def == null)
                {
                    _def = DefDatabase<stoneDrop>.GetNamed(DefName, errorOnFail: true);

                    // 防呆：若 XML 写成 min > max，就自动交换
                    if (_def.min > _def.max)
                    {
                        int t = _def.min; _def.min = _def.max; _def.max = t;
                    }
                    
                    // 防呆：确保概率在有效范围内
                    if (_def.maxDropRate < 0f) _def.maxDropRate = 0f;
                    if (_def.maxDropRate > 1f) _def.maxDropRate = 1f;
                }
                return _def;
            }
        }
        public static List<string> RaceRemove { get { return Def.RaceRemove; } }
        public static int Min { get { return Def.min; } }
        public static int Max { get { return Def.max; } }
        public static float DropRate { get { return Def.maxDropRate; } }

        public static int Random() { return Rand.RangeInclusive(Min, Max); }

        // 为了兼容你补丁里的调用，顺手提供一个别名
        public static int RandomCount() { return Random(); }
        
        // 新增：检查是否应该掉落物品
        public static bool ShouldDrop()
        {
            // 如果概率为0，则永不掉落
            if (DropRate <= 0f) return false;
            
            // 如果概率为1，则必定掉落
            if (DropRate >= 1f) return true;
            
            // 否则根据概率随机决定
            return Rand.Value <= DropRate;
        }
    }

    // 拦截 Pawn.Kill，死亡后在死亡位置掉落
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Kill")]
    [HarmonyPatch(new Type[] { typeof(DamageInfo?), typeof(Hediff) })]
    public static class Patch_Pawn_Kill_DropGreenstone
    {
        public static void Postfix(Pawn __instance)
        {
            try
            {
                if (__instance == null) return;
                if (!__instance.Dead) return;

                Map map = (__instance.Corpse != null ? __instance.Corpse.Map : __instance.MapHeld);
                if (map == null) return;

                IntVec3 cell = (__instance.Corpse != null ? __instance.Corpse.PositionHeld : __instance.PositionHeld);
                if (!cell.IsValid) return;

                var cfg = GreenstoneDrop.Def;
                if (cfg == null || cfg.thingToDrop == null)
                {
                    return;
                }
                // 检查种族是否在排除列表中，如果在则跳过掉落
                if (GreenstoneDrop.RaceRemove.Contains(__instance.def.defName))
                {
                    return; // 种族在排除列表中，不执行掉落
                }
                // 新增：概率检查，决定是否掉落
                if (!GreenstoneDrop.ShouldDrop())
                {
                    return; // 概率未命中，不掉落物品
                }

                int total = GreenstoneDrop.RandomCount();
                if (total <= 0) return;

                // 拆堆按堆叠上限掉落
                int remaining = total;
                int limit = Math.Max(1, cfg.thingToDrop.stackLimit);
                while (remaining > 0)
                {
                    int toMake = remaining > limit ? limit : remaining;
                    Thing thing = ThingMaker.MakeThing(cfg.thingToDrop);
                    thing.stackCount = toMake;
                    GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
                    remaining -= toMake;
                }
            }
            catch (Exception e)
            {
                Log.Error($"[GreenstoneDrop] error in Pawn.Kill Postfix: {e}");
            }
        }
    }
}
