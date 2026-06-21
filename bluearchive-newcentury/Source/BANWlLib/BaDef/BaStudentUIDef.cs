using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.BaDef
{
    /// <summary>
    /// 学生简介配置，负责保存图鉴简介页显示的文字与学院图标路径。
    /// </summary>
    public class StudentBio
    {
        public string StudentDesp;
        public string StudentBioName;
        public string AcademyLogoPath;
        public string StudentCard;
    }

    /// <summary>
    /// 武器界面配置，负责保存图鉴中武器图标和武器类型文本。
    /// </summary>
    public class WapenUI
    {
        public string WapenUIImagePath;
        public string WapenTypeText;
    }

    /// <summary>
    /// 学生技能展示配置，负责保存图鉴界面显示的各项技能等级。
    /// </summary>
    public class Skills
    {
        public int Shooting = 0;
        public int Melee = 0;
        public int Construction = 0;
        public int Mining = 0;
        public int Cooking = 0;
        public int Plants = 0;
        public int Animals = 0;
        public int Crafting = 0;
        public int Artistic = 0;
        public int Medical = 0;
        public int Social = 0;
        public int Intellectual = 0;
    }

    /// <summary>
    /// 学生能力展示配置，负责保存技能图标、标题、说明和界面偏移。
    /// </summary>
    public class Ability
    {
        public string AbilityImagePath;
        public string AbilityTypeText;
        public string AbilityTitle;
        public string AbilitySubtitle;
        public string AbilityIntroduction;
        public float offSetX = 0f;
        public float offSetY = 0f;
    }

    /// <summary>
    /// 学生 UI 数据配置，负责保存图鉴、抽卡和任务界面需要展示的学生资源。
    /// </summary>
    public class BaStudentUI
    {
        public string StudentAvatar;
        public string StudentName = "Auto";
        public string BackgroundPath;
        public string CharacterimagePath;
        public float CharacterimageOffsetSize = 1f;
        public float CharacterimageOffsetX = 0f;
        public float CharacterimageOffsetY = 0f;
        public string CharacterTypePath;
        public int CharacterStarCount = 3;
        public string infotagImagePath1;
        public string infotagImagePath2;
        public string infotagImagePath3;
        public string infotagImagePath4;
        // 头像图像资源缓存，自动获取缓存。
        public Sprite StudentAvatarSprite;
        // 学生身份缓存，保存当前 studentId。
        public string StudentId;
        // 头像图片路径，自动获取缓存。
        public string StudentAvatarPath;
        public Skills Skills = new Skills();
        public Ability Ability1 = new Ability();
        public Ability Ability2 = new Ability();
        public Ability Ability3 = new Ability();
        public Ability Ability4 = new Ability();
        public WapenUI WapenUI = new WapenUI();
        public StudentBio StudentBio = new StudentBio();
    }

    /// <summary>
    /// 新学生数据 Def，负责保存学生身份、PawnKind 和界面数据。
    /// </summary>
    public class BaStudentDef : Def
    {
        public PawnKindDef kindDef;
        public string studentId;
        public BaStudentUI BaStudentUI;
        public BaStudentData baStudentData;
    }

    /// <summary>
    /// 学生玩法数据配置，负责保存属性类型、定位、抽卡返还和学生资源路径。
    /// </summary>
    public class BaStudentData
    {
        public damageType DamageType;
        public damageType DefenseType;
        public posType PosType;
        public int StarCont;
        public string DraggableAudio;
        public Dictionary<ThingDef, int> GakaStudentThingData = new Dictionary<ThingDef, int>();
        public Dictionary<ThingDef, int> UpGakaStudentThingData = new Dictionary<ThingDef, int>();
        public Dictionary<ThingDef, int> OneGakaStudentThingData = new Dictionary<ThingDef, int>();
        public string avtTexPath;
        public string stuSchool;
    }

    /// <summary>
    /// 学生攻击或防御属性类型，负责表达不同伤害体系的分类。
    /// </summary>
    public enum damageType
    {
        Explosion,
        Mysterious,
        Vibration,
        Through,
        Composite
    }

    /// <summary>
    /// 学生战斗定位类型，负责表达任务和图鉴中展示的位置分类。
    /// </summary>
    public enum posType
    {
        Assist,
        Henc,
        Medical,
        Tank,
        Vehicle
    }
}
