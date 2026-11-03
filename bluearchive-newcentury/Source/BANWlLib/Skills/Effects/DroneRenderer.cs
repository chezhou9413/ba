using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //无人机四向图形与实体弹丸共用位置，材质和网格由原版Graphic缓存。
    public static class DroneRenderer
    {
        //四向偏移只在这里应用一次，保留高度偏移和轻微悬停。
        public static Vector3 Position(Hediff_SpecialSkillState s)
        {
            Vector3 pos = s.pawn.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            pos += s.profile.shiroko.droneGraphic.DrawOffsetForRot(s.pawn.Rotation);
            pos.z += Mathf.Sin(s.Now * 0.05f) * 0.06f;
            return pos;
        }

        //使用四向材质及镜像网格，避免Graphic.Draw重复叠加偏移。
        public static void Draw(Hediff_SpecialSkillState s)
        {
            Graphic graphic = s.profile.shiroko.droneGraphic.Graphic;
            Rot4 rotation = s.pawn.Rotation;
            Graphics.DrawMesh(graphic.MeshAt(rotation), Position(s), Quaternion.identity, graphic.MatAt(rotation), 0);
        }
    }
}

