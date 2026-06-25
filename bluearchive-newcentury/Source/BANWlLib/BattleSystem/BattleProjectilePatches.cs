using HarmonyLib;
using BANWlLib.Projectiles;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    [HarmonyPatch(typeof(Projectile), "get_DamageAmount")]
    public static class BattleProjectileDamagePatch
    {
        public static void Postfix(Projectile __instance, ref int __result)
        {
            if (__instance is Projectile_PiercingArea || !(__instance?.Launcher is Pawn pawn))
            {
                return;
            }

            __result = Mathf.Max(0, Mathf.RoundToInt(BattleStatUtility.ScaleWeaponDamageBase(pawn, __result)));
        }
    }

    // 子弹命中前注册施法者快照，负责让子弹附加的治疗 Hediff 能使用施法者的治疗力加成和 EX 倍率。
    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class ProjectileImpactHealContextPatch
    {
        // 在原版 Impact 执行前注册施法者快照，负责在 DamageDef.additionalHediffs 附加 Hediff 时让 HealProjectileContext 有数据可读。
        public static void Prefix(Projectile __instance, Thing hitThing)
        {
            if (hitThing is Pawn targetPawn && __instance?.Launcher is Pawn casterPawn)
            {
                BattleCasterSnapshot snapshot = BattleStatUtility.CreateSnapshot(casterPawn);
                if (snapshot != null)
                {
                    HealProjectileContext.Register(targetPawn, snapshot);
                }
            }
        }
    }
}
