using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 战斗属性 StatWorker 值注入补丁，负责把 PawnKind 基础属性和叠层状态加成注入原版 StatWorker，让角色面板显示完整加成。
    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized))]
    public static class BattleStatWorkerValuePatch
    {
        // 在原版计算后追加 BA 战斗属性额外加成，负责让角色面板的属性值包含 PawnKind 基础属性和叠层状态。
        public static void Postfix(StatRequest req, StatDef ___stat, ref float __result)
        {
            if (!IsBattleStat(___stat) || !req.HasThing || !(req.Thing is Pawn pawn))
            {
                return;
            }

            __result += BattleStatUtility.GetBaseStatOffset(pawn, ___stat);
            __result += BattleStatUtility.GetAdditionalBattleStatOffset(pawn, ___stat);
        }

        // 判断是否是战斗属性，负责限定补丁范围。
        private static bool IsBattleStat(StatDef statDef)
        {
            return statDef?.category != null && statDef.category.defName == "BANW_BattleStats";
        }
    }
}
