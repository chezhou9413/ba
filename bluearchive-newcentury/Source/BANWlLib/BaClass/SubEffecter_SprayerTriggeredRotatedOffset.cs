using UnityEngine;
using Verse;

namespace BANWlLib.BaClass
{
    // 触发式旋转偏移喷射特效，负责按施法者朝向或目标方向旋转位置偏移后生成 Mote。
    public class SubEffecter_SprayerTriggeredRotatedOffset : SubEffecter_Sprayer
    {
        // 创建旋转偏移喷射子特效，负责绑定 SubEffecterDef 和父 Effecter。
        public SubEffecter_SprayerTriggeredRotatedOffset(SubEffecterDef def, Effecter parent)
            : base(def, parent)
        {
        }

        // 触发子特效，负责根据 Pawn 到目标点的方向旋转配置偏移，并按 initialDelayTicks 决定立即生成或延迟生成。
        public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
        {
            if (def.moteDef == null)
            {
                Log.Error("[BANW] 旋转偏移喷射特效缺少 moteDef。");
                return;
            }

            Map map = A.Map ?? B.Map;
            if (map == null)
            {
                Log.Error("[BANW] 旋转偏移喷射特效缺少地图，无法生成 Mote。");
                return;
            }

            Vector3 rotatedOffset = def.positionOffset;
            float? rotationAngle = null;

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
                float angle = pawn.Rotation.AsAngle;
                if (B.IsValid)
                {
                    angle = (B.CenterVector3 - pawn.DrawPos).AngleFlat();
                }
                else
                {
                    Stance_Busy stance = pawn.stances?.curStance as Stance_Busy;
                    if (stance != null && stance.focusTarg.IsValid)
                    {
                        angle = (stance.focusTarg.CenterVector3 - pawn.DrawPos).AngleFlat();
                    }
                }
                rotatedOffset = def.positionOffset.RotatedBy(angle);
                rotationAngle = angle;
            }

            Vector3 pos = A.Cell.ToVector3Shifted() + rotatedOffset;
            if (def.initialDelayTicks > 0)
            {
                RotatedOffsetMoteDelayComponent.Queue(map, def.moteDef, pos, def.scale.RandomInRange, rotationAngle, def.initialDelayTicks);
                return;
            }

            SpawnMote(def.moteDef, pos, map, def.scale.RandomInRange, rotationAngle);
        }

        // 生成 Mote，负责应用缩放、位置和旋转角度。
        public static void SpawnMote(ThingDef moteDef, Vector3 pos, Map map, float scale, float? rotationAngle)
        {
            if (moteDef == null)
            {
                Log.Error("[BANW] 旋转偏移喷射特效生成任务缺少 moteDef。");
                return;
            }

            if (!pos.ShouldSpawnMotesAt(map, moteDef.drawOffscreen))
            {
                return;
            }

            Mote mote = (Mote)ThingMaker.MakeThing(moteDef);
            mote.Scale = scale;
            mote.exactPosition = pos;
            if (rotationAngle.HasValue)
            {
                mote.exactRotation = rotationAngle.Value;
            }

            GenSpawn.Spawn(mote, pos.ToIntVec3(), map);
        }
    }
}
