using System.Collections.Generic;
using Verse;

namespace BANWlLib.mainUI.pojo
{
    //学生养成保存数据，负责记录学生等级、额外经验、星级和技能等级。
    public class StudentSave : IExposable
    {
        public string DefName;
        public float StudentLv = 0f;
        public int StudentLvInt = 0;
        public int StudentExtra = 0;
        public int CurrentStarLevel = 1;
        public Dictionary<string, int> SkillXPs;

        //构造空学生保存数据，负责满足 Scribe 深度序列化创建实例的要求。
        public StudentSave()
        {
            DefName = string.Empty;
            StudentLv = 0f;
            StudentLvInt = 0;
            StudentExtra = 0;
            CurrentStarLevel = 1;
            SkillXPs = new Dictionary<string, int>();
        }

        //按当前学生养成状态构造保存数据，负责写入召回时记录的等级和技能信息。
        public StudentSave(string defName, float studentLv, int studentLvInt, int studentExtra, int currentStarLevel, Dictionary<string, int> skillXPs)
        {
            DefName = defName ?? string.Empty;
            StudentLv = studentLv;
            StudentLvInt = studentLvInt;
            StudentExtra = studentExtra;
            CurrentStarLevel = currentStarLevel;
            SkillXPs = skillXPs ?? new Dictionary<string, int>();
        }

        //保存和读取学生养成数据，负责让召回后的学生再次出击时恢复成长状态。
        public void ExposeData()
        {
            Scribe_Values.Look(ref DefName, "DefName", string.Empty);
            Scribe_Values.Look(ref StudentLv, "StudentLv", 0f);
            Scribe_Values.Look(ref StudentLvInt, "StudentLvInt", 0);
            Scribe_Values.Look(ref StudentExtra, "StudentExtra", 0);
            Scribe_Values.Look(ref CurrentStarLevel, "CurrentStarLevel", 1);
            Scribe_Collections.Look(ref SkillXPs, "SkillXPs", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (SkillXPs == null)
                {
                    SkillXPs = new Dictionary<string, int>();
                }
                if (CurrentStarLevel <= 0)
                {
                    CurrentStarLevel = 1;
                }
            }
        }
    }
}
