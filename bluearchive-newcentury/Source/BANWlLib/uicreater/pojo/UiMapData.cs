#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using BANWlLib.mainUI.Gaka;
using BANWlLib.mainUI.ManualUI;
using BANWlLib.mainUI.Mission;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.Tool;
using System.Collections.Generic;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
#if RW_1_7
using RimUnity.UI;
#else
using UnityEngine.UI;
#endif
#if RW_1_7
using RimUnity.Video;
#else
using UnityEngine.Video;
#endif
using Verse;

namespace newpro
{
    //地图UI数据中心保存主界面、入口、资源包与各功能面板的运行时引用。
    [StaticConstructorOnStartup]
    public static class UiMapData
    {
#if RW_1_7
        public const string ContentVersion = "1.7";
#else
        public const string ContentVersion = "1.6";
#endif
        public static GameObject openUIBUTT;
        public static Dictionary<string, UIbody> BodyMap = new Dictionary<string, UIbody>();
        public static Dictionary<string, string> ImagraceMap = new Dictionary<string, string>();
        public static bool uiclose;
        public static string choosbody;
        public static bool isOpenGacha;
        public static Engine.UI.Text stoneText;
        public static Engine.UI.Text poitText;
        public static Engine.UI.Text poitText2;
        public static string UIraceimg;
        public static List<string> carddata = new List<string>();
        public static GameObject shilianjiesuan;
        public static GameObject poitlist;
        public static GameObject card;
        public static Dictionary<string, GameObject> cardHead = new Dictionary<string, GameObject>();
        public static int chikcrad;
        public static GameObject mainUI;
        public static VideoClip aluolavideoA;
        public static List<VideoClip> aluolavideoB = new List<VideoClip>();
        public static GameObject showUI;
        public static GameObject costUI;
        public static AssetBundle bundle;
        public static Engine.UI.Text qinghuishitext1;
        public static Engine.UI.Text huangpiaotext1;
        public static bool isOpenShop = false;
        public static VideoClip xiaokongvideoA;
        public static List<VideoClip> xiaokongvideoB = new List<VideoClip>();
        public static VideoClip xiaokongvideoC;
        public static GameObject selectShotPage;
        public static Sprite shopButtonImageSelect;
        public static Sprite shopButtonImageNoSelect;
        public static string modRootPath;
        public static GameObject shop;
        public static GameObject shotpet;
        public static List<GameObject> ordinaryOBJ = new List<GameObject>();
        public static List<GameObject> Fragment1OBJ = new List<GameObject>();
        public static List<GameObject> Fragment2OBJ = new List<GameObject>();
        public static Engine.UI.Text dsptext;
        public static GameObject buyParticle;
        public static Camera uiCamera;
        public static Engine.UI.Text jingcuixianshi;
        public static GameObject goumaiMack;
        public static AudioSource mainAudioPlay;
        public static bool isLocKBack = false;

        //触发静态数据中心初始化并保留字段的声明默认值。
        static UiMapData()
        {
        }

        //卸载已载入资源包并清理通过资源包生成的Sprite缓存。
        public static void uplodorBundle()
        {
            if (bundle != null)
            {
#if !RW_1_7
                bundle.Unload(false);
#endif
            }

            BAUIRimWorldSpriteLoader.ClearAll();
        }

        //销毁地图UI对象并重置所有跨地图静态引用。
        public static void Reset()
        {
            MyCoolMusicMod.MyCoolMusicMod.LoopBGMManager.StopUiAudio();
            isLocKBack = false;
            BANWlLib.mainUI.Images.BAUIImageCache.Reset();
            RimWorldUISpriteUtil.ClearGeneratedSpriteCache();

            if (mainUI != null)
            {
                mainUI.SetActive(false);
                Object.Destroy(mainUI);
            }
            if (uiCamera != null)
            {
                Object.Destroy(uiCamera.gameObject);
            }
            if (showUI != null)
            {
                Object.Destroy(showUI);
            }
            if (costUI != null)
            {
                Object.Destroy(costUI);
            }

            mainAudioPlay = null;
            MissionMapData.Reset();
            GakaMapData.Reset();
            openUIBUTT = null;
            isOpenGacha = false;
            jingcuixianshi = null;
            uiCamera = null;
            buyParticle = null;
            dsptext = null;
            shotpet = null;
            ordinaryOBJ.Clear();
            Fragment1OBJ.Clear();
            Fragment2OBJ.Clear();
            modRootPath = null;
            isOpenShop = false;
            showUI = null;
            costUI = null;
            aluolavideoB.Clear();
            aluolavideoA = null;
            xiaokongvideoA = null;
            xiaokongvideoB.Clear();
            xiaokongvideoC = null;
            mainUI = null;
            chikcrad = 0;
            cardHead.Clear();
            BodyMap.Clear();
            ImagraceMap.Clear();
            carddata.Clear();
            uiclose = false;
            poitlist = null;
            choosbody = null;
            shilianjiesuan = null;
            card = null;
            shop = null;
            selectShotPage = null;
            shopButtonImageSelect = null;
            shopButtonImageNoSelect = null;
            goumaiMack = null;
        }
    }
}
