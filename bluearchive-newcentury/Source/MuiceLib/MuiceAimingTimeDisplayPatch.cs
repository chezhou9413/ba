using HarmonyLib;
using RimWorld;
using Verse;

namespace MuiceLib;

[HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetStatDrawEntryLabel))]
public static class MuiceAimingTimeLabelPatch
{
    public static void Postfix(StatDef stat, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, ref string __result)
    {
        if (finalized && stat == StatDefOf.AimingDelayFactor && optionalReq.Thing is Pawn pawn)
        {
            __result = stat.ValueToString(GetDisplayedAimingTime(pawn), numberSense);
        }
    }

    internal static float GetDisplayedAimingTime(Pawn pawn)
    {
        return pawn.GetStatValue(StatDefOf.AimingDelayFactor)
            * pawn.GetStatValue(MuiceStatDefOf.Muice_RangedAimingTimeFactor);
    }
}

[HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetExplanationUnfinalized))]
public static class MuiceAimingTimeExplainPatch
{
    public static void Postfix(StatDef ___stat, StatRequest req, ref string __result)
    {
        if (___stat != StatDefOf.AimingDelayFactor || req.Thing is not Pawn pawn)
        {
            return;
        }

        float factor = pawn.GetStatValue(MuiceStatDefOf.Muice_RangedAimingTimeFactor);
        if (factor != 1f)
        {
            __result += $"    {MuiceStatDefOf.Muice_RangedAimingTimeFactor.LabelCap}: x{factor.ToStringPercent()}\n";
        }
    }
}

[HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetExplanationFinalizePart))]
public static class MuiceAimingTimeFinalizePatch
{
    public static void Prefix(StatDef ___stat, StatRequest req, ref float finalVal)
    {
        if (___stat == StatDefOf.AimingDelayFactor && req.Thing is Pawn pawn)
        {
            finalVal = MuiceAimingTimeLabelPatch.GetDisplayedAimingTime(pawn);
        }
    }
}
