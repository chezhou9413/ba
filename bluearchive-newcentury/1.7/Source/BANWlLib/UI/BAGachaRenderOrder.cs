using System.Collections.Generic;
using RimUnity;
using RimUnity.UI;

namespace BANWlLib.mainUI.Initialization;

//将签字场景的作者排序落实到材质队列，防止相机再次排序时把背景粒子画在纸面上。
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

        //底板保留 3000，Panda 在 3001 抓取背景；纸张 107 和白幕 110 排在粒子之后。
        BindImage("Clip2/Card", 3007);
        BindImage("Clip2/Card/one", 3007);
        BindImage("Clip2/Card/tree", 3007);
        BindImage("Clip25", 3010);
        BindImage("Clip35/Clip45", 3010);
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
