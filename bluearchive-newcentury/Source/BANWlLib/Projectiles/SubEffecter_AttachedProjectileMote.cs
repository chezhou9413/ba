using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    // 投射物附着 Mote 子特效，负责把持续特效绑定到飞行子弹并按弹道角度旋转。
    public class SubEffecter_AttachedProjectileMote : SubEffecter
    {
        private Mote mote;

        // 初始化子特效，负责接收 EffecterDef 中配置的 Mote 参数。
        public SubEffecter_AttachedProjectileMote(SubEffecterDef def, Effecter parent)
            : base(def, parent)
        {
        }

        // 每 tick 维护子特效，负责在首次运行时生成 Mote，并持续同步到投射物位置。
        public override void SubEffectTick(TargetInfo A, TargetInfo B)
        {
            Projectile projectile = A.Thing as Projectile;
            if (projectile == null || projectile.Map == null || def.moteDef == null)
            {
                DestroyMote();
                return;
            }

            if (mote == null || mote.Destroyed)
            {
                SpawnMote(projectile);
            }

            if (mote == null || mote.Destroyed)
            {
                return;
            }

            ProjectileFlightEffectData data = ResolveFlightData(projectile);
            Vector3 offset = BuildOffset(projectile, data);
            Vector3 drawPosition = projectile.DrawPos + offset;
            mote.Maintain();
            mote.exactPosition = drawPosition;
            mote.exactRotation = ResolveRotation(projectile, data);
            mote.Position = drawPosition.ToIntVec3();
        }

        // 清理子特效，负责在投射物销毁时移除附着 Mote。
        public override void SubCleanup()
        {
            DestroyMote();
            base.SubCleanup();
        }

        // 生成 Mote，负责使用 XML 中的缩放、颜色和初始位置。
        private void SpawnMote(Projectile projectile)
        {
            ProjectileFlightEffectData data = ResolveFlightData(projectile);
            Vector3 drawPosition = projectile.DrawPos + BuildOffset(projectile, data);
            if (!drawPosition.ShouldSpawnMotesAt(projectile.Map, def.moteDef.drawOffscreen))
            {
                return;
            }

            mote = (Mote)ThingMaker.MakeThing(def.moteDef);
            GenSpawn.Spawn(mote, drawPosition.ToIntVec3(), projectile.Map);
            mote.Scale = def.scale.RandomInRange * (parent?.scale ?? 1f);
            mote.exactPosition = drawPosition;
            mote.exactRotation = ResolveRotation(projectile, data);
            mote.rotationRate = def.rotationRate.RandomInRange;
            mote.instanceColor = EffectiveColor;
            mote.yOffset = data.offsetUp + EffectiveOffset.y;
            mote.curvedScale = def.moteDef.mote.scalers?.ScaleAtTime(0f) ?? Vector3.one;
            mote.Maintain();
        }

        // 解析飞行特效参数，负责在没有上下文时提供 XML 子特效自身的默认偏移。
        private ProjectileFlightEffectData ResolveFlightData(Projectile projectile)
        {
            if (ProjectileFlightEffectContext.TryGet(projectile, out ProjectileFlightEffectData data))
            {
                return data;
            }

            return new ProjectileFlightEffectData
            {
                rotateWithProjectile = true,
                offsetForward = 0f,
                offsetRight = 0f,
                offsetUp = EffectiveOffset.y
            };
        }

        // 构建世界偏移，负责把弹道前后、左右和高度偏移转换到投射物当前位置。
        private Vector3 BuildOffset(Projectile projectile, ProjectileFlightEffectData data)
        {
            Vector3 forward = projectile.ExactRotation * Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            return forward * data.offsetForward
                + right * data.offsetRight
                + Vector3.up * data.offsetUp
                + new Vector3(EffectiveOffset.x, 0f, EffectiveOffset.z);
        }

        // 解析 Mote 旋转角，负责按配置决定是否跟随投射物弹道方向。
        private float ResolveRotation(Projectile projectile, ProjectileFlightEffectData data)
        {
            float baseRotation = data.rotateWithProjectile ? projectile.ExactRotation.eulerAngles.y : 0f;
            return baseRotation + def.rotation.RandomInRange;
        }

        // 销毁当前 Mote，负责处理已存在和已销毁两种状态。
        private void DestroyMote()
        {
            if (mote != null && !mote.Destroyed)
            {
                mote.Destroy();
            }

            mote = null;
        }
    }
}
