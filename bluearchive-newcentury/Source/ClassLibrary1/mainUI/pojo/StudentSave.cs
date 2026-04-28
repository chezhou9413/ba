using BANWlLib.pache;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.mainUI.pojo
{
    // 学生数据，需支持保存与读取
    public class StudentSave : IExposable
    {
        // 学生对应的 Def 名称
        public string DefName;

        public float StudentLv = 0f;
        public int StudentLvInt = 0;
        public int StudentExtra = 0;
        public Dictionary<string, int> SkillXPs;
        // 无参构造函数（Deep 序列化要求）
        public StudentSave()
        {
            DefName = string.Empty;
            StudentLv = 0f;
            StudentLvInt = 0;
            StudentExtra = 0;
            SkillXPs = new Dictionary<string, int>();
        }

        // 便捷构造
        public StudentSave(string defName, float studentLv,int studentLvInt,int studentExtra,Dictionary<string,int> skillXPs)
        {
            DefName = defName;
            StudentLv = studentLv;
            StudentLvInt = studentLvInt;
            StudentExtra = studentExtra;
            SkillXPs = skillXPs;
        }

        // 序列化/反序列化实现
        public void ExposeData()
        {
            // 基础值类型保存
            Scribe_Values.Look(ref DefName, "DefName", string.Empty);
            Scribe_Values.Look(ref StudentLv, "StudentLv", 0f);
            Scribe_Values.Look(ref StudentLvInt, "StudentLvInt", 0);
            Scribe_Values.Look(ref StudentExtra, "StudentExtra", 0);
            Scribe_Collections.Look(ref SkillXPs, "SkillXPs", LookMode.Def, LookMode.Value);
        }

    }
}
