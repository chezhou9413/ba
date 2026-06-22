using BANWlLib;
using BANWlLib.mainUI.Mission;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.StudentManual;
using HarmonyLib;
using MyCoolMusicMod.MyCoolMusicMod;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Verse;
namespace newpro
{
    /// <summary>
    /// 游戏级 UI 初始化组件负责在地图可用后延迟创建 BA 主界面，并在读档或新游戏时清理旧 UI 缓存。
    /// </summary>
    public class GameComponent_LateInit : GameComponent
    {
        private bool hasInitializedThisSession = false;
        private bool needReinitUI = false;
        private bool initializationQueued = false;
        private bool initializationFailed = false;
        public GameComponent_LateInit(Game game) { }
        public static GameObject uiInstance = null;

        public List<string> UIheads;
        public List<string> UIbodys;
        public string UIimgPath;

        /// <summary>
        /// 保存和读取 UI 初始化组件的运行状态。
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
        }

        /// <summary>
        /// 游戏初始化完成回调，保留给后续需要在存档加载后补充初始化的逻辑。
        /// </summary>
        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        /// <summary>
        /// 每 tick 检查地图是否可用，并在合适时机排队执行一次 UI 初始化。
        /// </summary>
        public override void GameComponentTick()
        {
            if (needReinitUI)
            {
                needReinitUI = false;          
                hasInitializedThisSession = false; 
                initializationQueued = false;
                initializationFailed = false;
                Log.Message("[抽卡UI] 检测到读档，正在重置 UI...");
                UiMapData.Reset();
                ManualMapData.Reset();
            }

            if (!hasInitializedThisSession && !initializationQueued && !initializationFailed && Find.CurrentMap != null)
            {
                initializationQueued = true;
                LongEventHandler.QueueLongEvent(() =>
                {
                    bool initialized = InitializeGachaUI();
                    hasInitializedThisSession = initialized;
                    initializationFailed = !initialized;
                    initializationQueued = false;
                }, "加载BAUI核心中，请稍等(｡・ω・｡)", false, null);
            }

            if (UiMapData.uiclose && Find.TickManager != null && Find.TickManager.CurTimeSpeed != TimeSpeed.Normal)
            {
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            }
        }

