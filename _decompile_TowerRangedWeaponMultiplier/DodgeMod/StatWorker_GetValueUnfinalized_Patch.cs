using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DodgeMod;

[HarmonyPatch(typeof(StatWorker))]
[HarmonyPatch("GetValueUnfinalized")]
public static class StatWorker_GetValueUnfinalized_Patch
{
	[HarmonyPostfix]
	public static void Postfix(ref float __result, StatRequest req, StatDef ___stat)
	{
		try
		{
			if (___stat != DodgeStatDefOf.BANW_Miss || !((StatRequest)(ref req)).HasThing || !(((StatRequest)(ref req)).Thing is Pawn))
			{
				return;
			}
			Thing thing = ((StatRequest)(ref req)).Thing;
			Pawn val = (Pawn)(object)((thing is Pawn) ? thing : null);
			if (val == null || ((Thing)val).Destroyed)
			{
				return;
			}
			__result = 0f;
			if (val.story?.traits?.allTraits != null)
			{
				foreach (Trait allTrait in val.story.traits.allTraits)
				{
					if (((allTrait == null) ? null : allTrait.CurrentData?.statOffsets) != null)
					{
						__result += StatUtility.GetStatOffsetFromList(allTrait.CurrentData.statOffsets, DodgeStatDefOf.BANW_Miss);
					}
				}
			}
			Pawn_ApparelTracker apparel = val.apparel;
			if (((apparel != null) ? apparel.WornApparel : null) != null)
			{
				foreach (Apparel item in val.apparel.WornApparel)
				{
					if (item != null)
					{
						__result += StatExtension.GetStatValue((Thing)(object)item, DodgeStatDefOf.BANW_Miss, true, -1);
					}
				}
			}
			if (val.health?.hediffSet?.hediffs == null)
			{
				return;
			}
			foreach (Hediff hediff in val.health.hediffSet.hediffs)
			{
				if (((hediff == null) ? null : hediff.CurStage?.statOffsets) != null)
				{
					__result += StatUtility.GetStatOffsetFromList(hediff.CurStage.statOffsets, DodgeStatDefOf.BANW_Miss);
				}
			}
		}
		catch (Exception arg)
		{
			Log.Warning($"Error in dodge stat calculation: {arg}");
		}
	}
}
