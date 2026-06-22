using System;
using HarmonyLib;
using Verse;

namespace BANWlLib
{
    [StaticConstructorOnStartup]
    public static class ModMain
    {
        static ModMain()
        {
            try
            {
                var harmony = new Harmony("com.BANWlLib");
                harmony.PatchAll();

                // Log.Message("[BANW] Harmony 补丁应用成功！"); // 注释：普通log输出，屏蔽
            }
            catch (Exception ex)
            {
                // 【关键修改】如果PatchAll()失败，在日志中打印详细的错误信息
                Log.Error($"[BANW] Harmony 补丁应用失败: {ex.ToString()}");
            }
        }
    }
} 