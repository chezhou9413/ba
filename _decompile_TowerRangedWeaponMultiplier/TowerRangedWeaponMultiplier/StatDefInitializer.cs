using System;
using RimWorld;
using Verse;

namespace TowerRangedWeaponMultiplier;

[StaticConstructorOnStartup]
public static class StatDefInitializer
{
	static StatDefInitializer()
	{
		EnsureStatDefExists("BANW_RangedWeapon_Damage", () => new StatDef
		{
			defName = "BANW_RangedWeapon_Damage",
			label = "基础攻击力加成",
			description = "优先计算的基础加成",
			category = StatCategoryDefOf.PawnCombat,
			defaultBaseValue = 0f,
			alwaysHide = false,
			minValue = 0f,
			toStringStyle = (ToStringStyle)2,
			displayPriorityInCategory = 5499,
			scenarioRandomizable = true
		});
		EnsureStatDefExists("BANW_FinalDamageMultiplier", () => new StatDef
		{
			defName = "BANW_FinalDamageMultiplier",
			label = "攻击力加成",
			description = "最终伤害的加成",
			category = StatCategoryDefOf.PawnCombat,
			defaultBaseValue = 0f,
			alwaysHide = false,
			minValue = 0f,
			toStringStyle = (ToStringStyle)8,
			displayPriorityInCategory = 5500,
			scenarioRandomizable = true
		});
	}

	private static void EnsureStatDefExists(string defName, Func<StatDef> creator)
	{
		if (DefDatabase<StatDef>.GetNamedSilentFail(defName) == null)
		{
			DefDatabase<StatDef>.Add(creator());
			Log.Message("动态创建缺失的 StatDef: " + defName);
		}
	}
}
