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
    public class GameComponent_LateInit : GameComponent
    {
        private bool hasInitializedThisSession = false;
        private bool needReinitUI = false;
        public GameComponent_LateInit(Game game) { }
        public static GameObject uiInstance = null;

        public List<string> UIheads;
        public List<string> UIbodys;
        public string UIimgPath;
        // 保存和加载状态
        public override void ExposeData()
        {
            base.ExposeData();
        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void GameComponentTick()
        {
            if (needReinitUI)
            {
                needReinitUI = false;          
                hasInitializedThisSession = false; 
                Log.Message("[抽卡UI] 检测到读档，正在重置 UI...");
                // 读档时清理静态UI缓存，避免数据叠加
                UiMapData.Reset();
                ManualMapData.Reset();
            }

            // 原有逻辑保持不变
            if (!hasInitializedThisSession && Find.CurrentMap != null)
            {
                hasInitializedThisSession = true;
                LongEventHandler.QueueLongEvent(() =>
                {
                    InitializeGachaUI();
                }, "加载BAUI核心中，请稍等(｡・ω・｡)", false, null);
            }

            if (UiMapData.uiclose && Find.TickManager != null && Find.TickManager.CurTimeSpeed != TimeSpeed.Normal)
            {
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            }
        }

        /// <summary>
        /// 初始化抽卡UI系统
        /// 性能优化：分步加载，减少初始化时间
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

                // 性能优化：延迟加载角色卡数据，只在需要时加载
                if (UiMapData.ImagraceMap == null || UiMapData.ImagraceMap.Count == 0)
                {
                    UiMapData.ImagraceMap = imgcvT2d.GetPngMap(UiMapData.UIraceimg);
                }

                UiMapData.uiclose = false;

                // 确保EventSystem存在
                if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    UnityEngine.Object.DontDestroyOnLoad(eventSystem);
                }           
                UICoreStart.InitializeMianUI();
                ManualLord.lord();
                MissionUIlord.lord();
                return true;
            }
            catch (System.Exception ex)
            {
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
        public static class PatchDisableUIRootOnGUI
        {
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
        public static class PatchDisableUIRootUpdate
        {
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
        public static class PatchDisableUIRootOnGUIE
        {
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
        public static class PatchDisableUIRootUpdateE
        {
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
        public static class PatchDisableUIRootOnGUIP
        {
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
        public static class PatchDisableUIRootUpdateP
        {
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
    // 监听新游戏创建事件
    [HarmonyPatch(typeof(Game), "InitNewGame")]
    public static class Patch_GameInitNewGame
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // 标记需要重新初始化UI（新存档也要清理旧UI）
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
    // 监听游戏加载事件
    [HarmonyPatch(typeof(Game), "LoadGame")]
    public static class Patch_GameLoadGame
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // 标记需要重新初始化UI
            if (Current.Game != null)
            {
                var gameComponent = Current.Game.GetComponent<GameComponent_LateInit>();
                if (gameComponent != null)
                {
                    // 通过反射设置needReinitUI字段
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

