using BANWlLib.BattleSystem;
using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //普通投射物存档补丁，负责保存发射时的战斗配置和来源身份。
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.ExposeData))]
    public static class ProjectileBattleSavePatch
    {
        //将静态命中上下文随具体弹丸保存并在读档时恢复。
        public static void Postfix(Projectile __instance)
        {
            ProjectileBattleContext.TryGet(__instance, out ProjectileBattleData data);
            Scribe_Deep.Look(ref data, "banwBattleData");
            if (data != null) ProjectileBattleContext.Register(__instance, data);
        }
    }
}
