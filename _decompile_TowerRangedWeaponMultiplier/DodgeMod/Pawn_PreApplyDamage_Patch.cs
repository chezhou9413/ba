using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DodgeMod;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("PreApplyDamage")]
public static class Pawn_PreApplyDamage_Patch
{
	[HarmonyPrefix]
	public static bool Prefix(Pawn __instance, ref DamageInfo dinfo)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			DamageDef def = ((DamageInfo)(ref dinfo)).Def;
			if (((def != null) ? ((Def)def).GetModExtension<IgnoreDodgeExtension>() : null) != null)
			{
				return true;
			}
			float totalDodgeChance = DodgeCalculator.GetTotalDodgeChance(__instance);
			if (totalDodgeChance > 0f && Rand.Value < totalDodgeChance)
			{
				if (((Thing)__instance).Map != null)
				{
					MoteMaker.ThrowText(((Thing)__instance).DrawPos, ((Thing)__instance).Map, "MISS", Color.white, 3.9f);
				}
				((DamageInfo)(ref dinfo)).SetAmount(0f);
				return false;
			}
		}
		catch (Exception arg)
		{
			Log.Error($"Error in dodge processing: {arg}");
		}
		return true;
	}
}