        /// <summary>
        /// 初始化抽卡 UI 系统，负责加载资源路径、事件系统、主界面、图鉴和任务界面。
        /// </summary>
        /// <returns>是否成功初始化</returns>
        public static bool InitializeGachaUI()
        {
            try
            {
                if (UiMapData.modRootPath == null)
                {
                    UiMapData.modRootPath = LoadedModManager.GetMod<LordBgmData>().Content.RootDir;
                }

                string UIimgPath = Path.Combine(UiMapData.modRootPath, "Common", "Textures");
                UiMapData.UIraceimg = Path.Combine(UiMapData.modRootPath, "1.6", "Defs", "GameDefs", "raceimg");

                if (UiMapData.ImagraceMap == null || UiMapData.ImagraceMap.Count == 0)
                {
                    UiMapData.ImagraceMap = imgcvT2d.GetPngMap(UiMapData.UIraceimg);
                }

                UiMapData.uiclose = false;

                if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    UnityEngine.Object.DontDestroyOnLoad(eventSystem);
                }           
                if (!UICoreStart.InitializeMianUI())
                {
                    Log.Error("[抽卡UI] 主 UI 初始化失败，已停止本轮初始化，避免加载界面反复重试。");
                    return false;
                }
                ManualLord.lord();
                MissionUIlord.lord();
                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[抽卡UI] 初始化 BA UI 核心失败，已停止本轮重试。\n{ex}");
                return false;
            }
        }

        /// <summary>
        /// 获取UI图片路径
        /// </summary>
        /// <returns>UI图片路径</returns>
        private static string GetUIImagePath()
        {
            return Path.Combine(
                LoadedModManager.GetMod<LordBgmData>().Content.RootDir,
                "Common", "Textures"
            );
        }


        [HarmonyPatch(typeof(UIRoot), "UIRootOnGUI")]
        /// <summary>
        /// Harmony 补丁负责在 BA 全屏 UI 打开时拦截基础 UIRoot 的 OnGUI。
        /// </summary>
        public static class PatchDisableUIRootOnGUI
        {
            /// <summary>
            /// Prefix 根据 BA UI 是否接管输入决定是否继续执行原版 OnGUI。
            /// </summary>
            [HarmonyPrefix]
            public static bool prefix()
            {
                if (UiMapData.uiclose)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIRoot), "UIRootUpdate")]
        /// <summary>
        /// Harmony 补丁负责在 BA 全屏 UI 打开时拦截基础 UIRoot 的 Update。
        /// </summary>
        public static class PatchDisableUIRootUpdate
        {
            /// <summary>
            /// Prefix 根据 BA UI 是否接管输入决定是否继续执行原版 Update。
            /// </summary>
            [HarmonyPrefix]
            public static bool prefix()
            {
                if (UiMapData.uiclose)
                {
                    return false;

                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIRoot_Entry), "UIRootOnGUI")]
        /// <summary>
        /// Harmony 补丁负责在 BA 全屏 UI 打开时拦截入口界面的 OnGUI。
        /// </summary>
        public static class PatchDisableUIRootOnGUIE
        {
            /// <summary>
            /// Prefix 根据 BA UI 是否接管输入决定是否继续执行入口界面 OnGUI。
            /// </summary>
            [HarmonyPrefix]
            public static bool prefix()
            {
                if (UiMapData.uiclose)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIRoot_Entry), "UIRootUpdate")]
        /// <summary>
        /// Harmony 补丁负责在 BA 全屏 UI 打开时拦截入口界面的 Update。
        /// </summary>
        public static class PatchDisableUIRootUpdateE
        {
            /// <summary>
            /// Prefix 根据 BA UI 是否接管输入决定是否继续执行入口界面 Update。
            /// </summary>
            [HarmonyPrefix]
            public static bool prefix()
            {
                if (UiMapData.uiclose)
                {
                    return false;

                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIRoot_Play), "UIRootOnGUI")]
        /// <summary>
        /// Harmony 补丁负责在 BA 全屏 UI 打开时拦截游戏内界面的 OnGUI。
        /// </summary>
        public static class PatchDisableUIRootOnGUIP
        {
            /// <summary>
            /// Prefix 根据 BA UI 是否接管输入决定是否继续执行游戏内界面 OnGUI。
            /// </summary>
            [HarmonyPrefix]
            public static bool prefix()
            {
                if (UiMapData.uiclose)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIRoot_Play), "UIRootUpdate")]
        /// <summary>
        /// Harmony 补丁负责在 BA 全屏 UI 打开时拦截游戏内界面的 Update。
        /// </summary>
        public static class PatchDisableUIRootUpdateP
        {
            /// <summary>
            /// Prefix 根据 BA UI 是否接管输入决定是否继续执行游戏内界面 Update。
            /// </summary>
            [HarmonyPrefix]
            public static bool prefix()
            {
                if (UiMapData.uiclose)
                {
                    return false;

                }
                return true;
            }
        }
    }
}

namespace newpro
{
    /// <summary>
    /// 新游戏创建补丁负责在创建新存档时标记 BA UI 需要重建。
    /// </summary>
    [HarmonyPatch(typeof(Game), "InitNewGame")]
    public static class Patch_GameInitNewGame
    {
        /// <summary>
        /// Postfix 在新游戏初始化后通知 UI 组件清理旧缓存。
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Current.Game != null)
            {
                var gameComponent = Current.Game.GetComponent<GameComponent_LateInit>();
                if (gameComponent != null)
                {
                    var field = typeof(GameComponent_LateInit).GetField("needReinitUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(gameComponent, true);
                    }
                }
            }
        }
    }
}

namespace newpro
{
    /// <summary>
    /// 存档加载补丁负责在读档后标记 BA UI 需要重建。
    /// </summary>
    [HarmonyPatch(typeof(Game), "LoadGame")]
    public static class Patch_GameLoadGame
    {
        /// <summary>
        /// Postfix 在读档完成后通知 UI 组件清理旧缓存。
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Current.Game != null)
            {
                var gameComponent = Current.Game.GetComponent<GameComponent_LateInit>();
                if (gameComponent != null)
                {
                    var field = typeof(GameComponent_LateInit).GetField("needReinitUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(gameComponent, true);
                    }
                }
            }
        }
    }
}

