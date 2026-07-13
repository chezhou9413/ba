namespace SandWormLib
{
    // SandWormSmallThing 负责提供小型沙虫实体，复用基础沙虫移动和受击架构但禁用头部秒杀。
    public sealed class SandWormSmallThing : SandWormThing
    {
        private const string ToonOutlineShaderName = "Custom/ToonOutlineFixed";
        private const string BodyTextureName = "Sandworm_Body_baseColor";

        protected override float InitialSizeMultiplier => 0.5f;

        protected override int BaseMaxHitPoints => 20000;

        protected override string HitProxyDefName => "SandWorm_SmallHitProxy";

        internal override bool AllowsHeadInstantKill => ChallengeHeadInstantKillOverride;

        // ApplyInstanceMaterial 负责给小型沙虫复用的预制体实例替换独立运行时材质。
        protected override void ApplyInstanceMaterial(UnityEngine.Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            if (renderer is UnityEngine.ParticleSystemRenderer)
            {
                return;
            }

            UnityEngine.Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                UnityEngine.Material material = materials[i];
                if (!ShouldApplySmallWormMaterial(material))
                {
                    continue;
                }

                ApplySmallWormMaterialProperties(material);
            }
        }

        // ShouldApplySmallWormMaterial 负责只筛选沙虫身体材质，避免粒子和辅助面被描边 shader 污染。
        private static bool ShouldApplySmallWormMaterial(UnityEngine.Material material)
        {
            if (material == null)
            {
                return false;
            }

            if (!material.HasProperty("_MainTex"))
            {
                return false;
            }

            UnityEngine.Texture texture = material.GetTexture("_MainTex");
            return texture != null && texture.name == BodyTextureName;
        }

        // ApplySmallWormMaterialProperties 负责写入小型沙虫描边材质参数，材质实例不会污染大沙虫。
        private static void ApplySmallWormMaterialProperties(UnityEngine.Material material)
        {
            UnityEngine.Shader shader = UnityEngine.Shader.Find(ToonOutlineShaderName);
            if (shader != null)
            {
                material.shader = shader;
            }

            SetColor(material, "_Color", new UnityEngine.Color(1f, 0.919f, 0.714f, 1f));
            SetColor(material, "_LitTint", UnityEngine.Color.white);
            SetColor(material, "_ShadowTint", UnityEngine.Color.white);
            SetColor(material, "_OutlineColor", new UnityEngine.Color(0.05f, 0.04f, 0.04f, 1f));
            SetFloat(material, "_StepThreshold", 0.536f);
            SetFloat(material, "_StepAmount", 0.0516f);
            SetFloat(material, "_OutlineWidth", 0.2265f);
            SetVector(material, "_LightDir", new UnityEngine.Vector4(0.72f, 1.93f, 3.83f, 2.30f));
        }

        // SetColor 负责在材质存在指定颜色属性时写入颜色值。
        private static void SetColor(UnityEngine.Material material, string propertyName, UnityEngine.Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        // SetFloat 负责在材质存在指定浮点属性时写入浮点值。
        private static void SetFloat(UnityEngine.Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        // SetVector 负责在材质存在指定向量属性时写入向量值。
        private static void SetVector(UnityEngine.Material material, string propertyName, UnityEngine.Vector4 value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetVector(propertyName, value);
            }
        }
    }
}
