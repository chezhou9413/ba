using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 战斗属性显示补丁，负责接管 BANW_BattleStats 在角色和物品信息卡中的显示规则。
    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.ShouldShowFor))]
    public static class BattleStatVisibilityPatch
    {
        // 判断属性是否应该显示，负责避免原版 StatWorker 对自定义战斗属性输出未处理分支红字。
        public static bool Prefix(StatRequest req, StatDef ___stat, ref bool __result)
        {
            if (!IsBattleStat(___stat))
            {
                return true;
            }

            __result = IsPawnStatRequest(req) || IsConfiguredEquipmentStatRequest(req, ___stat);
            return false;
        }

        // 判断是否是战斗属性，负责限定补丁范围，避免影响原版和其他模组属性。
        private static bool IsBattleStat(StatDef statDef)
        {
            return statDef?.category != null && statDef.category.defName == "BANW_BattleStats";
        }

        // 判断是否是角色属性请求，负责让角色面板显示战斗属性。
        private static bool IsPawnStatRequest(StatRequest req)
        {
            if (req.Empty)
            {
                return false;
            }

            if (req.HasThing)
            {
                return req.Thing is Pawn || req.Thing?.def?.race != null;
            }

            ThingDef thingDef = req.Def as ThingDef;
            return thingDef?.race != null;
        }

        // 判断是否是已配置战斗属性的装备请求，负责让武器和衣服只显示自己实际配置的加成。
        private static bool IsConfiguredEquipmentStatRequest(StatRequest req, StatDef statDef)
        {
            ThingDef thingDef = GetThingDef(req);
            if (!IsEquipmentThingDef(thingDef))
            {
                return false;
            }

            return HasStatBase(thingDef, statDef) || HasEquippedStatOffset(thingDef, statDef);
        }

        // 读取属性请求对应的 ThingDef，负责兼容实物信息卡和 Def 信息卡。
        private static ThingDef GetThingDef(StatRequest req)
        {
            if (req.Empty)
            {
                return null;
            }

            if (req.HasThing)
            {
                return req.Thing?.def;
            }

            return req.Def as ThingDef;
        }

        // 判断 ThingDef 是否是可装备物品，负责覆盖武器、服装和可作为装备使用的物品。
        private static bool IsEquipmentThingDef(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return false;
            }

            return thingDef.IsWeapon || thingDef.apparel != null || thingDef.equipmentType != EquipmentType.None;
        }

        // 判断 Def 本体属性里是否配置了目标战斗属性，负责显示物品自带数值。
        private static bool HasStatBase(ThingDef thingDef, StatDef statDef)
        {
            if (thingDef?.statBases == null || statDef == null)
            {
                return false;
            }

            for (int i = 0; i < thingDef.statBases.Count; i++)
            {
                if (thingDef.statBases[i].stat == statDef)
                {
                    return true;
                }
            }

            return false;
        }

        // 判断装备加成里是否配置了目标战斗属性，负责显示穿戴后提供给 Pawn 的加成。
        private static bool HasEquippedStatOffset(ThingDef thingDef, StatDef statDef)
        {
            if (thingDef?.equippedStatOffsets == null || statDef == null)
            {
                return false;
            }

            for (int i = 0; i < thingDef.equippedStatOffsets.Count; i++)
            {
                if (thingDef.equippedStatOffsets[i].stat == statDef)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
