using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace SandWormLib
{
    // SandWormLeviathanSite 负责维护沙虫挑战世界点、挑战地图生命周期和重复挑战状态。
    public sealed class SandWormLeviathanSite : MapParent
    {
        public static readonly IntVec3 LeviathanMapSize = new IntVec3(250, 1, 250);
        private const int RemoveMapDelayTicks = 30000;
        private const int ChallengeStateCheckIntervalTicks = 250;

        private bool leviathanKilled;
        private int removeMapTick = -1;
        private bool challengeStarted;
        private bool leviathanSpawned;
        private bool retryHammerPending;
        private bool removeMapForRetry;

        public override MapGeneratorDef MapGeneratorDef => MapGeneratorDefOf.Encounter;

        public override string Label => "SandWorm_Quest_Site_Label".Translate();

        public bool LeviathanKilled => leviathanKilled;
        public bool ChallengeStarted => challengeStarted;
        public bool LeviathanSpawned => leviathanSpawned;
        public bool RetryHammerPending => retryHammerPending;

        // NotifyChallengeEntered 记录玩家已经进入挑战地图，后续可监控撤离或团灭。
        public void NotifyChallengeEntered()
        {
            if (leviathanKilled)
            {
                return;
            }

            challengeStarted = true;
            retryHammerPending = false;
            removeMapForRetry = false;
        }

        // NotifyLeviathanSpawned 记录沙锤已经消耗，本次尝试中断后需要补发新沙锤。
        public void NotifyLeviathanSpawned()
        {
            if (leviathanKilled)
            {
                return;
            }

            challengeStarted = true;
            leviathanSpawned = true;
            retryHammerPending = false;
        }

        public override string GetInspectString()
        {
            string text = "SandWorm_Quest_Site_Inspect".Translate();
            if (leviathanKilled && removeMapTick > Find.TickManager.TicksGame)
            {
                text += "\n" + "SandWorm_Quest_Site_CleanupCountdown".Translate((removeMapTick - Find.TickManager.TicksGame).ToStringTicksToPeriod());
            }
            else if (retryHammerPending)
            {
                text += "\n" + "SandWorm_Quest_Site_RetryPending".Translate();
            }
            return text;
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            if (removeMapForRetry)
            {
                alsoRemoveWorldObject = false;
                return true;
            }

            alsoRemoveWorldObject = true;
            return leviathanKilled && removeMapTick > 0 && Find.TickManager.TicksGame >= removeMapTick;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref leviathanKilled, "leviathanKilled", defaultValue: false);
            Scribe_Values.Look(ref removeMapTick, "removeMapTick", -1);
            Scribe_Values.Look(ref challengeStarted, "challengeStarted", defaultValue: false);
            Scribe_Values.Look(ref leviathanSpawned, "leviathanSpawned", defaultValue: false);
            Scribe_Values.Look(ref retryHammerPending, "retryHammerPending", defaultValue: false);
            Scribe_Values.Look(ref removeMapForRetry, "removeMapForRetry", defaultValue: false);
        }

        // TickInterval 定期检查挑战地图是否已经没有可继续作战的玩家自由殖民者。
        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (retryHammerPending)
            {
                TryDeliverPendingRetryHammer();
            }

            if (!leviathanKilled && challengeStarted && HasMap && this.IsHashIntervalTick(ChallengeStateCheckIntervalTicks))
            {
                TryHandleInterruptedChallenge();
            }
        }

        // NotifyLeviathanKilled 标记挑战成功，安排地图和世界点在短延迟后清理。
        public void NotifyLeviathanKilled()
        {
            if (leviathanKilled)
            {
                return;
            }

            leviathanKilled = true;
            challengeStarted = false;
            leviathanSpawned = false;
            retryHammerPending = false;
            removeMapForRetry = false;
            removeMapTick = Find.TickManager.TicksGame + RemoveMapDelayTicks;
            if (HasMap)
            {
                SandWormQuestUtility.ClearAbnormalSandstormAfterKill(Map);
            }
            SandWormQuestUtility.StopLeviathanEntranceSong();
        }

        // TryHandleInterruptedChallenge 在玩家撤离或团灭时重置本次挑战，并补发沙锤。
        private void TryHandleInterruptedChallenge()
        {
            if (HasLivingFreeColonistAbleToContinue(Map))
            {
                return;
            }

            retryHammerPending = true;
            challengeStarted = false;
            leviathanSpawned = false;
            removeMapForRetry = true;
            SandWormQuestUtility.ClearAbnormalSandstormAfterKill(Map);
            SandWormQuestUtility.StopLeviathanEntranceSong();
            TryDeliverPendingRetryHammer();
            CheckRemoveMapNow();
        }

        // TryDeliverPendingRetryHammer 尝试补发重试用沙锤；若暂时没有主殖民地，则保留待投放状态。
        private void TryDeliverPendingRetryHammer()
        {
            if (!retryHammerPending)
            {
                return;
            }

            if (SandWormQuestUtility.TryDropRetrySandHammer(this))
            {
                retryHammerPending = false;
            }
        }

        // HasLivingFreeColonistAbleToContinue 判断挑战地图上是否还有未倒地的玩家自由殖民者。
        private static bool HasLivingFreeColonistAbleToContinue(Map map)
        {
            if (map == null)
            {
                return false;
            }

            List<Pawn> pawns = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn != null && !pawn.Destroyed && !pawn.Dead && !pawn.Downed && pawn.Spawned)
                {
                    return true;
                }
            }

            // 除了已刷在地图格子上的自由殖民者，还要检查地图内各种容器/载具/穿梭机里
            // 是否仍然装着可继续作战的玩家 Pawn。
            // 否则当所有人暂时处于穿梭机、载具座位或其他 IThingHolder 内时，
            // FreeColonistsSpawned 会短暂变成空列表，导致挑战被误判为中断。
            List<Thing> allThings = map.listerThings.AllThings;
            for (int i = 0; i < allThings.Count; i++)
            {
                Thing thing = allThings[i];
                if (thing is IThingHolder holder && HolderContainsActivePlayerColonist(holder))
                {
                    return true;
                }

                // 原版穿梭机等运输载具通常把乘员放在 ThingComp（例如 CompTransporter）里，
                // 它们未必让 Thing 本体直接实现 IThingHolder。
                // 因此这里还要把挂在 Thing 上的组件容器也一起递归检查，避免“人都在穿梭机里”
                // 时被误判成地图上已经没有可继续挑战的殖民者。
                if (thing is ThingWithComps thingWithComps)
                {
                    List<ThingComp> comps = thingWithComps.AllComps;
                    for (int compIndex = 0; compIndex < comps.Count; compIndex++)
                    {
                        if (comps[compIndex] is IThingHolder compHolder && HolderContainsActivePlayerColonist(compHolder))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // HolderContainsActivePlayerColonist 负责递归检查一个地图内容器及其子容器中，
        // 是否存在仍可继续挑战的玩家自由殖民者。
        private static bool HolderContainsActivePlayerColonist(IThingHolder holder)
        {
            if (holder == null)
            {
                return false;
            }

            ThingOwner directlyHeldThings = holder.GetDirectlyHeldThings();
            if (directlyHeldThings != null)
            {
                for (int i = 0; i < directlyHeldThings.Count; i++)
                {
                    Thing heldThing = directlyHeldThings[i];
                    if (heldThing is Pawn pawn && IsActivePlayerColonist(pawn))
                    {
                        return true;
                    }
                }
            }

            List<IThingHolder> childHolders = SimplePool<List<IThingHolder>>.Get();
            childHolders.Clear();
            try
            {
                holder.GetChildHolders(childHolders);
                for (int i = 0; i < childHolders.Count; i++)
                {
                    if (HolderContainsActivePlayerColonist(childHolders[i]))
                    {
                        return true;
                    }
                }
            }
            finally
            {
                childHolders.Clear();
                SimplePool<List<IThingHolder>>.Return(childHolders);
            }

            return false;
        }

        // IsActivePlayerColonist 负责统一判断一个 Pawn 是否算作“还能继续挑战”的玩家殖民者。
        private static bool IsActivePlayerColonist(Pawn pawn)
        {
            return pawn != null
                && !pawn.Destroyed
                && !pawn.Dead
                && !pawn.Downed
                && pawn.IsFreeColonist
                && pawn.Faction == Faction.OfPlayer;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(caravan))
            {
                yield return option;
            }

            foreach (FloatMenuOption option in CaravanArrivalAction_EnterSandWormLeviathanSite.GetFloatMenuOptions(caravan, this))
            {
                yield return option;
            }
        }
    }
}
