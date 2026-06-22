using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BANWlLib.BaClass
{
    public class SubEffecter_SprayerTriggeredRotatedOffset : SubEffecter_Sprayer
    {
        public SubEffecter_SprayerTriggeredRotatedOffset(SubEffecterDef def, Effecter parent)
            : base(def, parent)
        {
        }

        public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
        {
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
                float angle = pawn.Rotation.AsAngle; // Default to 4-direction rotation
                var stance = pawn.stances?.curStance as Stance_Busy;
                if (stance != null && stance.focusTarg.IsValid)
                {
                    angle = (stance.focusTarg.CenterVector3 - pawn.DrawPos).AngleFlat();
                }
                rotatedOffset = def.positionOffset.RotatedBy(angle);
                rotationAngle = angle;
            }

            Vector3 pos = A.Cell.ToVector3Shifted() + rotatedOffset;
            MakeMote(pos, A.Map, rotationAngle);
        }

        private void MakeMote(Vector3 pos, Map map, float? rotationAngle)
        {
            if (!pos.ShouldSpawnMotesAt(map, false))
            {
                return;
            }
            Mote mote = (Mote)ThingMaker.MakeThing(def.moteDef);
            mote.Scale = def.scale.RandomInRange;
            mote.exactPosition = pos;
            if (rotationAngle.HasValue)
            {
                mote.exactRotation = rotationAngle.Value;
            }
            GenSpawn.Spawn(mote, pos.ToIntVec3(), map);
        }
    }
}
