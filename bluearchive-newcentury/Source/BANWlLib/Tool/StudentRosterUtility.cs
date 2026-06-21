using BANWlLib.BaDef;
using BANWlLib.KindStats;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BANWlLib.Tool
{
    /// <summary>
    /// 学生名册工具，负责读取学生存档数据并维护学生 Pawn 的运行时引用。
    /// </summary>
    public static class StudentRosterUtility
    {
        public static ManualDataGameComp GetTracker()
        {
            return Current.Game?.GetComponent<ManualDataGameComp>();
        }

        public static StudentData GetStudentData(string defName)
        {
            return GetStudentData(GetTracker(), defName);
        }

        public static StudentData GetStudentData(ManualDataGameComp tracker, string defName)
        {
            if (tracker?.HaveStudent == null || string.IsNullOrEmpty(defName))
            {
                return null;
            }

            string studentId = StudentIdentityUtility.GetStudentId(defName);
            return tracker.HaveStudent.FirstOrDefault(s => s != null && s.DefName == studentId);
        }

        public static StudentSave GetStudentSave(ManualDataGameComp tracker, string defName)
        {
            if (tracker?.studentSaves == null || string.IsNullOrEmpty(defName))
            {
                return null;
            }

            string studentId = StudentIdentityUtility.GetStudentId(defName);
            return tracker.studentSaves.FirstOrDefault(s => s != null && s.DefName == studentId);
        }

        public static bool IsStudentDef(ManualDataGameComp tracker, string defName)
        {
            return GetStudentData(tracker, defName) != null;
        }

        public static bool IsStudentPawn(ManualDataGameComp tracker, Pawn pawn)
        {
            if (tracker?.HaveStudent == null || pawn == null || pawn.DestroyedOrNull())
            {
                return false;
            }

            string pawnStudentId = StudentIdentityUtility.GetStudentId(pawn);
            return tracker.HaveStudent.Any(s =>
                s != null &&
                (s.StudentPawn == pawn || (!string.IsNullOrEmpty(s.DefName) && s.DefName == pawnStudentId)));
        }

        public static Pawn FindRuntimeStudentPawn(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return null;
            }

            string studentId = StudentIdentityUtility.GetStudentId(defName);
            return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction
                .FirstOrDefault(p => p != null && !p.DestroyedOrNull() && StudentIdentityUtility.GetStudentId(p) == studentId);
        }

        public static void SyncAllStudentRuntimeState(ManualDataGameComp tracker)
        {
            if (tracker?.HaveStudent == null)
            {
                return;
            }

            foreach (StudentData studentData in tracker.HaveStudent)
            {
                SyncStudentRuntimeState(tracker, studentData);
            }
        }

        public static void SyncStudentRuntimeState(ManualDataGameComp tracker, StudentData studentData)
        {
            if (tracker == null || studentData == null || string.IsNullOrEmpty(studentData.DefName))
            {
                return;
            }

            Pawn resolvedPawn = studentData.StudentPawn;
            if (resolvedPawn != null)
            {
                bool pawnStillValid = !resolvedPawn.DestroyedOrNull();

                if (!pawnStillValid)
                {
                    resolvedPawn = null;
                }
            }

            if (resolvedPawn == null)
            {
                resolvedPawn = FindRuntimeStudentPawn(studentData.DefName);
            }

            studentData.StudentPawn = resolvedPawn;
            studentData.isGoing = resolvedPawn != null;

            if (resolvedPawn != null)
            {
                int level = pawnUtils.getStudentLv(resolvedPawn);
                if (level > 0)
                {
                    studentData.StudentLv = level;
                }
            }
        }

        public static void BindStudentPawn(StudentData studentData, Pawn pawn)
        {
            if (studentData == null)
            {
                return;
            }

            studentData.StudentPawn = pawn;
            studentData.isGoing = pawn != null && !pawn.DestroyedOrNull();

            if (studentData.isGoing)
            {
                int level = pawnUtils.getStudentLv(pawn);
                if (level > 0)
                {
                    studentData.StudentLv = level;
                }
            }
        }

        public static void ClearStudentPawn(StudentData studentData)
        {
            if (studentData == null)
            {
                return;
            }

            studentData.StudentPawn = null;
            studentData.isGoing = false;
        }

        public static HashSet<Pawn> GetRuntimeStudentPawnSet(ManualDataGameComp tracker)
        {
            HashSet<Pawn> pawns = new HashSet<Pawn>();
            if (tracker?.HaveStudent == null)
            {
                return pawns;
            }

            foreach (StudentData studentData in tracker.HaveStudent)
            {
                if (studentData?.StudentPawn != null && !studentData.StudentPawn.DestroyedOrNull())
                {
                    pawns.Add(studentData.StudentPawn);
                }
            }

            return pawns;
        }

        public static StudentSave GetOrCreateStudentSave(Pawn pawn)
        {
            ManualDataGameComp tracker = GetTracker();
            if (tracker == null || pawn == null)
            {
                return null;
            }

            string studentId = StudentIdentityUtility.GetStudentId(pawn);
            StudentSave studentSave = GetStudentSave(tracker, studentId);
            if (studentSave != null)
            {
                return studentSave;
            }

            StudentData studentData = GetStudentData(tracker, studentId);
            if (studentData == null)
            {
                return null;
            }

            studentSave = new StudentSave(studentData.DefName, pawnUtils.getStudentLvSeverity(pawn), pawnUtils.getStudentLv(pawn), pawn.GetComp<HumanIntPropertyComp>()?.CustomIntValue ?? 0, GetDefaultStarLevel(pawn), new Dictionary<string, int>());
            tracker.studentSaves.Add(studentSave);
            return studentSave;
        }

        public static int GetDefaultStarLevel(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1;
            }

            BaStudentDef studentDef;
            if (StudentIdentityUtility.TryGetStudentDef(StudentIdentityUtility.GetStudentId(pawn), out studentDef) && studentDef?.baStudentData != null)
            {
                return UnityEngine.Mathf.Max(1, studentDef.baStudentData.StarCont);
            }

            return 1;
        }

        public static int GetCurrentStarLevel(Pawn pawn)
        {
            StudentSave studentSave = GetOrCreateStudentSave(pawn);
            if (studentSave != null && studentSave.CurrentStarLevel > 0)
            {
                return studentSave.CurrentStarLevel;
            }

            return GetDefaultStarLevel(pawn);
        }

        public static void SetCurrentStarLevel(Pawn pawn, int starLevel)
        {
            StudentSave studentSave = GetOrCreateStudentSave(pawn);
            if (studentSave == null)
            {
                return;
            }

            studentSave.CurrentStarLevel = UnityEngine.Mathf.Max(1, starLevel);
            HealthScaleCache.Invalidate(pawn);
        }
    }
}
