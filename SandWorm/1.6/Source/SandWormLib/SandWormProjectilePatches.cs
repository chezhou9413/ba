using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Launch), new[]
    {
        typeof(Thing),
        typeof(Vector3),
        typeof(LocalTargetInfo),
        typeof(LocalTargetInfo),
        typeof(ProjectileHitFlags),
        typeof(bool),
        typeof(Thing),
        typeof(ThingDef)
    })]
    public static class SandWormProjectileLaunchPatch
    {
        public static void Prefix(
            Thing launcher,
            Vector3 origin,
            ref LocalTargetInfo usedTarget,
            ref LocalTargetInfo intendedTarget,
            ref ProjectileHitFlags hitFlags)
        {
            SandWormThing worm = GetTargetedSandWorm(usedTarget) ?? GetTargetedSandWorm(intendedTarget);
            if (worm == null || worm.Destroyed || !worm.Spawned || worm.Map == null)
            {
                return;
            }

            IntVec3 targetCell = usedTarget.Cell.IsValid ? usedTarget.Cell : worm.Position;
            if (worm.TryGetBestHitProxyForShot(origin, targetCell, out SandWormHitProxyThing proxy))
            {
                usedTarget = proxy;
                intendedTarget = proxy;
                hitFlags |= ProjectileHitFlags.IntendedTarget;
            }
            else
            {
                usedTarget = worm;
                intendedTarget = worm;
                hitFlags |= ProjectileHitFlags.IntendedTarget;
            }
        }

        private static SandWormThing GetTargetedSandWorm(LocalTargetInfo target)
        {
            if (!target.IsValid || !target.HasThing)
            {
                return null;
            }

            if (target.Thing is SandWormThing worm)
            {
                return worm;
            }

            if (target.Thing is SandWormHitProxyThing proxy)
            {
                return proxy.Owner;
            }

            return null;
        }
    }
}
