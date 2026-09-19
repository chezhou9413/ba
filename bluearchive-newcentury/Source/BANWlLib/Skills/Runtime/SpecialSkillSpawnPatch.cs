using System.Linq;
using BANWlLib.KindStats;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //角色生成补丁，负责根据PawnKind扩展安装原生特殊技能状态。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class SpecialSkillSpawnPatch
    {
        //生成或载入时安装缺失状态，并把已有状态注册到地图。
        public static void Postfix(Pawn __instance)
        {
            var profile = __instance.kindDef?.GetModExtension<SpecialSkillKindExtension>()?.profile;
            if (profile != null && Hediff_SpecialSkillState.Find(__instance, profile) == null)
            {
                Hediff_SpecialSkillState.Create(__instance, profile);
                HealthScaleCache.Invalidate(__instance);
            }
            foreach (var state in __instance.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>())
                __instance.Map.GetComponent<MapComponent_SpecialSkills>().Register(state);
        }
    }
}
