using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //黑子致命伤保护补丁，在伤口破坏身体部位前检测和限制致死伤害。
    [HarmonyPatch(typeof(DamageWorker_AddInjury), "FinalizeAndAddInjury",
        typeof(Pawn), typeof(Hediff_Injury), typeof(DamageInfo), typeof(DamageWorker.DamageResult))]
    public static class ShirokoLethalPatch
    {
        //沿用致命伤保护和伤口限制；层数由游戏时钟推进，达到上限后不再拦截死亡。
        public static bool Prefix(Pawn pawn, Hediff_Injury injury, ref float __result)
        {
            var state = Hediff_SpecialSkillState.Find(pawn, SpecialSkillRole.Shiroko);
            if (state == null || !pawn.health.WouldDieAfterAddingHediff(injury)) return true;
            int id = SpecialSkillEvents.CurrentDamage?.id ?? -Find.TickManager.TicksGame - 1;
            if (ShirokoSkills.ProtectLethal(state, id)) SpecialHealthUtility.LimitToSurvivable(pawn, injury);
            if (!pawn.Dead) return true;
            __result = 0;
            return false;
        }
    }
}

