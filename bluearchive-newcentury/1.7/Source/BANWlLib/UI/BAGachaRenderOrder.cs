using System.Collections.Generic;
using RimUnity;
using RimUnity.UI;

namespace BANWlLib.mainUI.Initialization;

//把抽卡画布排序落实到材质队列，保留原始背景光的叠加强度。
internal sealed class BAGachaRenderOrder : MonoBehaviour
{
    private readonly Dictionary<(Material source, int queue, bool unlitStreak), Material> materials = new();

    //排序与背景线条的无光照材质只配置到当前过场实例。
    internal void Initialize()
    {
        GetComponent<Canvas>().sortingOrder = 100;

        //源预制体的 Start 和 Loop 为 101，Shan 为 104，Loop 的星光为 105。
        BindParticles("Clip2/Red_Start", 3001);
        BindParticles("Clip2/Blue_Start", 3001);
        BindParticles("Clip2/Red_Loop", 3001);
        BindParticles("Clip2/Blue_Loop", 3001);
        BindParticles("Clip2/Red_Shan", 3004);
        BindParticles("Clip2/Blue_Shan", 3004);
        BindRevealParticles("Clip35/Red_Show");
        BindRevealParticles("Clip35/Blue_Shou");

        //底板保留 3000，纸张 107 和白幕 110 排在背景粒子之后。
        BindImage("Clip2/Card", 3007);
        BindImage("Clip2/Card/one", 3007);
        BindImage("Clip2/Card/tree", 3007);
        BindImage("Clip25", 3010);
        BindImage("Clip35/Clip45", 3010);

        //结算控件在卡面和粒子之后绘制，闪光仍覆盖整个页面。
        BindGraphics(transform.Find("Clip55/BackButtom"), 3007);
        BindGraphics(transform.Find("Clip55/TryAgainButtom"), 3007);
        BindGraphics(transform.Find("Clip55/poit"), 3007);
    }

    //落卡光框原为 101、卡面为 102，光框先绘制，避免直接加亮卡面。
    internal void ConfigureReveal(GameObject card)
    {
        foreach (ParticleSystemRenderer renderer in card.GetComponentsInChildren<ParticleSystemRenderer>(true))
            renderer.sharedMaterial = OrderedMaterial(renderer.sharedMaterial, 3001);
        BindGraphics(card.transform, 3002);
    }

    //作者将光柱设为 103、卡面设为 104、亮边和闪点设为 106，分别恢复前后层。
    internal void ConfigureResult(GameObject card)
    {
        BAUiParticles.Configure(card);
        Transform glowRoot = card.transform.Find("kapai_idle_cai_chai/root");
        foreach (ParticleSystemRenderer renderer in card.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            Transform node = renderer.transform;
            bool foreground = node.parent == glowRoot &&
                (node.name == "sanjiao" || node.name == "sanjiao01" || node.name == "sanjiao002" || node.name == "lingxing");
            renderer.sharedMaterial = OrderedMaterial(renderer.sharedMaterial, foreground ? 3006 : 3003);
        }
        BindGraphics(card.transform, 3004);
        //星级与 New 标签保留清晰轮廓，不被卡面亮边覆盖。
        BindGraphics(card.transform.Find("back/CharacteShow/StarShow"), 3007);
        BindGraphics(card.transform.Find("back/new"), 3007);
    }

    //同组图形沿用层级顺序，包含文字、遮罩和暂未显示的奖励内容。
    private void BindGraphics(Transform root, int queue)
    {
        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
            graphic.material = OrderedMaterial(graphic.material, queue);
    }

    //恢复外层 GakaAnimation 对嵌套粒子的排序覆盖，包含尚未激活的颜色分支。
    private void BindParticles(string path, int queue)
    {
        Transform group = transform.Find(path);
        Transform star = path.EndsWith("_Loop", System.StringComparison.Ordinal) ? group.Find("root/xing") : null;
        foreach (ParticleSystemRenderer renderer in group.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            int effectiveQueue = renderer.transform == star ? 3005 : queue;
            renderer.sharedMaterial = OrderedMaterial(renderer.sharedMaterial, effectiveQueue);
        }
    }

    //大光晕与紫色铺底在卡面后绘制，线条和三角碎片保留前景层。
    private void BindRevealParticles(string path)
    {
        Transform group = transform.Find(path);
        Transform glow = group.Find("root/glow");
        Transform streak = group.Find("root/xian");
        Transform washOne = group.Find("root/Particle System01");
        Transform washTwo = group.Find("root/Particle System02");
        foreach (ParticleSystemRenderer renderer in group.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            bool centralGlow = renderer.transform == glow;
            bool colorWash = renderer.transform == washOne || renderer.transform == washTwo;
            //光晕与铺底保留源材质颜色和透明度，HDR 相机负责保留高亮叠加。
            renderer.sharedMaterial = OrderedMaterial(renderer.sharedMaterial,
                centralGlow || colorWash ? 3001 : 3003,
                unlitStreak: renderer.transform == streak);
        }
    }

    //纸张和签名条纹保留同层的层级顺序，白幕仍覆盖整个交接过程。
    private void BindImage(string path, int queue)
    {
        Image image = transform.Find(path).GetComponent<Image>();
        image.material = OrderedMaterial(image.material, queue);
    }

    //按模板和队列复用实例，无光照线条与其他阶段分别缓存。
    private Material OrderedMaterial(Material source, int queue, bool unlitStreak = false)
    {
        var key = (source, queue, unlitStreak);
        if (materials.TryGetValue(key, out Material cached)) return cached;
        Material material;
        if (source != null)
        {
            material = new Material(source);
        }
        else
        {
            material = new Material(Shader.Find("RimUnity_UI"));
            material.SetVector("_MainTex_ST", new Vector4(1, 1, 0, 0));
        }
        //放射线原用受光粒子材质；UI 保留贴图、颜色、加法混合和背面剔除，不受地图昼夜影响。
        if (unlitStreak) material.shader = Shader.Find("RimUnity_ParticleAdditiveBack");
        material.renderQueue = queue;
        materials.Add(key, material);
        return material;
    }

    //关闭过场时释放专用材质，不影响资源模板和下一次抽卡。
    private void OnDestroy()
    {
        foreach (Material material in materials.Values) Destroy(material);
        materials.Clear();
    }
}
