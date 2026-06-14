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
    /// <summary>
    /// Pawn 学生数据工具，负责读写学生等级、保存数据和运行时学生名册。
    /// </summary>
    public static class pawnUtils
    {
        private static HediffDef hediffDef;

        /// <summary>
        /// 获取学生等级 Hediff 当前阶段。
        /// </summary>
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

        /// <summary>
        /// 判断 Pawn 是否没有负面 Hediff。
        /// </summary>
        public static bool IsAtFullHealth_IgnoreBenign(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
            {
                return false;
            }
            return pawn.health.hediffSet.hediffs.All(hediff => !hediff.def.isBad);
        }

        /// <summary>
        /// 保存学生 Pawn 的等级、额外经验和技能数据。
        /// </summary>
        public static void setStudentSave(Pawn __instance, ManualDataGameComp tracker)
        {
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
                tracker.studentSaves.Add(studentSave = new StudentSave(studentData.DefName, pawnUtils.getStudentLvSeverity(__instance), pawnUtils.getStudentLv(__instance), humanIntProperty.CustomIntValue, SkillXPs));
            }
            else if (studentData != null)
            {
                studentSave.StudentLv = pawnUtils.getStudentLvSeverity(__instance);
                studentSave.StudentLvInt = pawnUtils.getStudentLv(__instance);
                studentSave.StudentExtra = humanIntProperty.CustomIntValue;
                studentSave.SkillXPs = SkillXPs;
            }
        }

        /// <summary>
        /// 获取学生等级 Hediff 的严重度数值。
        /// </summary>
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

        /// <summary>
        /// 设置学生等级 Hediff 的严重度数值。
        /// </summary>
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

        /// <summary>
        /// 通过 Pawn 获取对应的学生名册数据。
        /// </summary>
        public static StudentData PawnGetStudentData(Pawn pawn) {
            ManualDataGameComp tracker = StudentRosterUtility.GetTracker();
            return StudentRosterUtility.GetStudentData(tracker, StudentIdentityUtility.GetStudentId(pawn));
        }
    }
}
