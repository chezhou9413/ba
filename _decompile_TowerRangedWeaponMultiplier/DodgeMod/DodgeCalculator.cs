using RimWorld;
using UnityEngine;
using Verse;

namespace DodgeMod;

public static class DodgeCalculator
{
	public static float GetTotalDodgeChance(Pawn pawn)
	{
		if (pawn == null || ((Thing)pawn).Destroyed)
		{
			return 0f;
		}
		try
		{
			return Mathf.Clamp01(StatExtension.GetStatValue((Thing)(object)pawn, DodgeStatDefOf.BANW_Miss, true, -1));
		}
		catch
		{
			return 0f;
		}
	}
}
