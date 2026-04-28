using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.mainUI.pojo
{
    // 学生数据，需支持保存与读取
    public class StudentData : IExposable
    {
        // 学生对应的 Def 名称
        public string DefName;
        // 是否正在出击
        public bool isGoing;
        // 地图中的 Pawn 引用（可为空）
        public Pawn StudentPawn;

        public int StudentLv = 1;
        // 无参构造函数（Deep 序列化要求）
        public StudentData()
        {
            DefName = string.Empty;
            isGoing = false;
            StudentPawn = null;
            StudentLv = 1;
        }

        // 便捷构造
        public StudentData(string defName)
        {
            DefName = defName ?? string.Empty;
            isGoing = false;
            StudentPawn = null;
            StudentLv = 1;
        }

        // 序列化/反序列化实现
        public void ExposeData()
        {
            // 基础值类型保存
            Scribe_Values.Look(ref DefName, "DefName", string.Empty);
            Scribe_Values.Look(ref isGoing, "isGoing", false);
            Scribe_Values.Look(ref StudentLv, "StudentLv",1);
            // 引用类型（Pawn）使用引用保存
            Scribe_References.Look(ref StudentPawn, "StudentPawn");
        }

        public override string ToString()
        {
            string msg = "学生名字：" + DefName;
            msg += "\n是否出击：" + isGoing;
            if (StudentPawn != null)
            {
                msg += "\n角色地图实例化对象：" + StudentPawn.Name;
            }
            else
            {
                msg += "\n角色地图未实例化对象";
            }
            return msg;
        }
    }
}
