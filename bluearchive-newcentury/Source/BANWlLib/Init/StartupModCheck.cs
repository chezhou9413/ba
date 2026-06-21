using Verse;
using UnityEngine;

namespace BANWlLib.Init
{
    [StaticConstructorOnStartup]
    public class StartupModCheck
    {
        static StartupModCheck()
        {
            // GPU 检测
            LongEventHandler.QueueLongEvent(() =>
            {
                CheckGPUCompatibility();
            }, "BANW_CheckingGPU", false, null);
        }

        static void CheckGPUCompatibility()
        {
            int vendorID = SystemInfo.graphicsDeviceVendorID;
            string gpuName = SystemInfo.graphicsDeviceName;

            // 0x10DE = NVIDIA, 非N卡则提示
            if (vendorID == 0x10DE)
                return;

            string vendorLabel;
            if (vendorID == 0x1002 || vendorID == 0x1022) // AMD
                vendorLabel = "AMD";
            else if (vendorID == 0x8086) // Intel
                vendorLabel = "Intel";
            else
                vendorLabel = "未知厂商";

            Dialog_MessageBox dialog = new Dialog_MessageBox(
                text: $"检测到你的显卡为 {vendorLabel} 显卡（{gpuName}）\n该显卡对 Unity 粒子系统的支持可能不太理想\n如遇到卡顿，可以前往 Mod 设置中关闭爆发型粒子效果",
                buttonAText: "我知道了",
                buttonAAction: null,
                buttonBText: null,
                buttonBAction: null,
                title: "BANW 显卡兼容性提示"
            );
            Find.WindowStack.Add(dialog);
        }
    }
}
