using BANWlLib.Effects.AssetBundles;
using UnityEngine;
using Verse;

namespace BANWlLib.Effects
{
    // AB Graphic 基础逻辑，负责保存 GraphicRequest 的原始字段并避免通过 Shader 属性递归读取材质。
    public abstract class Graphic_BAAbBase : Graphic_Single
    {
        private Shader cachedShader;

        // 初始化基础字段，负责保存 GraphicDatabase 传入的路径、颜色、尺寸和 shader 数据。
        protected void InitAbFields(GraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            maskPath = req.maskPath;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            cachedShader = req.shader;
        }

        // 获取 AB 材质，负责在绘制阶段按需加载贴图并复用材质缓存。
        protected Material AcquireAbMaterial()
        {
            mat = BAEffectBundleRegistry.AcquireMaterial(CreateRequest(), GetType().Name);
            return mat;
        }

        // 重建请求对象，负责让绘制阶段可以按当前 Graphic 状态重新获取材质。
        protected GraphicRequest CreateRequest()
        {
            return new GraphicRequest(GetType(), path, cachedShader, drawSize, color, colorTwo, data, data?.renderQueue ?? 0, data?.shaderParameters, maskPath);
        }
    }
}
