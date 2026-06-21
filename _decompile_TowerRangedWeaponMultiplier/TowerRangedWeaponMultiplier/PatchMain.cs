using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace TowerRangedWeaponMultiplier;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class PatchMain
{
	static PatchMain()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		new Harmony("TowerRangedWeaponMultiplier_HarmonyPatch").PatchAll(Assembly.GetExecutingAssembly());
	}
}
