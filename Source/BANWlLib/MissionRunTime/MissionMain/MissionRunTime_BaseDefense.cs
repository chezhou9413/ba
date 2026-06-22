using BANWlLib.BaDef;
using BANWlLib.Dev;
using newpro;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BANWlLib.MissionRunTime.MissionMain
{
    public class MissionRunTime_BaseDefense : BaMissionRunTimeAction
    {
        public int getLastTick()
        {
            if (def?.missionRunTimeDef?.enemySpawnPoints == null || def.missionRunTimeDef.enemySpawnPoints.Count == 0)
            {
                return 0;
            }
            return def.missionRunTimeDef.enemySpawnPoints.Max(p => p.SpawnTick);
        }
        public override void Tick()
        {
            base.Tick();
            enemyPawns.RemoveAll(p => p == null || p.Downed || p.Dead || p.Destroyed);
            foreach (EnemySpawnPoint esp in def.missionRunTimeDef.enemySpawnPoints)
            {
                if (ticksPassed == esp.SpawnTick)
                {
                    List<Pawn> pawns = PawnDropHelper.SpawEnemyForkind(map, esp);
                    enemyPawns.AddRange(pawns);
                }
            }
        }

        protected override bool CheckSuccess()
        {
            base.CheckSuccess();
            if (ticksPassed > getLastTick() && enemyPawns.Count == 0)
            {
                return true;
            }
            return false;
        }
        protected override bool CheckFail()
        {
            if (missionPawns.Count < 1)
            {
                Find.LetterStack.ReceiveLetter(
    label: "任务失败",
    text: "任务失败，已强制返回殖民地",
    textLetterDef: LetterDefOf.NeutralEvent,
    lookTargets: null // 明确告诉编译器这是 lookTargets
);
                return true;
            }
            if (ticksPassed > def.missionRunTimeDef.TimeEndTick)
            {
                Find.LetterStack.ReceiveLetter(
    label: "任务失败",
    text: "任务失败，已强制返回殖民地",
    textLetterDef: LetterDefOf.NeutralEvent,
    lookTargets: null
);
                return true;
            }
            base.CheckFail();
            return false;
        }
        protected override void GiveRewards()
        {
            List<Thing> things = new List<Thing>();
            Map rewardMap = Find.Maps.FirstOrDefault(m => m.IsPlayerHome && m != this.map) ?? Find.Maps.FirstOrDefault(m => m.IsPlayerHome);
            foreach (MissionReward missionReward in def.Reward)
            {
                int count = missionReward.count;
                Thing rewardThing = PawnDropHelper.DropProp(rewardMap, missionReward.thingDef, count);
                if (rewardThing != null)
                {
                    things.Add(rewardThing);
                }
            }
            MissionDebug.CompleteMissionInternal(def);
            Find.LetterStack.ReceiveLetter("任务完成", "奖励已发放", LetterDefOf.PositiveEvent, things);
        }
    }
}
