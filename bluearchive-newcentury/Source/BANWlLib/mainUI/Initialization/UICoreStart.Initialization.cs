using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
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
using RimUnity;
using RimUnity.EventSystems;
using RimUnity.UI;
using Verse;
using BANWlLib.mainUI.Initialization;

namespace BANWlLib
{
    //主界面资源加载、控件实例化与初始化分段计时。
    public static partial class UICoreStart
    {
        //加载主UI资源包并创建什亭之匣、入口按钮和独立COST界面。
        public static bool InitializeMianUI()
        {
            using (var timing = new BAUIInitializationTiming("核心界面"))
            {
                if (UiMapData.mainUI != null)
                {
                    UiMapData.mainUI.SetActive(false);
                    Engine.Object.Destroy(UiMapData.mainUI);
                    UiMapData.mainUI = null;
                }
                if (UiMapData.uiCamera != null)
                {
                    Engine.Object.Destroy(UiMapData.uiCamera.gameObject);
                    UiMapData.uiCamera = null;
                }
                if (UiMapData.costUI != null)
                {
                    Engine.Object.Destroy(UiMapData.costUI);
                    UiMapData.costUI = null;
                }
                UiMapData.uplodorBundle();
                UiMapData.bundle = AssetBundle.Open();
                timing.Mark("主界面资源包");
                if (UiMapData.bundle == null)
                {
                    return false;
                }
                var prefab = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/mainUI.prefab");
                ManualMapData.messageUI = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/messageUI.prefab");
                ManualMapData.messageUIQuek = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/messageUIQuek.prefab");
                UiMapData.mainUI = Engine.Object.Instantiate(prefab);
                GakaMapData.GakaUIPet = UiMapData.mainUI.transform.Find("GaKa").gameObject;
                UiMapData.mainUI.AddComponent<keyevents>();
                UiMapData.mainUI.transform.SetParent(null);
                UiMapData.goumaiMack = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/goumaiMack.prefab");
                UiMapData.uiCamera = Engine.Object.Instantiate(UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/Main Camera.prefab")).GetComponent<Camera>();
                UiMapData.uiCamera.depth = 100f;
                //原始 UI 相机使用 HDR，保留高亮粒子与低透明度相乘后的强度。
                UiMapData.uiCamera.allowHDR = true;
                UiMapData.mainUI.GetComponent<Canvas>().worldCamera = UiMapData.uiCamera;
                //复杂粒子需要场景顶点流，主界面与抽卡共用独立 UI 相机。
                UiMapData.mainUI.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                BAFullscreenLayout.ConfigureMain(UiMapData.mainUI);
                UiMapData.buyParticle = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/lizi.prefab");
                UiMapData.uiCamera.gameObject.SetActive(false);
                UiMapData.showUI = Engine.Object.Instantiate(UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/OpenUi.prefab"));
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
                timing.Mark("界面实例与COST");
                getShopButtonImage(UiMapData.bundle);
                Setshopselectpage();
                spineref.daySpine = UiMapData.mainUI.transform.Find("dayTime").gameObject;
                aronaSpineUIController aronaSpineUI = UiMapData.mainUI.transform.Find("dayTime/Button").GetComponent<aronaSpineUIController>();
                lordHudong.LordArona(aronaSpineUI);
                spineref.nightSpine = UiMapData.mainUI.transform.Find("nightTime").gameObject;
                aronaSpineUIController PunaraSpineUI = UiMapData.mainUI.transform.Find("nightTime/Button").GetComponent<aronaSpineUIController>();
                lordHudong.LordPunara(PunaraSpineUI);
                timing.Mark("对话路径绑定");
                UiMapData.qinghuishitext1 = UiMapData.mainUI.transform.Find("daohang").transform.Find("qinghuishishuliang").GetComponent<Engine.UI.Text>();
                UiMapData.huangpiaotext1 = UiMapData.mainUI.transform.Find("daohang").transform.Find("qianxianshi").GetComponent<Engine.UI.Text>();
                UiMapData.mainUI.transform.Find("fanhui").gameObject.AddComponent<MonoComp_BackButton>();
                //音乐使用原版播放器，移除旧预制体声源以免重新激活界面时自动播放。
                Engine.Object.Destroy(UiMapData.mainUI.transform.Find("BgmPlay").gameObject);
                UiMapData.mainAudioPlay = UiMapData.mainUI.transform.Find("SoundEffectPlay").GetComponent<AudioSource>();
                UiMapData.jingcuixianshi = UiMapData.mainUI.transform.Find("shangdian/showtextsuipian").GetComponent<Engine.UI.Text>();
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
                UiMapData.dsptext = UiMapData.mainUI.transform.Find("shangdian/dsp/Text").gameObject.GetComponent<Engine.UI.Text>();
                UiMapData.shop = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/shot.prefab");
                Gakalord.lordGaka(UiMapData.bundle);
                timing.Mark("抽卡界面资源");
                shotlord.Initializeshotlord();
                timing.Mark("商店商品");
                return true;
            }
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

            UiMapData.costUI = Engine.Object.Instantiate(
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
    }
}
