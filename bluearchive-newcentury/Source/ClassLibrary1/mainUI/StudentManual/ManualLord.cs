using BANWlLib.BaDef;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual.MonoComp;
using BANWlLib.Tool;
using MyCoolMusicMod;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.StudentManual
{
    public static class ManualLord
    {
        public static ManualDataGameComp tracker;
        public static void lord()
        {
            tracker = Current.Game.GetComponent<ManualDataGameComp>();
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
            BindReturnAndStart();
            StudentDetailsLord.LordStudentDetail();
        }

        public static void BindReturnAndStart()
        {
            ManualMapData.StudentManual = UiMapData.mainUI.transform.Find("cangku").gameObject;
            UiMapData.mainUI.transform.Find("Buttom").transform.Find("cangku").GetComponent<Button>().onClick.AddListener(() =>
            {
                foreach (StudentData studentData in tracker.HaveStudent)
                {
                    if (studentData.StudentPawn != null)
                    {
                        studentData.StudentLv = pawnUtils.getStudentLv(studentData.StudentPawn);
                    }
                }
                ExecuteManualUI();
            });
            lordStudentDef();
            lordPrefab();
            lordStudentlist();
            ManualMapData.ManualScrollView.AddComponent<StudentResetList>();
            ManualMapData.StudentManual.transform.Find("cangkulizt").transform.Find("typeButtm").gameObject.AddComponent<StudentTypeButtomController>();
            lordPawnRef();
            addSelectDetails();
        }

        public static void lordStudentlist()
        {
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();
            foreach (BaStudentUI baStudentUI in ManualMapData.studentUIList)
            {

                GameObject studentItem = GameObject.Instantiate(ManualMapData.StudentListOBJ);
                studentItem.transform.SetParent(ManualMapData.ManualScrollView.transform, false);
                StudentListShow studentListShow = studentItem.AddComponent<StudentListShow>();
                studentListShow.BaStudentUI = baStudentUI;
                studentListShow.studentData = StudentRosterUtility.GetStudentData(tracker, baStudentUI.RaceDefName);
            }
            StudentManualEvents.resetUIlistRefresh();
        }

        public static void lordPrefab()
        {
            ManualMapData.StudentListOBJ = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/StudentList.prefab");
            ManualMapData.StarOBJ = UiMapData.bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/UI/star.prefab");
            ManualMapData.ManualScrollView = UiMapData.mainUI.transform.Find("cangku").transform.Find("cangkulizt").transform.Find("Scroll View").transform.Find("Viewport").transform.Find("Content").gameObject;
        }
        public static void ExecuteManualUI()
        {
            ManualMapData.isOpenManual = true;
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
            ManualMapData.StudentManual.SetActive(true);
            StudentManualEvents.RaiseRefresh();
            StudentManualEvents.resetUIlistRefresh();
            MonoComp_BackButton.instance.setNewObj(ManualMapData.StudentManual, "bgm4");
            LoopBGMManager.switchUiBgm("bgm4");
        }

        public static void lordStudentDef()
        {
            ManualMapData.StudentList = DefDatabase<BaStudentRaceDef>.AllDefsListForReading;

            if (ManualMapData.StudentList.NullOrEmpty())
            {
                return;
            }

            foreach (BaStudentRaceDef BaStudentRaceDef in ManualMapData.StudentList)
            {
                if (BaStudentRaceDef == null)
                {
                    continue;
                }

                // 检查BaStudentUI是否为null
                if (BaStudentRaceDef.BaStudentUI == null)
                {
                    continue;
                }

                // 如果StudentName为"Auto"，则使用label作为学生名称
                if (BaStudentRaceDef.BaStudentUI.StudentName == "Auto")
                {
                    BaStudentRaceDef.BaStudentUI.StudentName = BaStudentRaceDef.label;
                }
                BaStudentRaceDef.BaStudentUI.RaceDefName = BaStudentRaceDef.defName;
                ManualMapData.studentUIList.Add(BaStudentRaceDef.BaStudentUI);
            }


            // 输出所有已加载的学生
            foreach (BaStudentUI baStudentUI in ManualMapData.studentUIList)
            {
                baStudentUI.StudentAvatarPath = UiMapData.modRootPath + "/Common/Textures/" + baStudentUI.StudentAvatar + ".png";
                baStudentUI.StudentAvatarSprite = imgcvT2d.LoadSpriteFromFile(baStudentUI.StudentAvatarPath);
                if (baStudentUI.StudentAvatarSprite == null)
                {
                    Log.Warning("学生头像文件不存在: " + baStudentUI.StudentAvatarPath);
                    continue;
                }
            }
        }
        //此函数用于赋值缺失引用的pawn读档时调用
        public static void lordPawnRef()
        {
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
        }

        public static void addSelectDetails()
        {
            GameObject SelectButtom = UiMapData.mainUI.transform.Find("cangku").transform.Find("Details").transform.Find("Background").transform.Find("mainInfo").transform.Find("SelectButtom").gameObject;
            GameObject StudentInfo = UiMapData.mainUI.transform.Find("cangku").transform.Find("Details").transform.Find("Background").transform.Find("mainInfo").transform.Find("StudentInfo").gameObject;
            GameObject StudentBio = UiMapData.mainUI.transform.Find("cangku").transform.Find("Details").transform.Find("Background").transform.Find("mainInfo").transform.Find("StudentBio").gameObject;
            StudentInfoType studentInfoType = SelectButtom.AddComponent<StudentInfoType>();
            studentInfoType.StudentBio = StudentBio;
            studentInfoType.StudentInfo = StudentInfo;
        }
    }
}
