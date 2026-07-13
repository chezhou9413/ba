using HarmonyLib;
using RimWorld;
using Verse;

namespace SandWormLib
{
    // SandWormIncidentPatches 负责阻止原版特殊事件把援助者刷到沙虫挑战地图。
    [HarmonyPatch(typeof(IncidentWorker_WandererJoin), "CanFireNowSub")]
    public static class SandWormIncidentCanFirePatch
    {
        // Postfix 在原版事件判定之后过滤沙虫地图，避免黑衣人等加入事件出现在挑战地图。
        public static void Postfix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        {
            if (!__result || __instance?.def == null || parms?.target == null)
            {
                return;
            }

            Map map = parms.target as Map;
            if (!(map?.Parent is SandWormLeviathanSite))
            {
                return;
            }

            string defName = __instance.def.defName;
            if (defName == "StrangerInBlackJoin" || defName == "GameEndedWanderersJoin")
            {
                __result = false;
            }
        }
    }
}
