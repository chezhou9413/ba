using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    // 方向命中 Mote 子特效，负责让命中特效沿投射物飞行方向从目标身体位置飞出。
    public class SubEffecter_DirectionalImpactMote : SubEffecter
    {
        // 初始化子特效，负责绑定 EffecterDef 中配置的 moteDef。
        public SubEffecter_DirectionalImpactMote(SubEffecterDef def, Effecter parent)
            : base(def, parent)
        {
        }

        // 触发子特效，负责在命中目标中心生成 MoteThrown 并设置弹道速度。
        public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
        {
            Thing target = B.Thing ?? A.Thing;
            Map map = B.Map ?? A.Map;
            if (target == null || map == null || def.moteDef == null)
            {
                return;
            }

            DirectionalImpactEffectData data;
            if (!DirectionalImpactEffectContext.TryGet(target, out data))
            {
                return;
            }

            Vector3 spawnPosition = target.DrawPos + data.direction * data.offsetForward + Vector3.up * data.offsetUp;
            if (!spawnPosition.ShouldSpawnMotesAt(map, false))
            {
                return;
            }

            Mote mote = (Mote)ThingMaker.MakeThing(def.moteDef);
            mote.Scale = def.scale.RandomInRange;
            mote.exactPosition = spawnPosition;
            mote.exactRotation = data.direction.AngleFlat();

            MoteThrown thrown = mote as MoteThrown;
            if (thrown != null)
            {
                thrown.rotationRate = def.rotationRate.RandomInRange;
                thrown.SetVelocity(data.direction.AngleFlat(), data.speed);
            }

            GenSpawn.Spawn(mote, spawnPosition.ToIntVec3(), map);
        }
    }
}
