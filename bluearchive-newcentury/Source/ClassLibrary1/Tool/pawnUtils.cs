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
    public static class pawnUtils
    {
        private static HediffDef hediffDef;
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

        public static bool IsAtFullHealth_IgnoreBenign(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
            {
                return false;
            }
            return pawn.health.hediffSet.hediffs.All(hediff => !hediff.def.isBad);
        }

        public static void setStudentSave(Pawn __instance, ManualDataGameComp tracker)
        {
            StudentSave studentSave = StudentRosterUtility.GetStudentSave(tracker, __instance.def.defName);
            StudentData studentData = StudentRosterUtility.GetStudentData(tracker, __instance.def.defName);
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

        public static StudentData PawnGetStudentData(Pawn pawn) {
            ManualDataGameComp tracker = StudentRosterUtility.GetTracker();
            return StudentRosterUtility.GetStudentData(tracker, pawn?.def?.defName);
        }
    }
}
