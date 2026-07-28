using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    // 方向命中特效工具，负责让普通投射物和穿透投射物共享目标过滤与 Effecter 触发流程。
    public static class DirectionalImpactEffectUtility
    {
        // 尝试触发投射物的方向命中特效，负责读取扩展、校验目标类型并传递弹道方向。
        public static void TryTrigger(Projectile projectile, Thing target, Vector3 direction)
        {
            PiercingProjectileExtension extension = projectile?.def?.GetModExtension<PiercingProjectileExtension>();
            Map map = projectile?.Map;
            if (map == null || target == null || extension?.directionalImpactEffect == null)
            {
                return;
            }

            if (target is Pawn && !extension.directionalImpactOnPawn)
            {
                return;
            }

            if (target is Building && !extension.directionalImpactOnBuilding)
            {
                return;
            }

            DirectionalImpactEffectContext.Register(
                target,
                direction,
                extension.directionalImpactSpeed,
                extension.directionalImpactOffsetForward,
                extension.directionalImpactOffsetUp);

            try
            {
                Effecter effecter = extension.directionalImpactEffect.Spawn();
                effecter.Trigger(new TargetInfo(projectile.Position, map), new TargetInfo(target));
                effecter.Cleanup();
            }
            finally
            {
                DirectionalImpactEffectContext.Clear(target);
            }
        }

        // 获取普通投射物的飞行方向，负责把投射物旋转转换为水平单位向量。
        public static Vector3 GetTravelDirection(Projectile projectile)
        {
            if (projectile == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = (projectile.ExactRotation * Vector3.forward).Yto0();
            if (direction.sqrMagnitude < 0.0001f)
            {
                return Vector3.forward;
            }

            direction.Normalize();
            return direction;
        }
    }
}
