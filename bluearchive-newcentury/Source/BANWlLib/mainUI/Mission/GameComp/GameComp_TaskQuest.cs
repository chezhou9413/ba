using BANWlLib.BaDef;
using BANWlLib.mainUI.Mission.MonoComp;
using BANWlLib.mainUI.pojo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.mainUI.Mission.GameComp
{
   public class GameComp_TaskQuest : GameComponent
    {
        public List<BaMissionNode> MissionQuest = new List<BaMissionNode>();
        public List<selectData> selectDataList = new List<selectData>();
        public List<Pawn> NoDie = new List<Pawn>();
        public bool isStarMission = false;
        public GameComp_TaskQuest(Game game)
        {
            // 构造函数
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isStarMission, "isStarMission",false);
            Scribe_Collections.Look(ref MissionQuest, "MissionQuest", LookMode.Def);
            Scribe_Collections.Look(ref selectDataList, "selectDataList", LookMode.Deep);
            Scribe_Collections.Look(ref NoDie, "NoDie", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (MissionQuest == null)
                {
                    MissionQuest = new List<BaMissionNode>();
                }
                if(selectDataList == null)
                {
                    selectDataList = new List<selectData>();
                }
            }
        }
    }
}
