using BANWlLib;
using BANWlLib.BaDef;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.Tool
{
    // Pawn 学生数据工具，负责读写学生等级、保存数据和运行时学生名册。
    public static class pawnUtils
    {
        private static HediffDef hediffDef;

        // 获取学生当前等级，负责从等级 Hediff 的阶段换算显示等级。
        public static int getStudentLv(Pawn pawn)
        {
            int a = -1;
            if (hediffDef == null)
            {
                hediffDef = HediffDef.Named("BANW_LevelTrait");
            }
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff != null)
            {
                a = (hediff.CurStageIndex + 1);
            }
            return a;
        }

        // 判断学生是否可按健康状态召回，负责忽略学生等级、星级这类养成显示状态。
        public static bool IsAtFullHealth_IgnoreBenign(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
            {
                return false;
            }
            return pawn.health.hediffSet.hediffs.All(IsRecallAllowedHediff);
        }

        // 判断单个 Hediff 是否允许召回，负责把养成状态和非负面状态排除在异常健康之外。
        private static bool IsRecallAllowedHediff(Hediff hediff)
        {
            if (hediff?.def == null)
            {
                return true;
            }

            if (!hediff.def.isBad)
            {
                return true;
            }

            string defName = hediff.def.defName;
            return defName == "BANW_LevelTrait" ||
                   defName == "DamageReductionStatus" ||
                   defName == "BANW_StarGrowthDisplayStatus";
        }

        //保存学生养成数据，负责记录等级严重度、等级、额外经验和技能等级，不覆盖已保存星级。
        public static void setStudentSave(Pawn __instance, ManualDataGameComp tracker)
        {
            if (__instance == null || tracker?.studentSaves == null)
            {
                return;
            }

            string studentId = StudentIdentityUtility.GetStudentId(__instance);
            StudentSave studentSave = StudentRosterUtility.GetStudentSave(tracker, studentId);
            StudentData studentData = StudentRosterUtility.GetStudentData(tracker, studentId);
            HumanIntPropertyComp humanIntProperty = __instance.GetComp<HumanIntPropertyComp>();
            if (humanIntProperty == null)
            {
                // Log.Warning("找不到humanIntProperty组件"); // 注释：普通log输出，屏蔽
            }
            Dictionary<string, int> SkillXPs = new Dictionary<string, int>();
            foreach (SkillRecord record in __instance.skills.skills)
            {
                SkillXPs[record.def.defName] = record.levelInt;
            }
            if (studentSave == null && studentData != null && humanIntProperty != null)
            {
                tracker.studentSaves.Add(studentSave = new StudentSave(studentData.DefName, pawnUtils.getStudentLvSeverity(__instance), pawnUtils.getStudentLv(__instance), humanIntProperty.CustomIntValue, StudentRosterUtility.GetDefaultStarLevel(__instance), SkillXPs));
            }
            else if (studentSave != null && studentData != null && humanIntProperty != null)
            {
                studentSave.StudentLv = pawnUtils.getStudentLvSeverity(__instance);
                studentSave.StudentLvInt = pawnUtils.getStudentLv(__instance);
                studentSave.StudentExtra = humanIntProperty.CustomIntValue;
                studentSave.SkillXPs = SkillXPs;
            }
        }

        // 获取学生等级严重度，负责保存等级经验进度。
        public static float getStudentLvSeverity(Pawn pawn)
        {
            float a = -1;
            if (hediffDef == null)
            {
                hediffDef = HediffDef.Named("BANW_LevelTrait");
            }
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff != null)
            {
                a = hediff.Severity;
            }
            return a;
        }

        // 设置学生等级严重度，负责在重新出击时恢复等级经验进度。
        public static void SetStudentLv(Pawn pawn,float value)
        {
            if (hediffDef == null)
            {
                hediffDef = HediffDef.Named("BANW_LevelTrait");
            }
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff != null)
            {
                hediff.Severity = value;
            }
        }

        // 获取 Pawn 对应的学生数据，负责通过当前名册定位 UI 学生定义。
        public static StudentData PawnGetStudentData(Pawn pawn) {
            ManualDataGameComp tracker = StudentRosterUtility.GetTracker();
            return StudentRosterUtility.GetStudentData(tracker, StudentIdentityUtility.GetStudentId(pawn));
        }
    }
}
