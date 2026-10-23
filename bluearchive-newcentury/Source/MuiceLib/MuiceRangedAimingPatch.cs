using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MuiceLib;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Verb), nameof(Verb.TryStartCastOn), new Type[]
{
    typeof(LocalTargetInfo), typeof(LocalTargetInfo),
    typeof(bool), typeof(bool), typeof(bool), typeof(bool)
})]
public static class MuiceRangedAimingPatch
{
    static MuiceRangedAimingPatch()
    {
        new Harmony("kalospacer.muicelib").PatchAll(typeof(MuiceRangedAimingPatch).Assembly);
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        FieldInfo aimingStat = AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.AimingDelayFactor));
        MethodInfo getStatValue = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue),
            new Type[] { typeof(Thing), typeof(StatDef), typeof(bool), typeof(int) });
        int insertionIndex = -1;

        for (int i = 0; i < codes.Count - 3; i++)
        {
            if (codes[i].LoadsField(aimingStat)
                && codes[i + 1].opcode == OpCodes.Ldc_I4_1
                && codes[i + 2].opcode == OpCodes.Ldc_I4_M1
                && codes[i + 3].Calls(getStatValue))
            {
                insertionIndex = i + 4;
                break;
            }
        }

        codes.InsertRange(insertionIndex, new CodeInstruction[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MuiceRangedAimingPatch), nameof(ApplyPawnRangedAimingFactor)))
        });
        return codes;
    }

    public static float ApplyPawnRangedAimingFactor(float vanillaAimingFactor, Verb verb)
    {
        if (verb.CasterPawn is not Pawn pawn
            || !verb.verbProps.Ranged
            || verb is IAbilityVerb
            || verb.DirectOwner is Ability)
        {
            return vanillaAimingFactor;
        }

        return vanillaAimingFactor * pawn.GetStatValue(MuiceStatDefOf.Muice_RangedAimingTimeFactor);
    }
}
