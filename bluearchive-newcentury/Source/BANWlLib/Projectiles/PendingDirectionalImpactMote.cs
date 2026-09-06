using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    // 待播放方向命中 Mote，负责保存延迟生成所需的 Def、位置、方向和运动参数。
    public class PendingDirectionalImpactMote : IExposable
    {
        public int fireAtTick;
        public ThingDef moteDef;
        public Vector3 spawnPosition;
        public Vector3 direction;
        public float speed;
        public float scale;
        public float rotationOffset;
        public float rotationRate;
        public int overrideSpawnTick = -1;

        // 生成方向命中 Mote，负责应用命中时记录的精确位置、朝向和飞行速度。
        public void Spawn(Map map)
        {
            if (map == null || moteDef == null || !spawnPosition.ShouldSpawnMotesAt(map, moteDef.drawOffscreen))
            {
                return;
            }

            Mote mote = (Mote)ThingMaker.MakeThing(moteDef);
            GenSpawn.Spawn(mote, spawnPosition.ToIntVec3(), map);
            mote.Scale = scale;
            mote.exactPosition = spawnPosition;
            //贴图朝向叠加自身轴向偏移，运动方向仍沿命中弹道。
            mote.exactRotation = direction.AngleFlat() + rotationOffset;
            mote.rotationRate = rotationRate;

            if (mote is MoteThrown thrown)
            {
                thrown.SetVelocity(direction.AngleFlat(), speed);
            }

            if (overrideSpawnTick >= 0)
            {
                mote.ForceSpawnTick(overrideSpawnTick);
            }
        }

        // 保存和读取待播放数据，负责支持延迟期间存读档。
        public void ExposeData()
        {
            Scribe_Values.Look(ref fireAtTick, "fireAtTick", 0);
            Scribe_Defs.Look(ref moteDef, "moteDef");
            Scribe_Values.Look(ref spawnPosition, "spawnPosition");
            Scribe_Values.Look(ref direction, "direction");
            Scribe_Values.Look(ref speed, "speed", 0f);
            Scribe_Values.Look(ref scale, "scale", 1f);
            Scribe_Values.Look(ref rotationOffset, "rotationOffset", 0f);
            Scribe_Values.Look(ref rotationRate, "rotationRate", 0f);
            Scribe_Values.Look(ref overrideSpawnTick, "overrideSpawnTick", -1);
        }
    }
}
