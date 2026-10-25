using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using BANWlLib;
using BANWlLib.mainUI.Mission;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.Initialization;
using BANWlLib.mainUI.StudentManual;
using HarmonyLib;
using MyCoolMusicMod.MyCoolMusicMod;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RimUnity;
using RimUnity.UI;
using RimUnity.Video;
using Verse;
namespace newpro
{
    //游戏级 UI 初始化组件在地图可用后延迟创建 BA 主界面，并在读档或新游戏时清理旧 UI 缓存。
    public class GameComponent_LateInit : GameComponent
    {
        private bool hasInitializedThisSession = false;
        private bool needReinitUI = false;
        private bool initializationQueued = false;
        private bool initializationFailed = false;
        //由游戏创建当前存档的 UI 初始化状态。
        public GameComponent_LateInit(Game game) { }
        public static GameObject uiInstance = null;

        public List<string> UIheads;
        public List<string> UIbodys;
        public string UIimgPath;
        //保存和读取 UI 初始化组件的运行状态。
        public override void ExposeData()
        {
            base.ExposeData();
        }
        //游戏初始化完成回调，保留给后续需要在存档加载后补充初始化的逻辑。
        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }
        //每 tick 检查地图是否可用，并在合适时机排队执行一次 UI 初始化。
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
        //初始化抽卡 UI 系统，加载资源路径、事件系统、主界面、图鉴和任务界面。
        public static bool InitializeGachaUI()
        {
            try
            {
                using (var timing = new BAUIInitializationTiming("整套界面"))
                {
                    if (UiMapData.modRootPath == null)
                    {
                        UiMapData.modRootPath = LoadedModManager.GetMod<BANWlLib.newpro>().Content.RootDir;
                    }

                    string UIimgPath = Path.Combine(UiMapData.modRootPath, "Common", "Textures");
                    UiMapData.UIraceimg = Path.Combine(UiMapData.modRootPath, UiMapData.ContentVersion, "Defs", "GameDefs", "raceimg");

                    if (UiMapData.ImagraceMap == null || UiMapData.ImagraceMap.Count == 0)
                    {
                        UiMapData.ImagraceMap = imgcvT2d.GetPngMap(UiMapData.UIraceimg);
                    }

                    UiMapData.uiclose = false;

                    if (Engine.EventSystems.EventSystem.current == null)
                    {
                        GameObject eventSystem = new GameObject("EventSystem");
                        eventSystem.AddComponent<Engine.EventSystems.EventSystem>();
                    }
                    timing.Mark("路径与事件系统");
                    if (!UICoreStart.InitializeMianUI())
                    {
                        throw new System.InvalidOperationException("主 UI 资源加载失败。");
                    }
                    timing.Mark("主界面");
                    LoopBGMManager.EnsureAudioLoaded();
                    timing.Mark("启动背景音频请求");
                    ManualLord.lord();
                    timing.Mark("学生手册");
                    MissionUIlord.lord();
                    UiMapData.mainUI.SetActive(false);
                    timing.Mark("任务界面");
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[抽卡UI] 初始化 BA UI 核心失败，已停止本轮重试。\n{ex}");
                UiMapData.Reset();
                ManualMapData.Reset();
                Messages.Message("BA 界面加载失败，已关闭未完成的界面。请检查前置模组版本，详细原因见 Player.log。", MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }
        //获取UI图片路径
        private static string GetUIImagePath()
        {
            return Path.Combine(
                LoadedModManager.GetMod<BANWlLib.newpro>().Content.RootDir,
                "Common", "Textures"
            );
        }


        [HarmonyPatch(typeof(UIRoot), "UIRootOnGUI")]
        //Harmony 补丁在 BA 全屏 UI 打开时拦截基础 UIRoot 的 OnGUI。
        public static class PatchDisableUIRootOnGUI
        {
            //Prefix 根据 BA UI 是否接管输入决定是否继续执行原版 OnGUI。
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
        //Harmony 补丁在 BA 全屏 UI 打开时拦截基础 UIRoot 的 Update。
        public static class PatchDisableUIRootUpdate
        {
            //Prefix 根据 BA UI 是否接管输入决定是否继续执行原版 Update。
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
        //Harmony 补丁在 BA 全屏 UI 打开时拦截入口界面的 OnGUI。
        public static class PatchDisableUIRootOnGUIE
        {
            //Prefix 根据 BA UI 是否接管输入决定是否继续执行入口界面 OnGUI。
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
        //Harmony 补丁在 BA 全屏 UI 打开时拦截入口界面的 Update。
        public static class PatchDisableUIRootUpdateE
        {
            //Prefix 根据 BA UI 是否接管输入决定是否继续执行入口界面 Update。
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
        //Harmony 补丁在 BA 全屏 UI 打开时拦截游戏内界面的 OnGUI。
        public static class PatchDisableUIRootOnGUIP
        {
            //Prefix 根据 BA UI 是否接管输入决定是否继续执行游戏内界面 OnGUI。
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
        //Harmony 补丁在 BA 全屏 UI 打开时拦截游戏内界面的 Update。
        public static class PatchDisableUIRootUpdateP
        {
            //Prefix 根据 BA UI 是否接管输入决定是否继续执行游戏内界面 Update。
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
    //新游戏创建补丁在创建新存档时标记 BA UI 需要重建。
    [HarmonyPatch(typeof(Game), "InitNewGame")]
    public static class Patch_GameInitNewGame
    {
        //Postfix 在新游戏初始化后通知 UI 组件清理旧缓存。
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
    //存档加载补丁在读档后标记 BA UI 需要重建。
    [HarmonyPatch(typeof(Game), "LoadGame")]
    public static class Patch_GameLoadGame
    {
        //Postfix 在读档完成后通知 UI 组件清理旧缓存。
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
