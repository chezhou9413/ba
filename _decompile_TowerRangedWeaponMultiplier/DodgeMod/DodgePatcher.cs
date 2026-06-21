using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace DodgeMod;

[StaticConstructorOnStartup]
public static class DodgePatcher
{
	static DodgePatcher()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			new Harmony("com.BANW.DodgeMod").PatchAll(Assembly.GetExecutingAssembly());
		}
		catch (Exception arg)
		{
			Log.Error($"Failed to initialize DodgeMod: {arg}");
		}
	}
}
