using System.Collections.Generic;
using Verse;

namespace BANWlLib.mainUI.pojo
{
    /// <summary>
    /// 学生持久化数据，负责保存学生等级、额外经验和技能等级。
    /// </summary>
    public class StudentSave : IExposable
    {
        // 学生对应的 studentId。
        public string DefName;

        public float StudentLv = 0f;
        public int StudentLvInt = 0;
        public int StudentExtra = 0;
        public Dictionary<string, int> SkillXPs;

        /// <summary>
        /// 构造空学生保存数据，负责满足 Scribe 深度序列化创建实例的要求。
        /// </summary>
        public StudentSave()
        {
            DefName = string.Empty;
            StudentLv = 0f;
            StudentLvInt = 0;
            StudentExtra = 0;
            SkillXPs = new Dictionary<string, int>();
        }

        /// <summary>
        /// 按当前学生 ID 和数值构造保存数据。
        /// </summary>
        public StudentSave(string defName, float studentLv, int studentLvInt, int studentExtra, Dictionary<string, int> skillXPs)
        {
            DefName = defName ?? string.Empty;
            StudentLv = studentLv;
            StudentLvInt = studentLvInt;
            StudentExtra = studentExtra;
            SkillXPs = skillXPs ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// 保存和读取学生持久化数据。
        /// </summary>
        public void ExposeData()
        {
            Scribe_Values.Look(ref DefName, "DefName", string.Empty);
            Scribe_Values.Look(ref StudentLv, "StudentLv", 0f);
            Scribe_Values.Look(ref StudentLvInt, "StudentLvInt", 0);
            Scribe_Values.Look(ref StudentExtra, "StudentExtra", 0);
            Scribe_Collections.Look(ref SkillXPs, "SkillXPs", LookMode.Def, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (SkillXPs == null)
                {
                    SkillXPs = new Dictionary<string, int>();
                }
            }
        }
    }
}
