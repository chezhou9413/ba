using BANWlLib.BaDef;
using BANWlLib.mainUI.ManualUI;
using BANWlLib.Tool;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.StudentManual.MonoComp;
using Verse;

namespace BANWlLib.mainUI.StudentManual
{
    //管理学生详情页显示与数据绑定，切换学生时释放上一页图片引用。
    public static class StudentDetailsLord
    {
        public static StudentDetailsController studentDetails;
        private static ManualDataGameComp tracker;

        //查找详情页节点并挂接数据显示控制器。
        public static void LordStudentDetail()
        {
            ManualMapData.StudentDetailOBJ = ManualMapData.StudentManual.transform.Find("Details").gameObject;
            studentDetails = ManualMapData.StudentDetailOBJ.AddComponent<StudentDetailsController>();
        }

        //释放上一页引用后激活并更新数据，图片在布局完成后按当前学生路径加载。
        public static void ShowStudentDetail(BaStudentUI baStudent)
        {
            ManualMapData.isOpenDetail = true;
            ManualMapData.StudentDetailOBJ.SetActive(false);
            BAManualUIImageLoader.ClearDetailImages();
            if (tracker == null)
            {
                tracker = Current.Game.GetComponent<ManualDataGameComp>();
            }
            if (studentDetails == null)
            {
                Log.Error("[学生手册] 详情页控制器尚未初始化。");
                return;
            }
            ManualMapData.StudentDetailOBJ.SetActive(true);
            MonoComp_BackButton.instance.setNewObj(ManualMapData.StudentDetailOBJ, null);
            studentDetails.BaStudentUI = baStudent;
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
            studentDetails.studentData = StudentRosterUtility.GetStudentData(tracker, baStudent.StudentId);
            studentDetails.setData();
        }

        //关闭详情页后回收已经归还引用的图片。
        public static void CloseStudentDetail()
        {
            ManualMapData.StudentDetailOBJ.SetActive(false);
            BAManualUIImageLoader.ClearDetailImages();
            ManualMapData.isOpenDetail = false;
        }
    }
}
