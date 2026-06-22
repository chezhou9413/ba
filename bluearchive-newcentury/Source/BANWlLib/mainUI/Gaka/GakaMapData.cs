using BANWlLib.BaDef;
using BANWlLib.BANWGamecomp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.mainUI.Gaka
{
    /// <summary>
    /// 抽卡结果数据，负责保存学生身份、PawnKind 和界面展示数据。
    /// </summary>
    public class gacaData
    {
        public BaStudentDef BaStudentDef;
        public PawnKindDef PawnKindDef;
        public string StudentId;
        public BaStudentUI StudentUI;
        public BaStudentData BaStudentData;
        public int starNum;
        public Sprite gakaAvt;
        public bool isNew = false;
        public bool isUp = false;
    }
    /// <summary>
    /// 抽卡界面运行时缓存，负责保存界面预制体、当前卡池和抽卡结果。
    /// </summary>
    public static class GakaMapData
    {
        public static GameObject Content;
        public static GameObject GakaUIPet;
        public static Gacha selectGachaDef;
        public static GameObject GachaPofab;
        public static GameObject GakaAnimationPofab;
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
        public static void Reset()
        {
            isP3 = false;
            tenGacha = false;
            gacaDatas = new List<gacaData>();
            showGaList = null;
            GakaAnimationPofab = null;
            GachaType = new Dictionary<Gacha,int>();
            gamecomp_GakaAction = null;
            Content = null;
            GakaUIPet = null;
            selectGachaDef = null;
            GachaPofab = null;
        }
    }
}
