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
        /// <summary>
        /// 获取当前游戏的学生名册组件。
        /// </summary>
        public static ManualDataGameComp GetTracker()
        {
            return Current.Game?.GetComponent<ManualDataGameComp>();
        }

        /// <summary>
        /// 按当前学生身份获取学生数据。
        /// </summary>
        public static StudentData GetStudentData(string defName)
        {
            return GetStudentData(GetTracker(), defName);
        }

        /// <summary>
        /// 在指定名册中按当前学生身份获取学生数据。
        /// </summary>
        public static StudentData GetStudentData(ManualDataGameComp tracker, string defName)
        {
            if (tracker?.HaveStudent == null || string.IsNullOrEmpty(defName))
            {
                return null;
            }

            string studentId = StudentIdentityUtility.GetStudentId(defName);
            return tracker.HaveStudent.FirstOrDefault(s => s != null && s.DefName == studentId);
        }

        /// <summary>
        /// 在指定名册中按当前学生身份获取学生保存数据。
        /// </summary>
        public static StudentSave GetStudentSave(ManualDataGameComp tracker, string defName)
        {
            if (tracker?.studentSaves == null || string.IsNullOrEmpty(defName))
            {
                return null;
            }

            string studentId = StudentIdentityUtility.GetStudentId(defName);
            return tracker.studentSaves.FirstOrDefault(s => s != null && s.DefName == studentId);
        }

        /// <summary>
        /// 判断指定学生身份是否已存在于名册中。
        /// </summary>
        public static bool IsStudentDef(ManualDataGameComp tracker, string defName)
        {
            return GetStudentData(tracker, defName) != null;
        }

        /// <summary>
        /// 判断 Pawn 是否属于学生名册，负责按当前学生 ID 比较。
        /// </summary>
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

        /// <summary>
        /// 按学生身份查找当前地图和队伍中的运行时 Pawn。
        /// </summary>
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

        /// <summary>
        /// 同步全部学生的运行时 Pawn 引用。
        /// </summary>
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

        /// <summary>
        /// 同步单个学生的运行时 Pawn 引用和等级缓存。
        /// </summary>
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

        /// <summary>
        /// 将学生数据绑定到运行时 Pawn，并同步等级缓存。
        /// </summary>
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

        /// <summary>
        /// 清理学生数据上的运行时 Pawn 引用。
        /// </summary>
        public static void ClearStudentPawn(StudentData studentData)
        {
            if (studentData == null)
            {
                return;
            }

            studentData.StudentPawn = null;
            studentData.isGoing = false;
        }

        /// <summary>
        /// 获取名册中全部有效学生 Pawn 的集合。
        /// </summary>
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
