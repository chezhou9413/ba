using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    //方向命中 Mote 子特效，让命中特效沿投射物飞行方向从目标身体位置飞出。
    public class SubEffecter_DirectionalImpactMote : SubEffecter
    {
        //绑定 EffecterDef 中配置的 moteDef。
        public SubEffecter_DirectionalImpactMote(SubEffecterDef def, Effecter parent)
            : base(def, parent)
        {
        }

        //在命中目标中心生成 MoteThrown 并设置弹道速度。
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

            Vector3 spawnPosition = target.DrawPos + def.positionOffset +
                data.direction * data.offsetForward + Vector3.up * data.offsetUp;
            DirectionalImpactMoteDelayComponent.SpawnOrQueue(map, spawnPosition, data, def, overrideSpawnTick);
        }
    }
}
