using System.Collections.Generic;
using Verse;

namespace BANWlLib.mainUI.pojo
{
    public class StudentSave : IExposable
    {
        public string DefName;
        public float StudentLv = 0f;
        public int StudentLvInt = 0;
        public int StudentExtra = 0;
        public int CurrentStarLevel = 1;
        public Dictionary<string, int> SkillXPs;

        public StudentSave()
        {
            DefName = string.Empty;
            StudentLv = 0f;
            StudentLvInt = 0;
            StudentExtra = 0;
            CurrentStarLevel = 1;
            SkillXPs = new Dictionary<string, int>();
        }

        public StudentSave(string defName, float studentLv, int studentLvInt, int studentExtra, int currentStarLevel, Dictionary<string, int> skillXPs)
        {
            DefName = defName ?? string.Empty;
            StudentLv = studentLv;
            StudentLvInt = studentLvInt;
            StudentExtra = studentExtra;
            CurrentStarLevel = currentStarLevel;
            SkillXPs = skillXPs ?? new Dictionary<string, int>();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref DefName, "DefName", string.Empty);
            Scribe_Values.Look(ref StudentLv, "StudentLv", 0f);
            Scribe_Values.Look(ref StudentLvInt, "StudentLvInt", 0);
            Scribe_Values.Look(ref StudentExtra, "StudentExtra", 0);
            Scribe_Values.Look(ref CurrentStarLevel, "CurrentStarLevel", 1);
            Scribe_Collections.Look(ref SkillXPs, "SkillXPs", LookMode.Def, LookMode.Value);

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
