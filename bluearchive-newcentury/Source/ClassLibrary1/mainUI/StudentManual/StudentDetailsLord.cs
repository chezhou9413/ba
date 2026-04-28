using BANWlLib.BaDef;
using BANWlLib.Tool;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.StudentManual.MonoComp;
using Verse;

namespace BANWlLib.mainUI.StudentManual
{
    public static class StudentDetailsLord
    {
        public static StudentDetailsController studentDetails;
        private static ManualDataGameComp tracker;

        public static void LordStudentDetail()
        {
            ManualMapData.StudentDetailOBJ = ManualMapData.StudentManual.transform.Find("Details").gameObject;
            studentDetails = ManualMapData.StudentDetailOBJ.AddComponent<StudentDetailsController>();
        }

        public static void ShowStudentDetail(BaStudentUI baStudent)
        {
            ManualMapData.isOpenDetail = true;
            if (tracker == null)
            {
                tracker = Current.Game.GetComponent<ManualDataGameComp>();
            }
            if (studentDetails == null)
            {
                Log.Warning("[StudentDetailsLord] studentDetails is not initialized. Call LordStudentDetail first.");
            }
            ManualMapData.StudentDetailOBJ.SetActive(true);
            MonoComp_BackButton.instance.setNewObj(ManualMapData.StudentDetailOBJ, null);
            studentDetails.BaStudentUI = baStudent;
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
            studentDetails.studentData = StudentRosterUtility.GetStudentData(tracker, baStudent.RaceDefName);
            studentDetails.setData();
        }

        public static void CloseStudentDetail()
        {
            ManualMapData.StudentDetailOBJ.SetActive(false);
            ManualMapData.isOpenDetail = false;
        }
    }
}
