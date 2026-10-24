using System.Collections.Generic;
using RimUnity;
using RimUnity.UI;

namespace BANWlLib.mainUI.Initialization;

//把签字与结算页的画布排序落实到材质队列，防止背景粒子覆盖卡面。
internal sealed class BAGachaRenderOrder : MonoBehaviour
{
    private readonly Dictionary<(Material source, int queue), Material> materials = new();

    //只配置当前过场实例，沿用原 Shader、贴图、混合及深度状态。
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

    //纸张和签名条纹保留同层的层级顺序，白幕仍覆盖整个交接过程。
    private void BindImage(string path, int queue)
    {
        Image image = transform.Find(path).GetComponent<Image>();
        image.material = OrderedMaterial(image.material, queue);
    }

    //同一模板与队列共用一个实例材质，避免逐粒子重复复制和修改共享资源。
    private Material OrderedMaterial(Material source, int queue)
    {
        var key = (source, queue);
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
