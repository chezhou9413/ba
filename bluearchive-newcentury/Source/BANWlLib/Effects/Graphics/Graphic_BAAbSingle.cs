using BANWlLib.Effects.AssetBundles;
using UnityEngine;
using Verse;

namespace BANWlLib.Effects
{
    // AB 单图 Graphic，负责让静态特效贴图从 AssetBundle 加载并沿用原 Graphic_Single 绘制。
    public class Graphic_BAAbSingle : Graphic_BAAbBase
    {
        // 初始化 Graphic 基础字段，负责避免 Graphic_Single 继续通过 ContentFinder 加载 PNG。
        public override void Init(GraphicRequest req)
        {
            InitAbFields(req);
            mat = BaseContent.BadMat;
        }

        // 单材质访问，负责覆盖直接读取 MatSingle 的绘制路径。
        public override Material MatSingle
        {
            get
            {
                return AcquireAbMaterial();
            }
        }

        // 北向材质访问，负责让方向绘制路径也使用 AB 材质。
        public override Material MatNorth
        {
            get
            {
                return AcquireAbMaterial();
            }
        }

        // 东向材质访问，负责让方向绘制路径也使用 AB 材质。
        public override Material MatEast
        {
            get
            {
                return AcquireAbMaterial();
            }
        }

        // 南向材质访问，负责让方向绘制路径也使用 AB 材质。
        public override Material MatSouth
        {
            get
            {
                return AcquireAbMaterial();
            }
        }

        // 西向材质访问，负责让方向绘制路径也使用 AB 材质。
        public override Material MatWest
        {
            get
            {
                return AcquireAbMaterial();
            }
        }

        // 按方向取材质，负责在缓存过期后重新从 AB 拉起材质。
        public override Material MatAt(Rot4 rot, Thing thing)
        {
            if (thing == null && Current.ProgramState != ProgramState.Playing)
            {
                return BaseContent.BadMat;
            }

            return AcquireAbMaterial();
        }

        // 绘制对象，负责让 Mote 使用原版 Mote 矩阵绘制路径并读取 exactRotation。
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            if (thing is Mote)
            {
                Graphic_Mote.DrawMote(data, AcquireAbMaterial(), color, loc, rot, thingDef, thing, 0, false, null);
                return;
            }

            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        // 生成染色版本，负责维持 RimWorld 原有换色调用语义。
        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_BAAbSingle>(path, newShader, drawSize, newColor, newColorTwo, data);
        }
    }
}
