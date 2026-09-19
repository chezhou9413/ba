using HarmonyLib;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //星野施法减伤补丁，负责从当前承伤系数中减去指定百分点。
    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized))]
    public static class HoshinoIncomingDamagePatch
    {
        //仅在输出EX执行期间调整承伤系数，并禁止负伤害。
        public static void Postfix(StatRequest req, StatDef ___stat, ref float __result)
        {
            if (___stat != StatDefOf.IncomingDamageFactor || !(req.Thing is Pawn pawn)) return;
            var state = pawn.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>()
                .FirstOrDefault(s => s.profile.role == SpecialSkillRole.Hoshino && s.stage == 0 && s.castEndTick >= s.Now);
            if (state != null && state.stage == 0 && state.castEndTick >= state.Now)
                __result = Mathf.Max(0f, __result - state.profile.damageTakenReduction);
        }
    }
}
