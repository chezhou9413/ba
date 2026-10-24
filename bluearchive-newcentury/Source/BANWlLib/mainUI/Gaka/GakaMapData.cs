#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using BANWlLib.BaDef;
using BANWlLib.BANWGamecomp;
using System.Collections;
using System.Collections.Generic;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
using Verse;

namespace BANWlLib.mainUI.Gaka
{
    //抽卡结果数据，保存学生身份、PawnKind 和界面展示数据。
    public class gacaData
    {
        public BaStudentDef BaStudentDef;
        public PawnKindDef PawnKindDef;
        public string StudentId;
        public BaStudentUI StudentUI;
        public BaStudentData BaStudentData;
        public int starNum;
        public string AvatarPath;
        public bool isNew = false;
        public bool isUp = false;
    }
    //抽卡界面运行时缓存，保存界面预制体、当前卡池和抽卡结果。
    public static class GakaMapData
    {
        public static GameObject Content;
        public static GameObject GakaUIPet;
        public static Gacha selectGachaDef;
        public static GameObject GachaPofab;
        public static GameObject GakaAnimationPofab;
        public static bool presentationAssetsLoaded;
        public static MonoComp.MonoComp_GakaAnimationPofab activeAnimation;
        public static Gamecomp_gakaAction gamecomp_GakaAction;

        public static Sprite cardZheng;
        public static Sprite cardFan;
        public static Dictionary<Gacha, int> GachaType = new Dictionary<Gacha, int>();

        public static GameObject showGaList;

        public static bool tenGacha = false;
        public static List<gacaData> gacaDatas = new List<gacaData>();

        public static Sprite studentcard1star;
        public static Sprite studentcard2star;
        public static Sprite studentcard3star;

        public static Sprite gtwenhaoback;
        public static Sprite srwenhaoback;
        public static Sprite srrwenhaoback;

        public static Sprite gtback;
        public static Sprite srback;
        public static Sprite srrback;

        public static Sprite gtstar;
        public static Sprite srstar;
        public static Sprite srrstar;

        public static bool isP3;
        public static GameObject ResultList;

        public static GameObject ShotMaskBack;
        public static GameObject GakaShotList;

        //释放本模块创建的卡面精灵，资源包中的纹理与预制体仍由资源系统持有。
        internal static void ReleasePresentationAssets()
        {
            ReleaseSprite(ref studentcard1star);
            ReleaseSprite(ref studentcard2star);
            ReleaseSprite(ref studentcard3star);
            ReleaseSprite(ref gtwenhaoback);
            ReleaseSprite(ref srwenhaoback);
            ReleaseSprite(ref srrwenhaoback);
            ReleaseSprite(ref gtback);
            ReleaseSprite(ref srback);
            ReleaseSprite(ref srrback);
            ReleaseSprite(ref gtstar);
            ReleaseSprite(ref srstar);
            ReleaseSprite(ref srrstar);
#if RW_1_7
            //RimUnity 返回图集共享精灵，只清空引用。
            cardZheng = null;
            cardFan = null;
#else
            //Unity 的图集查询返回独立副本，需要释放。
            ReleaseSprite(ref cardZheng);
            ReleaseSprite(ref cardFan);
#endif
            presentationAssetsLoaded = false;
            ResultList = null;
            showGaList = null;
            GakaAnimationPofab = null;
        }

        //销毁并清空拥有的精灵，允许加载中途失败时清理已创建的部分。
        private static void ReleaseSprite(ref Sprite sprite)
        {
            if (sprite != null) Engine.Object.Destroy(sprite);
            sprite = null;
        }

        //清空抽卡界面的静态对象和结果引用。
        public static void Reset()
        {
            Gakalord.CloseAnimation(activeAnimation);
            ReleasePresentationAssets();
            isP3 = false;
            tenGacha = false;
            gacaDatas = new List<gacaData>();
            GachaType = new Dictionary<Gacha,int>();
            gamecomp_GakaAction = null;
            Content = null;
            GakaUIPet = null;
            selectGachaDef = null;
            GachaPofab = null;
            ShotMaskBack = null;
            GakaShotList = null;
        }
    }
}
