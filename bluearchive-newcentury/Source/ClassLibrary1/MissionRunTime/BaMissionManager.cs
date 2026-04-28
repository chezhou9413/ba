using BANWlLib.BaDef;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.MissionRunTime
{
    public class BaMissionManager : WorldComponent
    {
        public List<BaMissionRunTimeAction> activeMissions = new List<BaMissionRunTimeAction>();
        public List<BaMissionRunTimeAction> tmpMissionsToRemove = new List<BaMissionRunTimeAction>(); // 防止遍历时修改集合

        public BaMissionManager(World world) : base(world) { }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            foreach (var mission in activeMissions)
            {
                if (mission.state == MissionState.Active)
                {
                    mission.Tick();
                }
                else
                {
                    tmpMissionsToRemove.Add(mission);
                }
            }

            // 清理已完成的任务
            if (tmpMissionsToRemove.Count > 0)
            {
                foreach (var m in tmpMissionsToRemove) activeMissions.Remove(m);
                tmpMissionsToRemove.Clear();
            }
        }

        // 添加任务的接口
        public void AddMission(BaMissionRunTimeAction mission)
        {
            activeMissions.Add(mission);
            mission.OnStart();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref activeMissions, "activeMissions", LookMode.Deep);
        }
    }
}
