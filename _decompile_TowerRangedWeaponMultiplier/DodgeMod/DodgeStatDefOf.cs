using RimWorld;
using Verse;

namespace DodgeMod;

[DefOf]
public static class DodgeStatDefOf
{
	public static StatDef BANW_Miss;

	private static StatCategoryDef GetCombatCategory()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		StatCategoryDef namedSilentFail = DefDatabase<StatCategoryDef>.GetNamedSilentFail("Combat");
		if (namedSilentFail != null)
		{
			return namedSilentFail;
		}
		StatCategoryDef namedSilentFail2 = DefDatabase<StatCategoryDef>.GetNamedSilentFail("Basics");
		if (namedSilentFail2 != null)
		{
			return namedSilentFail2;
		}
		return new StatCategoryDef
		{
			defName = "Combat",
			label = "Combat"
		};
	}

	static DodgeStatDefOf()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		if (DefDatabase<StatDef>.GetNamedSilentFail("DodgeChance") == null)
		{
			BANW_Miss = new StatDef
			{
				defName = "DodgeChance",
				label = "Dodge Chance",
				description = "Chance to dodge incoming attacks",
				category = GetCombatCategory(),
				defaultBaseValue = 0f,
				toStringStyle = (ToStringStyle)8,
				showOnHumanlikes = true,
				showOnNonWorkTables = false,
				hideAtValue = 0f
			};
			DefDatabase<StatDef>.Add(BANW_Miss);
		}
		else
		{
			BANW_Miss = DefDatabase<StatDef>.GetNamed("DodgeChance", true);
		}
		DefOfHelper.EnsureInitializedInCtor(typeof(DodgeStatDefOf));
	}
}
