using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.BaDef
{
    public class BaMissionNode:Def
    {
        public float oder;
        public string MissionID;
        public string MissionTitle;
        public BaMissionType MissionType;
        public BaMissionNode UnlockedOn;
        public string MissionDes;
        public List<string> MissionTarget = new List<string>();
        public List<MissionReward> Reward = new List<MissionReward>();
        public List<EnemyList> EnemyList = new List<EnemyList>();
        public BaMapDef missionMapDef;
        public BaMissionRunTime missionRunTimeDef;
    }
    public enum BaMissionQuality
    {
        Low,
        Medium,
        High,
        Epic
    }
    public class MissionReward
    {
        public ThingDef thingDef;
        public BaMissionQuality quality;
        public int count;
    }

    public class EnemyList
    {
        public PawnKindDef pawnKindDef;
        public string tagPath1;
        public string tagPath2;
    }
}
