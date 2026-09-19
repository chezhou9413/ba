using BANWlLib.mainUI;
using BANWlLib.CostSystem;
using BANWlLib.mainUI.Gaka;
using BANWlLib.mainUI.Mission.GameComp;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.StudentManual;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using RimWorld;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Verse;

namespace BANWlLib
{
    //主界面Spine引用表负责保存昼夜角色展示对象。
    public static class spineref
    {
        public static GameObject daySpine;
        public static GameObject nightSpine;
    }

    //主界面入口负责加载资源包、创建独立界面并协调各页面开关。
    public static partial class UICoreStart
    {
        //检查拖动、任务和老师条件后决定是否允许打开什亭之匣。
        public static bool CanShowGachaUI()
        {
            try
            {
                if (LongPressDraggableButton.isMove)
                {
                    return false;
                }

                bool senseiExists = Find.CurrentMap.mapPawns.AllPawnsSpawned
                    .Any(p => p.kindDef != null && p.kindDef.defName == "BANW_Sensei");
                UiMapData.chikcrad += 1;
                GameComp_TaskQuest quest = Current.Game.GetComponent<GameComp_TaskQuest>();
                if (quest.isStarMission)
                {
                    Messages.Message("当前正在进行任务中，无法打开什亭之匣", MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                if (!senseiExists && !DebugSettings.godMode)
                {
                    Messages.Message("需要拥有老师才能打开什亭之匣。", MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                UiMapData.uiCamera.gameObject.SetActive(true);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }


        //关闭当前页面并返回游戏地图界面。
        public static void fanhuiyouxi()
        {
            if (UiMapData.isLocKBack)
            {
                return;
            }
            MonoComp_BackButton.instance.ClearAll();
            colseMianUI();
            if (UiMapData.uiCamera != null) UiMapData.uiCamera.gameObject.SetActive(false);
            if (UiMapData.mainUI != null) UiMapData.mainUI.SetActive(false);

            UiMapData.uiclose = false;

            if (UiMapData.showUI != null)
            {
                UiMapData.showUI.SetActive(true);
            }
            ClearSelectedUiObject();
        }

        //显示什亭之匣主界面并切换对应昼夜内容与背景音乐。
        public static void showMianUI()
        {
            if (CanShowGachaUI())
            {
                Map map = Find.CurrentMap;
                if (UiMapData.uiCamera != null)
                {
                    UiMapData.uiCamera.gameObject.SetActive(true);
                }
                if (IsNight(map))
                {
                    spineref.nightSpine.SetActive(true);
                    spineref.daySpine.SetActive(false);
                }
                else
                {
                    spineref.nightSpine.SetActive(false);
                    spineref.daySpine.SetActive(true);
                }
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                UiMapData.mainUI.SetActive(true);
                LoopBGMManager.switchUiBgm("bgm2");
                MonoComp_BackButton.instance.backList.Clear();
                MonoComp_BackButton.instance.backObj = UiMapData.mainUI;
                MonoComp_BackButton.instance.currentBgm = "bgm2";
                UiMapData.uiclose = true;
                UiMapData.showUI.SetActive(false);
                ClearSelectedUiObject();
            }
        }

        //根据地图本地时间判断是否使用夜间界面。
        public static bool IsNight(Map map)
        {
            int hour = GenLocalDate.HourOfDay(map);
            return hour >= 18 || hour < 6;
        }

        //隐藏什亭之匣主界面并恢复关闭状态。
        public static bool colseMianUI()
        {
            UiMapData.mainUI.SetActive(false);
            UiMapData.uiclose = false;
            return true;
        }

        //初始化商店分页按钮组件与默认选中页。
        public static void Setshopselectpage()
        {
            UiMapData.selectShotPage = UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("yiban").gameObject;
            UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("yiban").gameObject.AddComponent<ShopButtonPage>();
            UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("shenmingwenzi1").gameObject.AddComponent<ShopButtonPage>();
            UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("shenmingwenzi2").gameObject.AddComponent<ShopButtonPage>();
        }

        //打开商店页面并刷新商店事件。
        public static bool ShowShopUI()
        {
            UiMapData.isOpenShop = true;
            LoopBGMManager.switchUiBgm("bgm3");
            UiMapData.mainUI.transform.Find("shangdian").gameObject.SetActive(true);
            MonoComp_BackButton.instance.setNewObj(UiMapData.mainUI.transform.Find("shangdian").gameObject, "bgm3");
            ShopEvents.RaiseRefresh();
            return true;
        }

        //打开招募页面并初始化招募内容。
        public static bool ShowGaka()
        {
            LoopBGMManager.switchUiBgm("bgm");
            GakaMapData.GakaUIPet.SetActive(true);
            Gakalord.OpenGakaUI();
            MonoComp_BackButton.instance.setNewObj(UiMapData.mainUI.transform.Find("GaKa").gameObject, "bgm");
            ShopEvents.RaiseRefresh();
            return true;
        }

        //打开任务页面并切换任务背景音乐。
        public static bool ShowMissionUI()
        {
            LoopBGMManager.switchUiBgm("bgm5");
            UiMapData.mainUI.transform.Find("Mission").gameObject.SetActive(true);
            MonoComp_BackButton.instance.setNewObj(UiMapData.mainUI.transform.Find("Mission").gameObject, "bgm5");
            ShopEvents.RaiseRefresh();
            return true;
        }

        //打开总力战页面并切换对应背景音乐。
        public static bool ShowTotalWarUI()
        {
            LoopBGMManager.switchUiBgm("bgm3");
            UiMapData.mainUI.transform.Find("TotalWar").gameObject.SetActive(true);
            MonoComp_BackButton.instance.setNewObj(UiMapData.mainUI.transform.Find("TotalWar").gameObject, "bgm3");
            ShopEvents.RaiseRefresh();
            return true;
        }

        //关闭商店页面并清理商店打开标记。
        public static bool CloseShopUI()
        {
            UiMapData.isOpenShop = false;
            UiMapData.mainUI.transform.Find("shangdian").gameObject.SetActive(false);
            return true;
        }

        //从资源包读取商店按钮选中与未选中Sprite。
        public static bool getShopButtonImage(AssetBundle bundle)
        {
            Image selectShotImage = UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("yiban").gameObject.GetComponent<Image>();
            GameObject temp = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/weixuanzeianliutex.prefab");
            UiMapData.shopButtonImageSelect = temp.GetComponent<Image>().sprite;
            UiMapData.shopButtonImageNoSelect = selectShotImage.sprite;
            return true;
        }

        //清除EventSystem当前选中对象，避免游戏快捷键被UI焦点吞掉。
        private static void ClearSelectedUiObject()
        {
            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem != null)
            {
                currentEventSystem.SetSelectedGameObject(null);
            }
        }
    }

    //Mod 入口负责在 RimWorld 创建模组对象时安装轻量补丁，并记录全局资源根路径。
    public class newpro : Mod
    {
        //构造函数只执行轻量初始化，避免在游戏启动初始化阶段加载 UI 音频或贴图资源。
        public newpro(ModContentPack content) : base(content)
        {
            UiMapData.modRootPath = content.RootDir;
            BANWlLib.ModMain.ApplyHarmonyPatches();
        }
    }
}
