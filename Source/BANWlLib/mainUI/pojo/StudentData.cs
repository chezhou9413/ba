using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.mainUI.pojo
{
    /// <summary>
    /// 学生名册数据，负责保存学生身份、出击状态、运行时 Pawn 引用和等级缓存。
    /// </summary>
    public class StudentData : IExposable
    {
        // 学生对应的 Def 名称
        public string DefName;
        // 是否正在出击
        public bool isGoing;
        // 地图中的 Pawn 引用（可为空）
        public Pawn StudentPawn;

        public int StudentLv = 1;
        /// <summary>
        /// 构造空学生数据，负责满足 Scribe 深度序列化创建实例的要求。
        /// </summary>
        public StudentData()
        {
            DefName = string.Empty;
            isGoing = false;
            StudentPawn = null;
            StudentLv = 1;
        }

        /// <summary>
        /// 按当前学生 ID 构造名册数据。
        /// </summary>
        public StudentData(string defName)
        {
            DefName = defName ?? string.Empty;
            isGoing = false;
            StudentPawn = null;
            StudentLv = 1;
        }

        /// <summary>
        /// 保存和读取学生名册数据。
        /// </summary>
        public void ExposeData()
        {
            // 基础值类型保存
            Scribe_Values.Look(ref DefName, "DefName", string.Empty);
            Scribe_Values.Look(ref isGoing, "isGoing", false);
            Scribe_Values.Look(ref StudentLv, "StudentLv",1);
            // 引用类型（Pawn）使用引用保存
            Scribe_References.Look(ref StudentPawn, "StudentPawn");
        }

        /// <summary>
        /// 输出学生名册调试文本，负责展示身份、出击状态和运行时 Pawn 信息。
        /// </summary>
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
