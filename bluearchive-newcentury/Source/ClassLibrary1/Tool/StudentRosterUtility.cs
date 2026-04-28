using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BANWlLib.Tool
{
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

            return tracker.HaveStudent.FirstOrDefault(s => s != null && s.DefName == defName);
        }

        public static StudentSave GetStudentSave(ManualDataGameComp tracker, string defName)
        {
            if (tracker?.studentSaves == null || string.IsNullOrEmpty(defName))
            {
                return null;
            }

            return tracker.studentSaves.FirstOrDefault(s => s != null && s.DefName == defName);
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

            return tracker.HaveStudent.Any(s =>
                s != null &&
                (s.StudentPawn == pawn || (!string.IsNullOrEmpty(s.DefName) && s.DefName == pawn.def?.defName)));
        }

        public static Pawn FindRuntimeStudentPawn(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return null;
            }

            return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction
                .FirstOrDefault(p => p != null && !p.DestroyedOrNull() && p.def?.defName == defName);
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
    }
}
