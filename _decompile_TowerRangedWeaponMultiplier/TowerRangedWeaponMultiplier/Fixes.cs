using System;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TowerRangedWeaponMultiplier;

public static class Fixes
{
	[HarmonyPatch(typeof(ProjectileProperties), "GetDamageAmount", new Type[]
	{
		typeof(float),
		typeof(Thing),
		typeof(StringBuilder)
	})]
	private static class FixOne
	{
		private static readonly StatDef DamageStat = SafeGetStatDef("BANW_RangedWeapon_Damage");

		private static readonly StatDef FinalDamageStat = SafeGetStatDef("BANW_FinalDamageMultiplier");

		private static int lastProcessedFrame = -1;

		private static float originalDamageMultiplier = 1f;

		[HarmonyPrefix]
		private static void Prefix(ref float damageMultiplier, Thing weapon)
		{
			try
			{
				if (weapon == null)
				{
					return;
				}
				if (Time.frameCount != lastProcessedFrame)
				{
					lastProcessedFrame = Time.frameCount;
					originalDamageMultiplier = damageMultiplier;
				}
				else
				{
					damageMultiplier = originalDamageMultiplier;
				}
				IThingHolder parentHolder = weapon.ParentHolder;
				Pawn_EquipmentTracker val = (Pawn_EquipmentTracker)(object)((parentHolder is Pawn_EquipmentTracker) ? parentHolder : null);
				if (val == null)
				{
					return;
				}
				Pawn pawn = val.pawn;
				if (pawn == null || ((Thing)pawn).Destroyed)
				{
					return;
				}
				float num = 0f;
				float num2 = 0f;
				Pawn_ApparelTracker apparel = pawn.apparel;
				if (((apparel != null) ? apparel.WornApparel : null) != null)
				{
					foreach (Apparel item in pawn.apparel.WornApparel)
					{
						if (item != null)
						{
							num += StatExtension.GetStatValue((Thing)(object)item, DamageStat, true, -1);
							num2 += StatExtension.GetStatValue((Thing)(object)item, FinalDamageStat, true, -1);
						}
					}
				}
				if (pawn.story?.traits?.allTraits != null)
				{
					foreach (Trait allTrait in pawn.story.traits.allTraits)
					{
						if (((allTrait == null) ? null : allTrait.CurrentData?.statOffsets) != null)
						{
							num += StatUtility.GetStatOffsetFromList(allTrait.CurrentData.statOffsets, DamageStat);
							num2 += StatUtility.GetStatOffsetFromList(allTrait.CurrentData.statOffsets, FinalDamageStat);
						}
					}
				}
				if (pawn.health?.hediffSet?.hediffs != null)
				{
					foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
					{
						if (((hediff == null) ? null : hediff.CurStage?.statOffsets) != null)
						{
							num += StatUtility.GetStatOffsetFromList(hediff.CurStage.statOffsets, DamageStat);
							num2 += StatUtility.GetStatOffsetFromList(hediff.CurStage.statOffsets, FinalDamageStat);
						}
					}
				}
				damageMultiplier *= 1f + num;
				damageMultiplier *= 1f + num2;
				Log.Message("[DamageCalc] Pawn: " + ((Entity)pawn).LabelShort + " | " + $"Base Multiplier: {originalDamageMultiplier} | " + $"Damage Bonus: {num * 100f}% | " + $"Final Multiplier Bonus: {num2 * 100f}% | " + $"Final Multiplier: {damageMultiplier}");
			}
			catch (Exception arg)
			{
				Log.Error($"Error in TowerRangedWeaponMultiplier: {arg}");
			}
		}

		[HarmonyPostfix]
		private static void Postfix(int __result, float damageMultiplier, Thing weapon, StringBuilder explanation)
		{
			try
			{
				if (explanation == null || weapon == null)
				{
					return;
				}
				string text = explanation.ToString();
				if (text.Contains("攻击力加成：") || text.Contains("最终伤害乘数："))
				{
					return;
				}
				IThingHolder parentHolder = weapon.ParentHolder;
				Pawn_EquipmentTracker val = (Pawn_EquipmentTracker)(object)((parentHolder is Pawn_EquipmentTracker) ? parentHolder : null);
				if (val == null)
				{
					return;
				}
				Pawn pawn = val.pawn;
				if (pawn == null || ((Thing)pawn).Destroyed)
				{
					return;
				}
				float num = 0f;
				float num2 = 0f;
				Pawn_ApparelTracker apparel = pawn.apparel;
				if (((apparel != null) ? apparel.WornApparel : null) != null)
				{
					foreach (Apparel item in pawn.apparel.WornApparel)
					{
						if (item != null)
						{
							num += StatExtension.GetStatValue((Thing)(object)item, DamageStat, true, -1);
							num2 += StatExtension.GetStatValue((Thing)(object)item, FinalDamageStat, true, -1);
						}
					}
				}
				if (pawn.story?.traits?.allTraits != null)
				{
					foreach (Trait allTrait in pawn.story.traits.allTraits)
					{
						if (((allTrait == null) ? null : allTrait.CurrentData?.statOffsets) != null)
						{
							num += StatUtility.GetStatOffsetFromList(allTrait.CurrentData.statOffsets, DamageStat);
							num2 += StatUtility.GetStatOffsetFromList(allTrait.CurrentData.statOffsets, FinalDamageStat);
						}
					}
				}
				if (pawn.health?.hediffSet?.hediffs != null)
				{
					foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
					{
						if (((hediff == null) ? null : hediff.CurStage?.statOffsets) != null)
						{
							num += StatUtility.GetStatOffsetFromList(hediff.CurStage.statOffsets, DamageStat);
							num2 += StatUtility.GetStatOffsetFromList(hediff.CurStage.statOffsets, FinalDamageStat);
						}
					}
				}
				if (explanation.Length > 0 && explanation[explanation.Length - 1] != '\n')
				{
					explanation.AppendLine("\n");
				}
				string text2 = ((Def)(DamageStat?)).label ?? "攻击力加成";
				string text3 = ((Def)(FinalDamageStat?)).label ?? "最终伤害乘数";
				string value = "  " + text2 + "：" + GenText.ToStringPercent(num);
				string value2 = "  " + text3 + "：" + GenText.ToStringPercent(1f + num2);
				explanation.AppendLine(value);
				explanation.AppendLine(value2);
			}
			catch (Exception arg)
			{
				Log.Error($"Error in TowerRangedWeaponMultiplier Postfix: {arg}");
			}
		}
	}

	private static StatDef SafeGetStatDef(string defName)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return (StatDef)(((object)DefDatabase<StatDef>.GetNamedSilentFail(defName)) ?? ((object)new StatDef
		{
			defName = defName,
			category = StatCategoryDefOf.Weapon
		}));
	}
}
