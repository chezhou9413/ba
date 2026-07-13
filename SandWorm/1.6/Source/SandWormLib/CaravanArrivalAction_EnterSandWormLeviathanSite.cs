using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SandWormLib
{
    public sealed class CaravanArrivalAction_EnterSandWormLeviathanSite : CaravanArrivalAction
    {
        private SandWormLeviathanSite site;

        public override string Label => "SandWorm_Quest_EnterSite".Translate(site.Label);

        public override string ReportString => "CaravanEntering".Translate(site.Label);

        public CaravanArrivalAction_EnterSandWormLeviathanSite()
        {
        }

        public CaravanArrivalAction_EnterSandWormLeviathanSite(SandWormLeviathanSite site)
        {
            this.site = site;
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
        {
            FloatMenuAcceptanceReport report = base.StillValid(caravan, destinationTile);
            if (!report)
            {
                return report;
            }

            if (site != null && site.Tile != destinationTile)
            {
                return false;
            }

            return CanEnter(caravan, site);
        }

        public override void Arrived(Caravan caravan)
        {
            FloatMenuAcceptanceReport report = CanEnter(caravan, site);
            if (!report)
            {
                Messages.Message(report.FailMessage, site, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (!site.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    DoEnter(caravan, site);
                }, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
            }
            else
            {
                DoEnter(caravan, site);
            }
        }

        private static void DoEnter(Caravan caravan, SandWormLeviathanSite site)
        {
            bool hadMap = site.HasMap;
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, SandWormLeviathanSite.LeviathanMapSize, null);
            SandWormQuestUtility.ForceVanillaSandstorm(map);
            if (!hadMap)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            }

            Find.LetterStack.ReceiveLetter(
                "LetterLabelCaravanEnteredMap".Translate(site),
                "LetterCaravanEnteredMap".Translate(caravan.Label, site).CapitalizeFirst(),
                LetterDefOf.NeutralEvent,
                caravan.PawnsListForReading);

            CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: false);
            site.NotifyChallengeEntered();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref site, "site");
        }

        public static FloatMenuAcceptanceReport CanEnter(Caravan caravan, SandWormLeviathanSite site)
        {
            if (site == null || !site.Spawned)
            {
                return false;
            }

            if (site.EnterCooldownBlocksEntering())
            {
                return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(site.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
            }

            if (!SandWormQuestUtility.CaravanHasSandHammer(caravan))
            {
                return FloatMenuAcceptanceReport.WithFailMessage("SandWorm_Quest_NeedSandHammer".Translate());
            }

            return true;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, SandWormLeviathanSite site)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(
                () => CanEnter(caravan, site),
                () => new CaravanArrivalAction_EnterSandWormLeviathanSite(site),
                "SandWorm_Quest_EnterSite".Translate(site.Label),
                caravan,
                site.Tile,
                site);
        }
    }
}
