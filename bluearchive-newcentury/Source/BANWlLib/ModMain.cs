using System;
using HarmonyLib;
using Verse;

namespace BANWlLib
{
    // Mod 补丁入口负责尽早安装 Harmony 补丁，避免 Def 解析阶段的小人贴图查询先走原版 PNG。
    public static class ModMain
    {
        private static bool patched;

        // 安装 Harmony 补丁，负责保证所有运行时接管逻辑只执行一次。
        public static void ApplyHarmonyPatches()
        {
            if (patched)
            {
                return;
            }

            try
            {
                var harmony = new Harmony("com.BANWlLib");
                harmony.PatchAll();
                patched = true;

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
