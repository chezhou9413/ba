using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SandWormLib
{
    public sealed class QuestPart_LeviathanAccept : QuestPart
    {
        public override void PreQuestAccept()
        {
            base.PreQuestAccept();

            if (!SandWormQuestUtility.TryFindDesertQuestTile(out PlanetTile tile))
            {
                Messages.Message("SandWorm_Quest_NoDesertTile".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            SandWormLeviathanSite site = (SandWormLeviathanSite)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed(SandWormQuestDefs.DesertSiteDefName));
            site.Tile = tile;
            site.SetFaction(Faction.OfPlayer);
            if (site.questTags == null)
            {
                site.questTags = new List<string>();
            }

            site.questTags.Add(SandWormQuestDefs.LeviathanQuestTag);
            Find.WorldObjects.Add(site);

            List<GlobalTargetInfo> questLookTargets = new List<GlobalTargetInfo>
            {
                new GlobalTargetInfo(site)
            };

            Map homeMap = Find.AnyPlayerHomeMap;
            if (homeMap != null)
            {
                List<Thing> rewards = SandWormQuestUtility.MakeSandHammerReward().ToList();
                if (rewards.Count > 0)
                {
                    IntVec3 dropSpot = DropCellFinder.TradeDropSpot(homeMap);
                    DropPodUtility.DropThingsNear(dropSpot, homeMap, rewards, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true);
                    GlobalTargetInfo hammerDropTarget = new GlobalTargetInfo(dropSpot, homeMap);
                    questLookTargets.Add(hammerDropTarget);
                    Find.LetterStack.ReceiveLetter(
                        "SandWorm_Quest_SandHammerArrived_Label".Translate(),
                        "SandWorm_Quest_SandHammerArrived_Text".Translate(),
                        LetterDefOf.PositiveEvent,
                        new LookTargets(hammerDropTarget),
                        quest: quest);
                }
            }

            QuestPart_LookTargets lookTargetPart = new QuestPart_LookTargets();
            lookTargetPart.targets.AddRange(questLookTargets);
            quest.AddPart(lookTargetPart);

            Find.LetterStack.ReceiveLetter(
                "SandWorm_Quest_SiteCreated_Label".Translate(),
                "SandWorm_Quest_SiteCreated_Text".Translate(site.LabelCap),
                LetterDefOf.NeutralEvent,
                new LookTargets(questLookTargets),
                quest: quest);

            Find.SignalManager.SendSignal(new Signal(SandWormQuestDefs.LeviathanAcceptedSignal, site.Named("SUBJECT")));
        }
    }
}
