using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //防御 EX 位移期间的盾面跟随载体，并同步移动方向。
    public class SubEffecter_HoshinoShieldMote : SubEffecter
    {
        private Mote mote;
        private int startTick = -1;
        private float rotationOffset;

        //绑定XML中的粒子、偏移与轴向校正。
        public SubEffecter_HoshinoShieldMote(SubEffecterDef def, Effecter parent) : base(def, parent) { }

        //触发时记录开始时间，延迟结束后才播放。
        public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
        {
            startTick = Find.TickManager.TicksGame;
            SubEffectTick(A, B);
        }

        //每帧对齐同一个粒子，播完后重播；不积累重叠的护盾实例。
        public override void SubEffectTick(TargetInfo A, TargetInfo B)
        {
            Thing source = A.Thing;
            if (source == null || !source.Spawned || startTick < 0 ||
                Find.TickManager.TicksGame < startTick + def.initialDelayTicks) return;
            float angle = source is HoshinoMovementFlyer flyer
                ? (flyer.DestinationPos - flyer.DrawPos).Yto0().AngleFlat()
                : SpecialDirectionalEffects.AimAngle((Pawn)source, LocalTargetInfo.Invalid);
            Vector3 offset = def.absoluteAngle ? def.positionOffset : def.positionOffset.RotatedBy(angle);
            Vector3 position = source.DrawPos + offset;
            if (mote == null || mote.Destroyed)
            {
                mote = (Mote)ThingMaker.MakeThing(def.moteDef);
                GenSpawn.Spawn(mote, source.Position, source.Map);
                mote.Scale = def.scale.RandomInRange;
                rotationOffset = def.rotation.RandomInRange;
            }
            mote.exactPosition = position;
            mote.yOffset = offset.y;
            mote.exactRotation = angle + rotationOffset;
            mote.Maintain();
        }

        //位移结束或载体销毁时立即清理盾面。
        public override void SubCleanup()
        {
            if (mote != null && !mote.Destroyed) mote.Destroy();
            mote = null;
        }
    }
}
