using System.Linq;
using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //角色死亡清理，负责只在真实死亡后结束无人机和地图效果。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill), typeof(DamageInfo?), typeof(Hediff))]
    public static class SpecialSkillDeathPatch
    {
        //任务免死仍保持角色存活时不清理技能状态。
        public static void Postfix(Pawn __instance)
        {
            if (!__instance.Dead) return;
            foreach (var state in __instance.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>().ToList())
                SpecialSkillDispatcher.LeaveMap(state);
        }
    }
}
