using System.Linq;
using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //角色离图清理，负责在健康系统停止更新前释放地图实体和持续特效。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DeSpawn))]
    public static class SpecialSkillDespawnPatch
    {
        //角色离图前保存的状态继续计时，但地图记录和拦截表现立即结束。
        public static void Prefix(Pawn __instance)
        {
            foreach (var state in __instance.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>().ToList())
                SpecialSkillDispatcher.LeaveMap(state);
        }
    }
}
