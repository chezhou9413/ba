using BANWlLib.mainUI.pojo;
using BANWlLib.Tool;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.mainUI.StudentManual
{
    public class ManualDataGameComp : GameComponent
    {
        public List<StudentData> HaveStudent = new List<StudentData>();
        public List<string> StudentCollect = new List<string>();
        public List<StudentSave> studentSaves = new List<StudentSave>();

        public ManualDataGameComp(Game game)
        {
            // 构造函数
        }

        public override void ExposeData()
        {
            base.ExposeData();

            // 1. 保存自定义对象列表，使用 LookMode.Deep，这部分是正确的
            //    前提是 StudentData 类实现了 IExposable 接口
            Scribe_Collections.Look(ref HaveStudent, "HaveStudent", LookMode.Deep);
            Scribe_Collections.Look(ref studentSaves, "studentSaves", LookMode.Deep);
            // 2. 保存字典，键和值都是简单类型，应使用 LookMode.Value
            Scribe_Collections.Look(ref StudentCollect, "StudentCollect");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (HaveStudent == null)
                {
                    HaveStudent = new List<StudentData>();
                }
                if (studentSaves == null)
                {
                    studentSaves = new List<StudentSave>();
                }
                if (StudentCollect == null)
                {
                    StudentCollect = new List<string>();
                }
                StudentRosterUtility.SyncAllStudentRuntimeState(this);
            }
        }
    }
}
