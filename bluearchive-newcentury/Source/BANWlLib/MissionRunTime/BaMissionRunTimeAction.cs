using BANWlLib.BaDef;
using BANWlLib.BANWMap;
using BANWlLib.Drop;
using BANWlLib.mainUI.Mission.GameComp;
using BANWlLib.mainUI.Mission.MonoComp;
using newpro;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BANWlLib.MissionRunTime
{
    public class BaMissionRunTimeAction : IExposable, ILoadReferenceable
    {
        public BaMissionNode def;
        public int uniqueID;
        public MissionState state = MissionState.Active;

        public IntVec3 spawnPosition;
        public Map map;
        public int ticksPassed;

        public List<Pawn> missionPawns = new List<Pawn>();
        public List<Pawn> enemyPawns = new List<Pawn>();
        public List<Pawn> defensePawn = new List<Pawn>();

        private BaDrop baDrop;
        public GameComp_TaskQuest comp_TaskQuest;

        public BaMissionRunTimeAction() { }

        public void removePawnfoNodid(Pawn pawn)
        {
            if (comp_TaskQuest == null)
            {
                comp_TaskQuest = Current.Game.GetComponent<GameComp_TaskQuest>();
            }
            comp_TaskQuest.NoDie.Remove(pawn);
        }

        public int GetRemainingSeconds()
        {
            int totalTicks = def.missionRunTimeDef.TimeEndTick;
            int left = totalTicks - this.ticksPassed;
            if (left < 0) left = 0;
            return left / 60;
        }

        public BaMissionRunTimeAction(BaMissionNode def, int id)
        {
            this.def = def;
            this.uniqueID = id;
        }

        public virtual void OnStart()
        {
        }

        public virtual void Tick()
        {
            if (missionPawns == null) return;

            ticksPassed++;
            for (int i = missionPawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = missionPawns[i];
                if (pawn != null && pawn.Spawned && pawn.Map == this.map && pawn.Downed)
                {
                    removePawnfoNodid(pawn);
                    JumpToCurrMap(pawn);
                    missionPawns.Remove(pawn);
                }
            }

            if (state == MissionState.Active)
            {
                if (CheckSuccess())
                {
                    End(MissionState.Success);
                }
                else if (CheckFail())
                {
                    End(MissionState.Failed);
                }
            }
        }

        public virtual void End(MissionState outcome)
        {
            this.state = outcome;
            GameComp_TaskQuest quest = Current.Game.GetComponent<GameComp_TaskQuest>();
            quest.isStarMission = false;
            if (outcome == MissionState.Success)
            {
                GiveRewards();
            }

            if (map != null)
            {
                SpawnPawnWithEffectManager spawnManager = Current.Game?.GetComponent<SpawnPawnWithEffectManager>();
                spawnManager?.CancelTasksForMap(map);

                List<Pawn> pawnsToRescue = new List<Pawn>();
                if (missionPawns != null) pawnsToRescue.AddRange(missionPawns);

                foreach (Pawn pawn in pawnsToRescue)
                {
                    if (pawn != null && !pawn.Dead && pawn.Map == map)
                    {
                        removePawnfoNodid(pawn);
                        JumpToCurrMap(pawn);
                    }
                }

                CreateMap.DestroyPocketMap(map);
                map = null;
            }
        }

        public void colseMission()
        {
            End(MissionState.Failed);
        }

        private void JumpToCurrMap(Pawn pawn)
        {
            if (baDrop == null)
            {
                baDrop = DefDatabase<BaDrop>.AllDefs.FirstOrDefault();
            }

            Map targetMap = Find.CurrentMap;
            if (targetMap == null || targetMap == this.map || !targetMap.IsPlayerHome)
            {
                targetMap = Find.Maps.FirstOrDefault(m => m.IsPlayerHome && m != this.map);
                if (targetMap == null)
                {
                    targetMap = Find.Maps.FirstOrDefault(m => m != this.map);
                }
            }

            if (targetMap == null)
            {
                Log.Error($"[BaMission] Could not find a safe map to return pawn {pawn.LabelShort} to.");
                return;
            }

            MapComponent_EveryFrame comp = targetMap.GetComponent<MapComponent_EveryFrame>();
            IntVec3 intVec = IntVec3.Zero;

            if (comp != null && comp.dropCell != IntVec3.Zero)
            {
                intVec = comp.dropCell;
            }
            else
            {
                intVec = DropCellFinder.RandomDropSpot(targetMap);
            }

            PawnDropHelper.JumpForPawnOfBaEff(targetMap, pawn, intVec);
        }

        protected virtual bool CheckSuccess() => false;
        protected virtual bool CheckFail() => false;
        protected virtual void GiveRewards() { }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref uniqueID, "uniqueID");
            Scribe_Values.Look(ref state, "state");
            Scribe_Values.Look(ref spawnPosition, "spawnPosition");
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref ticksPassed, "ticksPassed");
            Scribe_Collections.Look(ref missionPawns, "missionPawns", LookMode.Reference);
            Scribe_Collections.Look(ref enemyPawns, "enemyPawns", LookMode.Reference);
            Scribe_Collections.Look(ref defensePawn, "defensePawn", LookMode.Reference);
            Scribe_Defs.Look(ref baDrop, "baDrop");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (missionPawns == null) missionPawns = new List<Pawn>();
                if (enemyPawns == null) enemyPawns = new List<Pawn>();
                if (defensePawn == null) defensePawn = new List<Pawn>();
            }
        }

        public string GetUniqueLoadID() => "BaMissionRunTime_" + uniqueID;
    }

    public enum MissionState { Active, Success, Failed }
}
