using BANWlLib.BaDef;
using BANWlLib.KindStats;
using BANWlLib.mainUI.Mission.GameComp;
using BANWlLib.mainUI.Mission.MonoComp;
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

        //判断当前 Pawn 是否是已拥有且仍然存活的学生，负责给任务和殖民者列表排除学生本体。
        public static bool IsStudentPawn(ManualDataGameComp tracker, Pawn pawn)
        {
            if (tracker?.HaveStudent == null || pawn == null || pawn.DestroyedOrNull() || pawn.Dead)
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
                bool pawnStillValid = !resolvedPawn.DestroyedOrNull() && !resolvedPawn.Dead;

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

        //绑定学生 Pawn，负责同步出击状态和当前等级缓存。
        public static void BindStudentPawn(StudentData studentData, Pawn pawn)
        {
            if (studentData == null)
            {
                return;
            }

            studentData.StudentPawn = pawn;
            studentData.isGoing = pawn != null && !pawn.DestroyedOrNull() && !pawn.Dead;

            if (studentData.isGoing)
            {
                int level = pawnUtils.getStudentLv(pawn);
                if (level > 0)
                {
                    studentData.StudentLv = level;
                }
            }
        }

        //清除学生 Pawn 运行时引用，负责把学生恢复为未出击状态。
        public static void ClearStudentPawn(StudentData studentData)
        {
            if (studentData == null)
            {
                return;
            }

            studentData.StudentPawn = null;
            studentData.isGoing = false;
        }

        //获取当前所有存活学生 Pawn，负责给任务选择界面过滤普通殖民者。
        public static HashSet<Pawn> GetRuntimeStudentPawnSet(ManualDataGameComp tracker)
        {
            HashSet<Pawn> pawns = new HashSet<Pawn>();
            if (tracker?.HaveStudent == null)
            {
                return pawns;
            }

            foreach (StudentData studentData in tracker.HaveStudent)
            {
                if (studentData?.StudentPawn != null && !studentData.StudentPawn.DestroyedOrNull() && !studentData.StudentPawn.Dead)
                {
                    pawns.Add(studentData.StudentPawn);
                }
            }

            return pawns;
        }

        //处理学生真正死亡后的名册状态，负责保留养成存档并移除已拥有和可出击状态。
        public static void MarkStudentDeadAndUnowned(Pawn pawn)
        {
            ManualDataGameComp tracker = GetTracker();
            if (tracker?.HaveStudent == null || pawn == null)
            {
                return;
            }

            string studentId = StudentIdentityUtility.GetStudentId(pawn);
            if (string.IsNullOrEmpty(studentId))
            {
                Log.Error("[BANW] 学生死亡清册失败，无法解析学生身份：" + pawn.LabelShort);
                return;
            }

            pawnUtils.setStudentSave(pawn, tracker);

            tracker.HaveStudent.RemoveAll(student =>
                student != null &&
                (student.StudentPawn == pawn ||
                 StudentIdentityUtility.GetStudentId(student.DefName) == studentId));

            tracker.StudentCollect?.RemoveAll(defName => StudentIdentityUtility.GetStudentId(defName) == studentId);
            RemoveDeadStudentFromMissionSelection(pawn, studentId);
        }

        //清理任务编队中的死亡学生，负责防止旧编队绕过名册继续出击。
        private static void RemoveDeadStudentFromMissionSelection(Pawn pawn, string studentId)
        {
            GameComp_TaskQuest quest = Current.Game?.GetComponent<GameComp_TaskQuest>();
            if (quest == null)
            {
                return;
            }

            quest.NoDie?.RemoveAll(p => p == pawn || p == null || p.DestroyedOrNull() || p.Dead);
            quest.selectDataList?.RemoveAll(data => IsDeadStudentSelection(data, pawn, studentId));
        }

        //判断任务选择数据是否指向死亡学生，负责同时处理 Pawn 引用和学生身份引用。
        private static bool IsDeadStudentSelection(selectData data, Pawn pawn, string studentId)
        {
            if (data == null)
            {
                return true;
            }

            if (data.Pawn != null)
            {
                return data.Pawn == pawn || data.Pawn.DestroyedOrNull() || data.Pawn.Dead;
            }

            string selectedStudentId = data.StudentId ?? StudentIdentityUtility.GetStudentId(data.studentDef);
            return !string.IsNullOrEmpty(selectedStudentId) &&
                   StudentIdentityUtility.GetStudentId(selectedStudentId) == studentId;
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
