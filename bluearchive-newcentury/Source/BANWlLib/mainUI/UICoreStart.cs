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
    public static class UICoreStart
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

        //加载主UI资源包并创建什亭之匣、入口按钮和独立COST界面。
        public static bool InitializeMianUI()
        {
            if (UiMapData.mainUI != null)
            {
                UnityEngine.Object.Destroy(UiMapData.mainUI);
                UiMapData.mainUI = null;
            }
            if (UiMapData.uiCamera != null)
            {
                UnityEngine.Object.Destroy(UiMapData.uiCamera.gameObject);
                UiMapData.uiCamera = null;
            }
            if (UiMapData.costUI != null)
            {
                UnityEngine.Object.Destroy(UiMapData.costUI);
                UiMapData.costUI = null;
            }
            UiMapData.uplodorBundle();
            string abPath = Path.Combine(
                  LoadedModManager.GetMod<newpro>().Content.RootDir,
                  "1.6", "AssetBundles", "bamainui.ab"
              );
            UiMapData.bundle = AssetBundle.LoadFromFile(abPath);
            if (UiMapData.bundle == null)
            {
                return false;
            }
            var prefab = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/mainUI.prefab");
            ManualMapData.messageUI = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/messageUI.prefab");
            ManualMapData.messageUIQuek = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/messageUIQuek.prefab");
            UiMapData.mainUI = UnityEngine.Object.Instantiate(prefab);
            GakaMapData.GakaUIPet = UiMapData.mainUI.transform.Find("GaKa").gameObject;
            UiMapData.mainUI.AddComponent<keyevents>();
            UiMapData.mainUI.transform.SetParent(null);
            UiMapData.goumaiMack = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/goumaiMack.prefab");
            UiMapData.uiCamera = UnityEngine.Object.Instantiate(UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/Main Camera.prefab").GetComponent<Camera>());
            UiMapData.uiCamera.depth = 100f;
            UiMapData.mainUI.GetComponent<Canvas>().worldCamera = UiMapData.uiCamera;
            UiMapData.buyParticle = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/lizi.prefab");
            UiMapData.uiCamera.gameObject.SetActive(false);
            UiMapData.showUI = UnityEngine.Object.Instantiate(UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/OpenUi.prefab"));
            Button openMainButton = UiMapData.showUI.transform.Find("MainButtom").GetComponent<Button>();
            Navigation navigation = openMainButton.navigation;
            navigation.mode = Navigation.Mode.None;
            openMainButton.navigation = navigation;
            openMainButton.onClick.AddListener(() =>
            {
                UICoreStart.showMianUI();
            });
            UiMapData.showUI.transform.SetAsFirstSibling();
            UiMapData.openUIBUTT = openMainButton.gameObject;
            UiMapData.openUIBUTT.AddComponent<LongPressDraggableButton>();
            InitializeCostUI();
            UnityEngine.Object.DontDestroyOnLoad(UiMapData.mainUI);
            UnityEngine.Object.DontDestroyOnLoad(UiMapData.uiCamera.gameObject);
            getShopButtonImage(UiMapData.bundle);
            Setshopselectpage();
            spineref.daySpine = UiMapData.mainUI.transform.Find("dayTime").gameObject;
            aronaSpineUIController aronaSpineUI = UiMapData.mainUI.transform.Find("dayTime/Button").GetComponent<aronaSpineUIController>();
            lordHudong.LordArona(aronaSpineUI);
            spineref.nightSpine = UiMapData.mainUI.transform.Find("nightTime").gameObject;
            aronaSpineUIController PunaraSpineUI = UiMapData.mainUI.transform.Find("nightTime/Button").GetComponent<aronaSpineUIController>();
            lordHudong.LordPunara(PunaraSpineUI);
            UiMapData.qinghuishitext1 = UiMapData.mainUI.transform.Find("daohang").transform.Find("qinghuishishuliang").GetComponent<UnityEngine.UI.Text>();
            UiMapData.huangpiaotext1 = UiMapData.mainUI.transform.Find("daohang").transform.Find("qianxianshi").GetComponent<UnityEngine.UI.Text>();
            UiMapData.mainUI.transform.Find("fanhui").gameObject.AddComponent<MonoComp_BackButton>();
            UiMapData.mainBgmPlay = UiMapData.mainUI.transform.Find("BgmPlay").GetComponent<AudioSource>();
            UiMapData.mainAudioPlay = UiMapData.mainUI.transform.Find("SoundEffectPlay").GetComponent<AudioSource>();
            UiMapData.jingcuixianshi = UiMapData.mainUI.transform.Find("shangdian/showtextsuipian").GetComponent<UnityEngine.UI.Text>();
            UiMapData.mainUI.transform.Find("Buttom").transform.Find("zhaomu").GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowGaka();
            });
            UiMapData.mainUI.transform.Find("Buttom").transform.Find("shangdian").GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowShopUI();
            });
            UiMapData.mainUI.transform.Find("Buttom").transform.Find("renwu").GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowMissionUI();
            });
            UiMapData.mainUI.transform.Find("Buttom").transform.Find("zonglizhan").GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowTotalWarUI();
            });
            UiMapData.mainUI.transform.Find("daohang/fanhuiyouxi").GetComponent<Button>().onClick.AddListener(() =>
            {
                fanhuiyouxi();
            });
            UiMapData.shotpet = UiMapData.mainUI.transform.Find("shangdian/ScrollView/Viewport/Content").gameObject;
            UiMapData.dsptext = UiMapData.mainUI.transform.Find("shangdian/dsp/Text").gameObject.GetComponent<UnityEngine.UI.Text>();
            UiMapData.shop = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/shot.prefab");
            Gakalord.lordGaka(UiMapData.bundle);
            shotlord.Initializeshotlord();
            return true;
        }

        //把独立CostUI挂到入口按钮上方并绑定模组侧运行时Presenter。
        private static void InitializeCostUI()
        {
            GameObject prefab = UiMapData.bundle.LoadAsset<GameObject>(
                "Assets/Scenes/Resources/UI/CostUI.prefab");
            if (prefab == null)
            {
                throw new InvalidDataException("bamainui.ab 中缺少 CostUI.prefab。" );
            }

            UiMapData.costUI = UnityEngine.Object.Instantiate(
                prefab,
                UiMapData.openUIBUTT.transform,
                false);
            UiMapData.costUI.name = "CostUI";

            RectTransform rect = UiMapData.costUI.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(
                CostUiDragController.DefaultPositionX,
                CostUiDragController.DefaultPositionY);
            rect.localScale = Vector3.one;

            Transform costRoot = UiMapData.costUI.transform.Find("CostRoot");
            if (costRoot == null)
            {
                throw new InvalidDataException("CostUI.prefab 缺少 CostRoot。" );
            }

            costRoot.localScale = Vector3.one * 0.25f;
            UiMapData.costUI.AddComponent<CostUiPresenter>().Initialize(UiMapData.bundle);
            UiMapData.costUI.AddComponent<CostUiDragController>().Initialize(
                rect,
                costRoot as RectTransform);
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
