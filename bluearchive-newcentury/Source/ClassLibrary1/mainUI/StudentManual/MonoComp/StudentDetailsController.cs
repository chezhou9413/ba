using BANWlLib.BaDef;
using BANWlLib.mainUI.pojo;
using BANWlLib.Tool;
using BANWlLib.uicreater.tool;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using RimWorld;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    public class StudentDetailsController : MonoBehaviour
    {
        public static bool isWring = false;
        private bool deployInProgress = false;
        public BaStudentUI BaStudentUI;
        public StudentData studentData;
        private GameObject infoObj;
        private Image BackgroundImage;
        private Image Characterimage;
        private UnityEngine.UI.Text CharacterName;
        private UnityEngine.UI.Text CharacterLv;
        private Button DeployButton;
        private Button WithdrawButton;
        private static ManualDataGameComp tracker;
        private Image CharacterType;
        private GameObject CharacterStar;
        private GameObject SkillsBar;
        private GameObject mainInfo;
        private GameObject StudentBioContent;
        //原生偏移和缩放二维向量
        private Vector2 originalSize = Vector2.zero;
        private Vector2 originalOffset = Vector2.zero;
        void Awake()
        {
            StudentBioContent = transform.Find("Background").transform.Find("mainInfo").transform.Find("StudentBio").transform.Find("Background").transform.Find("Scroll View").transform.Find("Viewport").transform.Find("Content").gameObject;
            mainInfo = transform.Find("Background").transform.Find("mainInfo").gameObject;
            SkillsBar = transform.Find("Background").transform.Find("mainInfo").transform.Find("StudentInfo").transform.Find("Skills").transform.Find("SkillsBar").gameObject;
            BackgroundImage = transform.Find("Background").GetComponent<Image>();
            Characterimage = transform.Find("Background").transform.Find("Characterimage").GetComponent<Image>();
            CharacterName = transform.Find("Background").transform.Find("CharacterInfo").transform.Find("CharacterName").GetComponent<UnityEngine.UI.Text>();
            CharacterLv = transform.Find("Background").transform.Find("CharacterInfo").transform.Find("CharacterLv").GetComponent<UnityEngine.UI.Text>();
            CharacterType = transform.Find("Background").transform.Find("CharacterInfo").transform.Find("CharacterType").GetComponent<Image>();
            CharacterStar = transform.Find("Background").transform.Find("CharacterInfo").transform.Find("CharacterStar").gameObject;
            DeployButton = transform.Find("Background").transform.Find("Info").transform.Find("DeployButton").GetComponent<Button>();
            WithdrawButton = transform.Find("Background").transform.Find("Info").transform.Find("WithdrawButton").GetComponent<Button>();
            infoObj = transform.Find("Background").transform.Find("Info").gameObject;
            SetDeployButtomLogin();
        }
        void Enable()
        {

        }

        private static ManualDataGameComp GetTracker()
        {
            tracker = Current.Game?.GetComponent<ManualDataGameComp>();
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
            return tracker;
        }

        public void setData()
        {
            try
            {
                tracker = GetTracker();
                BackgroundImage.sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.BackgroundPath));
                Characterimage.sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.CharacterimagePath));
                CharacterType.sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.CharacterTypePath));
                CharacterName.text = BaStudentUI.StudentName;
                studentData = StudentRosterUtility.GetStudentData(tracker, BaStudentUI.RaceDefName);
                if (studentData != null)
                {
                    CharacterLv.text = "Lv." + studentData.StudentLv;
                }
                else
                {
                    CharacterLv.text = "Lv.1";
                }
                SetCharacterimageSizeAndOffset();
                SetDeployButton();
                SpawnStarCentered();
                SetInfoObj();
                SetSkills();
                SetActiveAbilitySet();
                SetWapenUI();
                SetStudentBio();
            }
            catch (Exception e)
            {
                Log.Error("[StudentDetailsController] 设置数据时发生错误: " + e.Message);
            }
        }

        private bool HasActiveStudentPawn()
        {
            return studentData != null &&
                   studentData.StudentPawn != null &&
                   !studentData.StudentPawn.DestroyedOrNull();
        }

        private bool IsStudentDeployedOrPending()
        {
            if (HasActiveStudentPawn())
            {
                return true;
            }

            return BaStudentUI != null &&
                   PawnDropHelper.HasPendingPawnForDefName(BaStudentUI.RaceDefName);
        }

        void SetDeployButton()
        {
            tracker = GetTracker();
            if (BaStudentUI != null)
            {
                studentData = StudentRosterUtility.GetStudentData(tracker, BaStudentUI.RaceDefName);
            }

            DeployButton.gameObject.SetActive(true);
            WithdrawButton.gameObject.SetActive(false);
            DeployButton.interactable = false;
            WithdrawButton.interactable = true;

            if (studentData == null)
            {
                DeployButton.transform.Find("mask").gameObject.SetActive(true);
                DeployButton.transform.Find("lock").gameObject.SetActive(true);
                DeployButton.interactable = false;
            }
            else if (IsStudentDeployedOrPending())
            {
                DeployButton.transform.Find("mask").gameObject.SetActive(false);
                DeployButton.transform.Find("lock").gameObject.SetActive(false);
                DeployButton.gameObject.SetActive(false);
                WithdrawButton.gameObject.SetActive(true);
            }
            else if (!studentData.isGoing)
            {
                DeployButton.transform.Find("mask").gameObject.SetActive(false);
                DeployButton.transform.Find("lock").gameObject.SetActive(false);
                DeployButton.interactable = true;
            }
            else
            {
                DeployButton.gameObject.SetActive(false);
                WithdrawButton.gameObject.SetActive(true);
            }
        }

        void ShowWithdrawStateImmediately()
        {
            DeployButton.interactable = false;
            DeployButton.gameObject.SetActive(false);
            WithdrawButton.gameObject.SetActive(true);
        }

        void Start()
        {

        }
        void SetDeployButtomLogin()
        {
            DeployButton.onClick.AddListener(() =>
            {
                try
                {
                    if (deployInProgress)
                    {
                        return;
                    }

                    tracker = GetTracker();
                    studentData = StudentRosterUtility.GetStudentData(tracker, BaStudentUI.RaceDefName);
                    if (studentData == null)
                    {
                        Log.Warning("[StudentDetailsController] 找不到当前学生存档数据，无法执行出击。");
                        SetDeployButton();
                        return;
                    }
                    if (studentData.isGoing || studentData.StudentPawn != null)
                    {
                        SetDeployButton();
                        return;
                    }
                    if (PawnDropHelper.HasPendingPawnForDefName(BaStudentUI.RaceDefName))
                    {
                        SetDeployButton();
                        return;
                    }

                    deployInProgress = true;
                    DeployButton.interactable = false;
                    Pawn pawn = PawnDropHelper.DropPawnsByDefName(BaStudentUI.RaceDefName);
                    if (pawn != null)
                    {
                        StudentRosterUtility.BindStudentPawn(studentData, pawn);
                        ShowWithdrawStateImmediately();
                        SetDeployButton();
                        LoopBGMManager.playEffAudio("出击音效");
                        lordStudentPawninfo(pawn,studentData);
                        Find.LetterStack.ReceiveLetter("学生抵达：" + BaStudentUI.StudentName + "", "学生" + BaStudentUI.StudentName + "已经抵达了老师的主殖民地！", LetterDefOf.PositiveEvent, pawn);
                        StudentManualEvents.RaiseRefresh();
                        StudentManualEvents.resetUIlistRefresh();
                    }
                }
                catch (Exception e)
                {
                    Log.Error("[StudentDetailsController] 出击按钮点击时发生错误: " + e.Message);
                }
                finally
                {
                    deployInProgress = false;
                    SetDeployButton();
                }
            });
            WithdrawButton.onClick.AddListener(() =>
            {
                tracker = GetTracker();
                studentData = StudentRosterUtility.GetStudentData(tracker, BaStudentUI.RaceDefName);
                if (studentData == null)
                {
                    SetDeployButton();
                    return;
                }
                if (!isWring)
                {
                    BamessageUI.ShowBaMessageUI("提醒", "*只能回收满血，没有生病的学生\r\n*学生在远行队内不能回收\r\n*学生死亡会保留星级和等级，在下一次抽到该学生出击时会应用\r\n*学生身上非自带的装备回收了会消失，比如：帽子，徽章，护符等，请确保回收前脱下装备", "了解");
                    isWring = true;
                    return;
                }
                if (!pawnUtils.IsAtFullHealth_IgnoreBenign(studentData.StudentPawn))
                {
                    BamessageUI.ShowBaMessageUI("召回失败", BaStudentUI.StudentName + " 的健康状态非正常，无法召回!", "返回");
                }
                else if (studentData.StudentPawn == null || studentData.StudentPawn.DestroyedOrNull())
                {
                    SetDeployButton();
                }
                else if (!studentData.StudentPawn.Spawned)
                {
                    BamessageUI.ShowBaMessageUI("召回失败", BaStudentUI.StudentName + "在远征队时无法召回!", "返回");
                }
                else
                {
                    LoopBGMManager.playEffAudio("出击音效");
                    pawnUtils.setStudentSave(studentData.StudentPawn, tracker);
                    studentData.StudentPawn.Destroy();
                    StudentRosterUtility.ClearStudentPawn(studentData);
                    SetDeployButton();
                    StudentManualEvents.RaiseRefresh();
                    StudentManualEvents.resetUIlistRefresh();
                }
            });
        }

        void SetInfoObj()
        {
            if (BaStudentUI.infotagImagePath1 == null || BaStudentUI.infotagImagePath1 == "")
            {
                infoObj.transform.Find("InfoTag1").gameObject.SetActive(false);
            }
            else
            {
                infoObj.transform.Find("InfoTag1").gameObject.SetActive(true);
                infoObj.transform.Find("InfoTag1").GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.infotagImagePath1));
            }
            if (BaStudentUI.infotagImagePath2 == null || BaStudentUI.infotagImagePath2 == "")
            {
                infoObj.transform.Find("InfoTag2").gameObject.SetActive(false);
            }
            else
            {
                infoObj.transform.Find("InfoTag2").gameObject.SetActive(true);
                infoObj.transform.Find("InfoTag2").GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.infotagImagePath2));
            }
            if (BaStudentUI.infotagImagePath3 == null || BaStudentUI.infotagImagePath3 == "")
            {
                infoObj.transform.Find("InfoTag3").gameObject.SetActive(false);
            }
            else
            {
                infoObj.transform.Find("InfoTag3").gameObject.SetActive(true);
                infoObj.transform.Find("InfoTag3").GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.infotagImagePath3));
            }
            if (BaStudentUI.infotagImagePath4 == null || BaStudentUI.infotagImagePath4 == "")
            {
                infoObj.transform.Find("InfoTag4").gameObject.SetActive(false);
            }
            else
            {
                infoObj.transform.Find("InfoTag4").gameObject.SetActive(true);
                infoObj.transform.Find("InfoTag4").GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.infotagImagePath4));
            }
        }
        void SetCharacterimageSizeAndOffset()
        {
            if (originalSize == Vector2.zero)
            {
                originalSize = Characterimage.rectTransform.sizeDelta;
            }
            if (originalOffset == Vector2.zero)
            {
                originalOffset = Characterimage.rectTransform.anchoredPosition;
            }

            Characterimage.rectTransform.sizeDelta = originalSize * BaStudentUI.CharacterimageOffsetSize;
            Characterimage.rectTransform.anchoredPosition = originalOffset + new Vector2(BaStudentUI.CharacterimageOffsetX, BaStudentUI.CharacterimageOffsetY);


        }

        void SpawnStarCentered()
        {
            // 首先，清空现有的星星。这部分代码是正确的。
            int childCount = CharacterStar.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(CharacterStar.transform.GetChild(i).gameObject);
            }

            // --- 新的居中逻辑 ---
            int totalStars = BaStudentUI.CharacterStarCount;
            float starSpacing = 50f; // 调整这个值来改变星星之间的间距

            // 计算初始偏移量
            float startX = 0f;
            if (totalStars % 2 == 0) // 星星总数为偶数
            {
                startX = (totalStars / 2 - 0.5f) * -starSpacing;
            }
            else // 星星总数为奇数
            {
                startX = (totalStars / 2) * -starSpacing;
            }
            // 循环实例化并放置每一颗星星
            for (int i = 0; i < totalStars; i++)
            {
                GameObject starObj = GameObject.Instantiate(ManualMapData.StarOBJ);
                starObj.transform.SetParent(CharacterStar.transform, false);
                RectTransform rt = starObj.GetComponent<RectTransform>();
                float xPos = startX + (i * starSpacing);
                rt.anchoredPosition = new Vector2(xPos, 0);
            }
        }

        void SetSkills()
        {
            // 射击 (Shooting)
            SkillsBar.transform.Find("Shooting").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Shooting.ToString();
            SkillsBar.transform.Find("Shooting").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Shooting / 20f;

            // 格斗 (Melee)
            SkillsBar.transform.Find("Melee").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Melee.ToString();
            SkillsBar.transform.Find("Melee").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Melee / 20f;

            // 建造 (Construction)
            SkillsBar.transform.Find("Construction").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Construction.ToString();
            SkillsBar.transform.Find("Construction").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Construction / 20f;

            // 采矿 (Mining)
            SkillsBar.transform.Find("Mining").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Mining.ToString();
            SkillsBar.transform.Find("Mining").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Mining / 20f;

            // 烹饪 (Cooking)
            SkillsBar.transform.Find("Cooking").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Cooking.ToString();
            SkillsBar.transform.Find("Cooking").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Cooking / 20f;

            // 种植 (Plants)
            SkillsBar.transform.Find("Plants").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Plants.ToString();
            SkillsBar.transform.Find("Plants").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Plants / 20f;

            // 动物 (Animals)
            SkillsBar.transform.Find("Animals").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Animals.ToString();
            SkillsBar.transform.Find("Animals").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Animals / 20f;

            // 手工 (Crafting)
            SkillsBar.transform.Find("Crafting").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Crafting.ToString();
            SkillsBar.transform.Find("Crafting").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Crafting / 20f;

            // 艺术 (Artistic)
            SkillsBar.transform.Find("Artistic").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Artistic.ToString();
            SkillsBar.transform.Find("Artistic").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Artistic / 20f;

            // 医疗 (Medical)
            SkillsBar.transform.Find("Medical").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Medical.ToString();
            SkillsBar.transform.Find("Medical").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Medical / 20f;

            // 社交 (Social)
            SkillsBar.transform.Find("Social").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Social.ToString();
            SkillsBar.transform.Find("Social").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Social / 20f;

            // 智识 (Intellectual)
            SkillsBar.transform.Find("Intellectual").transform.Find("Value").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Skills.Intellectual.ToString();
            SkillsBar.transform.Find("Intellectual").transform.Find("bar").GetComponent<Image>().fillAmount = BaStudentUI.Skills.Intellectual / 20f;
        }

        public static void lordStudentPawninfo(Pawn pawn,StudentData studentDataref)
        {
            tracker = GetTracker();
            HumanIntPropertyComp humanIntProperty = pawn.GetComp<HumanIntPropertyComp>();
            StudentSave studentSave = StudentRosterUtility.GetStudentSave(tracker, studentDataref.DefName);
            if (studentSave != null)
            {
                float lv = studentSave.StudentLv;
                pawnUtils.SetStudentLv(pawn, lv);
                humanIntProperty.SetValue(studentSave.StudentExtra);
                foreach (SkillRecord record in pawn.skills.skills)
                {
                    record.levelInt = studentSave.SkillXPs[record.def.defName];
                }
            }
        }

        void SetActiveAbilitySet()
        {
            GameObject Abilityinfo = mainInfo.transform.Find("StudentInfo").transform.Find("Abilityinfo").gameObject;
            GameObject Ability1 = mainInfo.transform.Find("StudentInfo").transform.Find("ActiveAbilitySet").transform.Find("Ability1").gameObject;
            Vector2 petPos = new Vector2(50, -50);
            if (BaStudentUI.Ability1 == null)
            {
                Ability1.SetActive(false);
            }
            else
            {
                ActiveInfoShow activeInfo = Ability1.transform.Find("Button").GetComponent<ActiveInfoShow>();
                if (activeInfo == null)
                {
                    activeInfo = Ability1.transform.Find("Button").gameObject.AddComponent<ActiveInfoShow>();
                }
                activeInfo.Abilityinfo = Abilityinfo;
                activeInfo.BaStudentUI = BaStudentUI;
                activeInfo.penter = Ability1;
                activeInfo.ID = "A1";
                Image back = Ability1.transform.Find("back").GetComponent<Image>();
                back.sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.Ability1.AbilityImagePath));
                back.rectTransform.anchoredPosition = petPos + new Vector2(BaStudentUI.Ability1.offSetX, BaStudentUI.Ability1.offSetY);
                Ability1.transform.Find("AbilityType").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Ability1.AbilityTypeText;

            }

            GameObject Ability2 = mainInfo.transform.Find("StudentInfo").transform.Find("ActiveAbilitySet").transform.Find("Ability2").gameObject;
            if (BaStudentUI.Ability2 == null)
            {
                Ability2.SetActive(false);
            }
            else
            {
                ActiveInfoShow activeInfo2 = Ability2.transform.Find("Button").GetComponent<ActiveInfoShow>();
                if (activeInfo2 == null)
                {
                    activeInfo2 = Ability2.transform.Find("Button").gameObject.AddComponent<ActiveInfoShow>();
                }
                activeInfo2.Abilityinfo = Abilityinfo;
                activeInfo2.BaStudentUI = BaStudentUI;
                activeInfo2.penter = Ability2;
                activeInfo2.ID = "A2";
                Image back = Ability2.transform.Find("back").GetComponent<Image>();
                back.sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.Ability2.AbilityImagePath));
                back.rectTransform.anchoredPosition = petPos + new Vector2(BaStudentUI.Ability2.offSetX, BaStudentUI.Ability2.offSetY);
                Ability2.transform.Find("AbilityType").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Ability2.AbilityTypeText;
            }

            // ------------------- 以下是为您补充的代码 -------------------

            // 处理三号技能 (Ability3)
            GameObject Ability3 = mainInfo.transform.Find("StudentInfo").transform.Find("ActiveAbilitySet").transform.Find("Ability3").gameObject;
            if (BaStudentUI.Ability3 == null)
            {
                Ability3.SetActive(false);
            }
            else
            {
                ActiveInfoShow activeInfo3 = Ability3.transform.Find("Button").GetComponent<ActiveInfoShow>();
                if (activeInfo3 == null)
                {
                    activeInfo3 = Ability3.transform.Find("Button").gameObject.AddComponent<ActiveInfoShow>();
                }
                activeInfo3.Abilityinfo = Abilityinfo;
                activeInfo3.BaStudentUI = BaStudentUI;
                activeInfo3.penter = Ability3;
                activeInfo3.ID = "A3";
                Image back = Ability3.transform.Find("back").GetComponent<Image>();
                back.sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.Ability3.AbilityImagePath));
                back.rectTransform.anchoredPosition = petPos + new Vector2(BaStudentUI.Ability3.offSetX, BaStudentUI.Ability3.offSetY);
                Ability3.transform.Find("AbilityType").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Ability3.AbilityTypeText;
            }

            // 处理四号技能 (Ability4)
            GameObject Ability4 = mainInfo.transform.Find("StudentInfo").transform.Find("ActiveAbilitySet").transform.Find("Ability4").gameObject;
            if (BaStudentUI.Ability4 == null)
            {
                Ability4.SetActive(false);
            }
            else
            {
                ActiveInfoShow activeInfo4 = Ability4.transform.Find("Button").GetComponent<ActiveInfoShow>();
                if (activeInfo4 == null)
                {
                    activeInfo4 = Ability4.transform.Find("Button").gameObject.AddComponent<ActiveInfoShow>();
                }
                activeInfo4.Abilityinfo = Abilityinfo;
                activeInfo4.BaStudentUI = BaStudentUI;
                activeInfo4.penter = Ability4;
                activeInfo4.ID = "A4";
                Image back = Ability4.transform.Find("back").GetComponent<Image>();
                back.sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.Ability4.AbilityImagePath));
                back.rectTransform.anchoredPosition = petPos + new Vector2(BaStudentUI.Ability4.offSetX, BaStudentUI.Ability4.offSetY);
                Ability4.transform.Find("AbilityType").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.Ability4.AbilityTypeText;
            }
        }

        void SetWapenUI()
        {
            if (BaStudentUI.WapenUI.WapenTypeText != null)
            {
                mainInfo.transform.Find("StudentInfo").transform.Find("wapen").gameObject.SetActive(true);
                mainInfo.transform.Find("StudentInfo").transform.Find("wapen").transform.Find("wapenImage").GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.WapenUI.WapenUIImagePath));
                mainInfo.transform.Find("StudentInfo").transform.Find("wapen").transform.Find("TypeBack").transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.WapenUI.WapenTypeText;
            }
            else
            {
                mainInfo.transform.Find("StudentInfo").transform.Find("wapen").gameObject.SetActive(false);
            }
        }

        void SetStudentBio()
        {
            if (BaStudentUI.StudentBio.StudentBioName != null)
            {
                GameObject gameObject = StudentBioContent.transform.Find("StudentAvatar").gameObject;
                gameObject.GetComponent<Image>().sprite = BaStudentUI.StudentAvatarSprite;
                gameObject.transform.Find("StudentDesp").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.StudentBio.StudentDesp;
                gameObject.transform.Find("StudentName").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.StudentBio.StudentBioName;
                gameObject.transform.Find("AcademyLogo").GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(BaStudentUI.StudentBio.AcademyLogoPath));
                StudentBioContent.transform.Find("StudentCard").GetComponent<UnityEngine.UI.Text>().text = BaStudentUI.StudentBio.StudentCard;
                LayoutRebuilder.ForceRebuildLayoutImmediate(StudentBioContent.transform.Find("StudentCard").GetComponent<UnityEngine.UI.Text>().rectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(StudentBioContent.transform.Find("StudentAvatar").GetComponent<RectTransform>());
            }
        }
    }
}
