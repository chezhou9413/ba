using UnityEngine;
using Verse;

namespace BANWlLib.BaClass
{
    // 待生成旋转偏移 Mote，负责保存单个延迟特效的生成时间、位置和视觉参数。
    public class PendingRotatedOffsetMote : IExposable
    {
        public int fireAtTick;
        public ThingDef moteDef;
        public Vector3 position;
        public float scale;
        public float rotationAngle;
        public bool hasRotation;

        // 生成延迟 Mote，负责把保存的视觉参数交给旋转偏移喷射特效的统一生成入口。
        public void Spawn(Map map)
        {
            SubEffecter_SprayerTriggeredRotatedOffset.SpawnMote(
                moteDef,
                position,
                map,
                scale,
                hasRotation ? rotationAngle : 0f);
        }

        // 保存和读取延迟 Mote，负责支持延迟期间存读档。
        public void ExposeData()
        {
            Scribe_Values.Look(ref fireAtTick, "fireAtTick", 0);
            Scribe_Defs.Look(ref moteDef, "moteDef");
            Scribe_Values.Look(ref position, "position");
            Scribe_Values.Look(ref scale, "scale", 1f);
            Scribe_Values.Look(ref rotationAngle, "rotationAngle", 0f);
            Scribe_Values.Look(ref hasRotation, "hasRotation", false);
        }
    }
}
