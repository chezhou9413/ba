using UnityEngine;
using Verse;

namespace BANWlLib.BaClass
{
    // 触发式旋转偏移喷射特效，负责按施法者朝向或目标方向旋转位置偏移后生成 Mote。
    public class SubEffecter_SprayerTriggeredRotatedOffset : SubEffecter_Sprayer
    {
        private int ticksLeft = -1;

        private int delayedOverrideSpawnTick = -1;

        private bool hasDelayedTrigger;

        // 创建旋转偏移喷射子特效，负责绑定 SubEffecterDef 和父 Effecter。
        public SubEffecter_SprayerTriggeredRotatedOffset(SubEffecterDef def, Effecter parent)
            : base(def, parent)
        {
        }

        // 触发子特效，负责按原版延迟子特效语义记录倒计时或立即生成。
        public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
        {
            if (def.initialDelayTicks > 0)
            {
                ticksLeft = def.initialDelayTicks;
                delayedOverrideSpawnTick = overrideSpawnTick;
                hasDelayedTrigger = true;
                return;
            }

            SpawnFromTargets(A, B, overrideSpawnTick);
        }

        // 维护子特效，负责在原版 warmup 生命周期内处理延迟生成。
        public override void SubEffectTick(TargetInfo A, TargetInfo B)
        {
            bool waiting = hasDelayedTrigger && ticksLeft > 0;
            if (waiting)
            {
                ticksLeft--;
            }

            if (waiting && ticksLeft <= 0)
            {
                hasDelayedTrigger = false;
                SpawnFromTargets(A, B, delayedOverrideSpawnTick);
            }

            base.SubEffectTick(A, B);
        }

        // 清理子特效，负责取消尚未生成的延迟特效。
        public override void SubCleanup()
        {
            hasDelayedTrigger = false;
            ticksLeft = -1;
            delayedOverrideSpawnTick = -1;
            base.SubCleanup();
        }

        // 按目标信息生成 Mote，负责根据 Pawn 到目标点的方向旋转配置偏移。
        private void SpawnFromTargets(TargetInfo A, TargetInfo B, int overrideSpawnTick)
        {
            Vector3 rotatedOffset = def.positionOffset;
            float rotationBase = def.rotation.RandomInRange;
            float targetAngle = 0f;
            bool shouldRotateByTarget = !def.absoluteAngle;

            Pawn pawn = null;
            if (A.HasThing && A.Thing is Pawn)
            {
                pawn = A.Thing as Pawn;
            }
            else if (B.HasThing && B.Thing is Pawn)
            {
                pawn = B.Thing as Pawn;
            }

            if (pawn != null)
            {
                targetAngle = pawn.Rotation.AsAngle;
                if (B.IsValid)
                {
                    targetAngle = (B.CenterVector3 - pawn.DrawPos).AngleFlat();
                }
                else
                {
                    Stance_Busy stance = pawn.stances?.curStance as Stance_Busy;
                    if (stance != null && stance.focusTarg.IsValid)
                    {
                        targetAngle = (stance.focusTarg.CenterVector3 - pawn.DrawPos).AngleFlat();
                    }
                }
            }

            if (shouldRotateByTarget)
            {
                rotatedOffset = def.positionOffset.RotatedBy(targetAngle);
            }

            float rotationAngle = rotationBase + (shouldRotateByTarget ? targetAngle : 0f);
            Vector3 pos = A.Cell.ToVector3Shifted() + rotatedOffset;
            SpawnMote(def.moteDef, pos, A.Map ?? B.Map, def.scale.RandomInRange, rotationAngle, overrideSpawnTick);
        }

        // 生成 Mote，负责应用缩放、位置和旋转角度。
        public static void SpawnMote(ThingDef moteDef, Vector3 pos, Map map, float scale, float rotationAngle, int overrideSpawnTick = -1)
        {
            if (moteDef == null)
            {
                Log.Error("[BANW] 旋转偏移喷射特效生成任务缺少 moteDef。");
                return;
            }

            if (map == null)
            {
                Log.Error("[BANW] 旋转偏移喷射特效缺少地图，无法生成 Mote。");
                return;
            }

            if (!pos.ShouldSpawnMotesAt(map, moteDef.drawOffscreen))
            {
                return;
            }

            Mote mote = (Mote)ThingMaker.MakeThing(moteDef);
            mote.Scale = scale;
            mote.exactPosition = pos;
            mote.exactRotation = rotationAngle;

            GenSpawn.Spawn(mote, pos.ToIntVec3(), map);
            if (overrideSpawnTick != -1)
            {
                mote.ForceSpawnTick(overrideSpawnTick);
            }
        }
    }
}
