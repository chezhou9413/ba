using BANWlLib.MissionRunTime;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BANWlLib.BaDef
{
    public class BaMissionRunTime:Def
    {
        public IntVec3 Spawnposition;
        public int TimeEndTick;
        public Type missionClass = typeof(BaMissionRunTimeAction);
        public List<EnemySpawnPoint> enemySpawnPoints = new List<EnemySpawnPoint>();
    }

    public class EnemySpawnPoint
    {
        public EffecterDef EffecterDef;
        public IntVec3 SpawnPosition;
        public int SpawnTick;
        public int DelayTicks;
        public AiType AiType = AiType.Hunt;
        public float DefendRadius = 10f;
        public bool CannotFlee = true;
        public FactionDef factionDef;
        public List<KindSetting> KindSettings;
    }

    public class KindSetting
    {
        public PawnKindDef pawnKindDef;
        public int count;
    }

    public enum AiType
    {
        Defend,
        Hunt
    }
}
