using BANWlLib.Effects.AssetBundles;
using UnityEngine;
using Verse;

namespace BANWlLib.Effects
{
    // AB 年龄驱动 Mote Graphic，负责保持原 Graphic_MoteWithAgeSecs 播放语义并从 AssetBundle 取得贴图。
    public class Graphic_BAAbMoteWithAgeSecs : Graphic_MoteWithAgeSecs
    {
        private Shader cachedShader;

        // 初始化 Graphic 基础字段，负责避免 Graphic_Single 继续通过 ContentFinder 加载 PNG。
        public override void Init(GraphicRequest req)
        {
            InitFields(req);
            mat = BaseContent.BadMat;
        }

        // 单材质访问，负责覆盖直接读取 MatSingle 的绘制路径。
        public override Material MatSingle
        {
            get
            {
                mat = BAEffectBundleRegistry.AcquireMaterial(CreateRequest(), GetType().Name);
                return mat;
            }
        }

        // 绘制 Mote，负责在每次绘制前刷新 AB 材质并保留年龄驱动 shader 语义。
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            mat = BAEffectBundleRegistry.AcquireMaterial(CreateRequest(), GetType().Name);
            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        // 生成染色版本，负责维持 RimWorld 原有换色调用语义。
        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_BAAbMoteWithAgeSecs>(path, newShader, drawSize, newColor, newColorTwo, data);
        }

        // 初始化基础字段，负责保存 GraphicDatabase 传入的路径、颜色、尺寸和 shader 数据。
        private void InitFields(GraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            maskPath = req.maskPath;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            cachedShader = req.shader;
        }

        // 重建请求对象，负责让绘制阶段可以按当前 Graphic 状态重新获取材质。
        private GraphicRequest CreateRequest()
        {
            return new GraphicRequest(GetType(), path, cachedShader, drawSize, color, colorTwo, data, data?.renderQueue ?? 0, data?.shaderParameters, maskPath);
        }
    }
}
