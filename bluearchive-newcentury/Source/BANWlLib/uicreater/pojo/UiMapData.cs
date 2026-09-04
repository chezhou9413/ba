using BANWlLib.mainUI.Gaka;
using BANWlLib.mainUI.ManualUI;
using BANWlLib.mainUI.Mission;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.Tool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Verse;

namespace newpro
{
    //地图UI数据中心负责保存主界面、入口、资源包与各功能面板的运行时引用。
    [StaticConstructorOnStartup]
    public static class UiMapData
    {
        public static GameObject openUIBUTT;
        public static Dictionary<string, UIbody> BodyMap = new Dictionary<string, UIbody>();
        public static Dictionary<string, string> ImagraceMap = new Dictionary<string, string>();
        public static bool uiclose;
        public static string choosbody;
        public static bool isOpenGacha;
        public static UnityEngine.UI.Text stoneText;
        public static UnityEngine.UI.Text poitText;
        public static UnityEngine.UI.Text poitText2;
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
        public static UnityEngine.UI.Text qinghuishitext1;
        public static UnityEngine.UI.Text huangpiaotext1;
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
        public static UnityEngine.UI.Text dsptext;
        public static GameObject buyParticle;
        public static Camera uiCamera;
        public static UnityEngine.UI.Text jingcuixianshi;
        public static GameObject goumaiMack;
        public static AudioSource mainBgmPlay;
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
                bundle.Unload(false);
            }

            BAUIRimWorldSpriteLoader.ClearAll();
        }

        //销毁地图UI对象并重置所有跨地图静态引用。
        public static void Reset()
        {
            isLocKBack = false;
            BAManualUIImageLoader.ClearAll();
            BAUIRimWorldSpriteLoader.ClearAll();
            RimWorldUISpriteUtil.ClearGeneratedSpriteCache();

            if (mainUI != null)
            {
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

            mainBgmPlay = null;
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
