using AlienRace;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.BaDef
{
    public class StudentBio
    {
        public string StudentDesp;
        public string StudentBioName;
        public string AcademyLogoPath;
        public string StudentCard;
    }
    public class WapenUI
    {
        public string WapenUIImagePath;
        public string WapenTypeText;
    }
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
        //头像图像资源缓存,自动获取缓存
        public Sprite StudentAvatarSprite;
        //种族名字缓存,自动获取缓存
        public string RaceDefName;
        //头像图片路径,自动获取缓存
        public string StudentAvatarPath;
        public Skills Skills = new Skills();
        public Ability Ability1 = new Ability();
        public Ability Ability2 = new Ability();
        public Ability Ability3 = new Ability();
        public Ability Ability4 = new Ability();
        public WapenUI WapenUI= new WapenUI();
        public StudentBio StudentBio = new StudentBio();
    }
    public class BaStudentRaceDef : ThingDef_AlienRace
    {
        public BaStudentUI BaStudentUI;
        public BaStudentData baStudentData;
    }

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

    public enum damageType
    {
        Explosion,
        Mysterious,
        Vibration,
        Through,
        Composite
    }

    public enum posType
    {
        Assist,
        Henc,
        Medical,
        Tank,
        Vehicle
    }
}
