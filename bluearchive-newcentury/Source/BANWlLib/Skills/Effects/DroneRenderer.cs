using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //无人机表现，负责跟随宿主绘制可配置贴图或简易测试机体。
    [StaticConstructorOnStartup]
    public static class DroneRenderer
    {
        private static readonly Material Body = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.18f, 0.3f, 0.42f));
        private static readonly Material Rotor = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.85f, 1f));
        private static readonly Dictionary<SpecialSkillProfileDef, Material> Textures = new Dictionary<SpecialSkillProfileDef, Material>();

        //根据宿主位置、偏移和轻微悬停摆动计算实体弹丸发射点。
        public static Vector3 Position(Hediff_SpecialSkillState s)
        {
            Vector3 pos = s.pawn.DrawPos + s.profile.droneOffset;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            pos.z += Mathf.Sin(s.Now * 0.05f) * 0.06f;
            return pos;
        }

        //有显式贴图则使用贴图，否则绘制独立配置的测试机体。
        public static void Draw(Hediff_SpecialSkillState s)
        {
            Vector3 pos = Position(s);
            Vector2 size = s.profile.droneDrawSize;
            if (!s.profile.droneTexturePath.NullOrEmpty())
            {
                if (!Textures.TryGetValue(s.profile, out Material material))
                {
                    material = MaterialPool.MatFrom(s.profile.droneTexturePath, ShaderDatabase.Cutout);
                    Textures.Add(s.profile, material);
                }
                Plane(pos, size.x, size.y, material);
                return;
            }
            Plane(pos, size.x * 0.5f, size.y * 0.6f, Body);
            foreach (float x in new[] { -0.4f, 0.4f })
                foreach (float z in new[] { -0.4f, 0.4f })
                    Plane(pos + new Vector3(x * size.x, 0.01f, z * size.y), size.x * 0.3f, size.y * 0.3f, Rotor);
        }

        //绘制一块平面机体部件。
        private static void Plane(Vector3 position, float width, float height, Material material)
        {
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(position, Quaternion.identity, new Vector3(width, 1f, height)), material, 0);
        }
    }
}

