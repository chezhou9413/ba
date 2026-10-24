using RimUnity;

namespace BANWlLib.mainUI.Initialization;

//界面特效使用真实帧时间，避免调试窗口或地图暂停阻断发射与淡入。
internal static class BAUiParticles
{
    //配置实例中的整组粒子，包含稍后才会激活的颜色分支和子发射器。
    internal static void Configure(GameObject root)
    {
        foreach (ParticleSystem particle in root.GetComponentsInChildren<ParticleSystem>(true))
            particle.main.useUnscaledTime = true;
    }
}
