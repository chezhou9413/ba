using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace BANWlLib.Skills
{
    //世界目标EX复制补丁，负责在跨地图技能成功施放后消费一次机会。
    [HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(GlobalTargetInfo))]
    public static class RioCopyGlobalActivatedPatch
    {
        //成功才消费，拒绝施法或取消世界目标选择不影响机会。
        public static void Postfix(Ability __instance, bool __result)
        {
            if (__result) RioSkills.Consume(__instance);
        }
    }
}
