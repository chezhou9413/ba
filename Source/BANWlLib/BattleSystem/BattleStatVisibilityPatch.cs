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

            __result = IsPawnStatRequest(req);
            return false;
        }

        // 判断是否是战斗属性，负责限定补丁范围，避免影响原版和其他模组属性。
        private static bool IsBattleStat(StatDef statDef)
        {
            return statDef?.category != null && statDef.category.defName == "BANW_BattleStats";
        }

        // 判断是否是角色属性请求，负责让角色面板显示战斗属性，同时让武器和普通物品面板跳过。
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
    }
}
