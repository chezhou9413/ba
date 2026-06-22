using BANWlLib.mainUI;
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
    public static class spineref
    {
        public static GameObject daySpine;
        public static GameObject nightSpine;
    }

    public static class UICoreStart
    {
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

        public static bool IsNight(Map map)
        {
            int hour = GenLocalDate.HourOfDay(map);
            return hour >= 18 || hour < 6;
        }

        public static bool colseMianUI()
        {
            UiMapData.mainUI.SetActive(false);
            UiMapData.uiclose = false;
            return true;
        }

        public static void Setshopselectpage()
        {
            UiMapData.selectShotPage = UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("yiban").gameObject;
            UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("yiban").gameObject.AddComponent<ShopButtonPage>();
            UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("shenmingwenzi1").gameObject.AddComponent<ShopButtonPage>();
            UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("shenmingwenzi2").gameObject.AddComponent<ShopButtonPage>();
        }

        public static bool ShowShopUI()
        {
            UiMapData.isOpenShop = true;
            LoopBGMManager.switchUiBgm("bgm3");
            UiMapData.mainUI.transform.Find("shangdian").gameObject.SetActive(true);
            MonoComp_BackButton.instance.setNewObj(UiMapData.mainUI.transform.Find("shangdian").gameObject, "bgm3");
            ShopEvents.RaiseRefresh();
            return true;
        }

        public static bool ShowGaka()
        {
            LoopBGMManager.switchUiBgm("bgm");
            GakaMapData.GakaUIPet.SetActive(true);
            Gakalord.OpenGakaUI();
            MonoComp_BackButton.instance.setNewObj(UiMapData.mainUI.transform.Find("GaKa").gameObject, "bgm");
            ShopEvents.RaiseRefresh();
            return true;
        }

        public static bool ShowMissionUI()
        {
            LoopBGMManager.switchUiBgm("bgm5");
            UiMapData.mainUI.transform.Find("Mission").gameObject.SetActive(true);
            MonoComp_BackButton.instance.setNewObj(UiMapData.mainUI.transform.Find("Mission").gameObject, "bgm5");
            ShopEvents.RaiseRefresh();
            return true;
        }

        public static bool ShowTotalWarUI()
        {
            LoopBGMManager.switchUiBgm("bgm3");
            UiMapData.mainUI.transform.Find("TotalWar").gameObject.SetActive(true);
            MonoComp_BackButton.instance.setNewObj(UiMapData.mainUI.transform.Find("TotalWar").gameObject, "bgm3");
            ShopEvents.RaiseRefresh();
            return true;
        }

        public static bool CloseShopUI()
        {
            UiMapData.isOpenShop = false;
            UiMapData.mainUI.transform.Find("shangdian").gameObject.SetActive(false);
            return true;
        }

        public static bool getShopButtonImage(AssetBundle bundle)
        {
            Image selectShotImage = UiMapData.mainUI.transform.Find("shangdian").transform.Find("xuanze").transform.Find("yiban").gameObject.GetComponent<Image>();
            GameObject temp = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/weixuanzeianliutex.prefab");
            UiMapData.shopButtonImageSelect = temp.GetComponent<Image>().sprite;
            UiMapData.shopButtonImageNoSelect = selectShotImage.sprite;
            return true;
        }

        private static void ClearSelectedUiObject()
        {
            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem != null)
            {
                currentEventSystem.SetSelectedGameObject(null);
            }
        }
    }

    public class newpro : Mod
    {
        public newpro(ModContentPack content) : base(content)
        {
        }
    }
}
