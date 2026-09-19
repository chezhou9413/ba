using BANWlLib.BattleSystem;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //原版子弹命中来源补丁，负责把发射时的普攻身份带入实际伤害事件。
    [HarmonyPatch(typeof(Bullet), "Impact", typeof(Thing), typeof(bool))]
    public static class SpecialBulletImpactPatch
    {
        //建立不改变原版伤害公式的来源范围。
        [HarmonyPriority(Priority.First)]
        public static void Prefix(Bullet __instance, Thing hitThing, out SpecialDamageScope __state)
        {
            __state = null;
            if (!ProjectileBattleContext.TryGet(__instance, out ProjectileBattleData data)) return;
            __state = SpecialDamageScope.Enter(new BattleDamageRequest
            {
                instigator = __instance.Launcher, target = hitThing,
                isNormalAttack = data.isNormalAttack, useBattleStats = data.useBattleStats,
                basePower = data.basePower, damagePrepared = false
            });
        }

        //原版或其他补丁出错时也释放弹丸来源范围。
        public static void Finalizer(SpecialDamageScope __state) { __state?.Dispose(); }
    }
}
