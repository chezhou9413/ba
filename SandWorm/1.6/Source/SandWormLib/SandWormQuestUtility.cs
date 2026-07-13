using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormQuestUtility 负责集中处理沙虫委托、世界据点、天气切换和奖励投放等共享逻辑。
    public static class SandWormQuestUtility
    {
        private const string VanillaSandstormDefName = "Sandstorm";
        private const string AbnormalSandstormDefName = "SandWorm_AbnormalSandstorm";
        private const string LeviathanEntranceSongDefName = "SandWorm_LeviathanEntranceSong";

        public static void ForceVanillaSandstorm(Map map)
        {
            ForceWeather(map, VanillaSandstormDefName);
        }

        // ClearAbnormalSandstormAfterKill 负责在沙虫击杀或挑战中断后移除异常天气并恢复晴天。
        public static void ClearAbnormalSandstormAfterKill(Map map)
        {
            ForceClearWeather(map);
            map?.GetComponent<SandWormDustStormMapComponent>()?.DestroyInstanceImmediate();
        }

        // ForceClearWeather 负责把挑战地图天气恢复为普通晴天，彻底结束沙暴效果。
        private static void ForceClearWeather(Map map)
        {
            if (map?.weatherManager == null)
            {
                return;
            }

            map.weatherManager.TransitionTo(WeatherDefOf.Clear);
        }

        // StopLeviathanEntranceSong 负责在沙虫击杀后停止专属 BGM，避免等待清图期间继续循环播放。
        public static void StopLeviathanEntranceSong()
        {
            MusicManagerPlay manager = Find.MusicManagerPlay;
            if (manager == null)
            {
                return;
            }

            SongDef entranceSongDef = DefDatabase<SongDef>.GetNamedSilentFail(LeviathanEntranceSongDefName);
            if (entranceSongDef == null || manager.CurrentSong != entranceSongDef)
            {
                return;
            }

            manager.Stop();
            manager.ScheduleNewSong();
        }

        public static void ForceAbnormalSandstorm(Map map)
        {
            ForceWeather(map, AbnormalSandstormDefName);
        }

        private static void ForceWeather(Map map, string weatherDefName)
        {
            if (map?.weatherManager == null)
            {
                return;
            }

            WeatherDef weatherDef = DefDatabase<WeatherDef>.GetNamedSilentFail(weatherDefName);
            if (weatherDef == null)
            {
                Log.Warning("[SandWorm] Missing weather def: " + weatherDefName);
                return;
            }

            map.weatherManager.TransitionTo(weatherDef);
        }

        public static bool HasBlockingLeviathanQuest()
        {
            return Find.QuestManager.QuestsListForReading.Any(delegate(Quest quest)
            {
                return quest.tags != null
                    && quest.tags.Contains(SandWormQuestDefs.LeviathanQuestTag)
                    && (quest.State == QuestState.NotYetAccepted
                        || quest.State == QuestState.Ongoing
                        || quest.State == QuestState.EndedSuccess);
            });
        }

        public static void NotifyLeviathanKilled(WorldObject site)
        {
            Current.Game.GetComponent<SandWormQuestGameComponent>()?.UnlockSyndicateButton();
            Find.LetterStack.ReceiveLetter(
                "SandWorm_Quest_LeviathanKilled_Label".Translate(),
                "SandWorm_Quest_LeviathanKilled_Text".Translate(),
                LetterDefOf.PositiveEvent,
                site);

            Find.SignalManager.SendSignal(new Signal(SandWormQuestDefs.LeviathanKilledSignal, site.Named("SUBJECT")));
            EndLeviathanQuestSuccess();
            Current.Game.GetComponent<SandWormQuestGameComponent>()?.ScheduleLuciferiumReward();
        }

        // IsSyndicateButtonUnlocked 负责判断底栏辛迪加按钮是否应该在当前存档中显示。
        public static bool IsSyndicateButtonUnlocked()
        {
            SandWormQuestGameComponent component = Current.Game?.GetComponent<SandWormQuestGameComponent>();
            if (component != null && component.SyndicateButtonUnlocked())
            {
                return true;
            }

            return Find.QuestManager?.QuestsListForReading.Any(delegate(Quest quest)
            {
                return quest.tags != null
                    && quest.tags.Contains(SandWormQuestDefs.LeviathanQuestTag)
                    && quest.State == QuestState.EndedSuccess;
            }) == true;
        }

        public static void EndLeviathanQuestSuccess()
        {
            foreach (Quest quest in Find.QuestManager.QuestsListForReading)
            {
                if (quest.tags != null && quest.tags.Contains(SandWormQuestDefs.LeviathanQuestTag) && quest.State != QuestState.EndedSuccess)
                {
                    quest.End(QuestEndOutcome.Success, sendLetter: false);
                }
            }
        }

        public static bool TryDropLuciferiumReward()
        {
            Map homeMap = Find.AnyPlayerHomeMap;
            if (homeMap == null)
            {
                return false;
            }

            ThingDef luciferiumDef = DefDatabase<ThingDef>.GetNamedSilentFail("Luciferium");
            if (luciferiumDef == null)
            {
                return false;
            }

            Thing luciferium = ThingMaker.MakeThing(luciferiumDef);
            luciferium.stackCount = 200;
            IntVec3 dropSpot = DropCellFinder.TradeDropSpot(homeMap);
            DropPodUtility.DropThingsNear(dropSpot, homeMap, new List<Thing> { luciferium }, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true);
            Find.LetterStack.ReceiveLetter(
                "SandWorm_Quest_LuciferiumReward_Label".Translate(),
                "SandWorm_Quest_LuciferiumReward_Text".Translate(),
                LetterDefOf.PositiveEvent,
                new TargetInfo(dropSpot, homeMap));
            return true;
        }

        // TryDropRetrySandHammer 在挑战中断后向主殖民地补发沙锤并提示玩家可重新挑战。
        public static bool TryDropRetrySandHammer(WorldObject site)
        {
            Map homeMap = Find.AnyPlayerHomeMap;
            if (homeMap == null)
            {
                return false;
            }

            List<Thing> rewards = MakeSandHammerReward().ToList();
            if (rewards.Count == 0)
            {
                return false;
            }

            IntVec3 dropSpot = DropCellFinder.TradeDropSpot(homeMap);
            DropPodUtility.DropThingsNear(dropSpot, homeMap, rewards, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true);
            LookTargets lookTargets = site != null ? new LookTargets(site) : new LookTargets(new TargetInfo(dropSpot, homeMap));
            Find.LetterStack.ReceiveLetter(
                "SandWorm_Quest_RetrySandHammer_Label".Translate(),
                "SandWorm_Quest_RetrySandHammer_Text".Translate(),
                LetterDefOf.NeutralEvent,
                lookTargets);
            return true;
        }

        public static bool TryCreateLeviathanQuest()
        {
            Map homeMap = Find.AnyPlayerHomeMap;
            if (homeMap == null)
            {
                return false;
            }

            if (!CanFindDesertQuestTile())
            {
                return false;
            }

            QuestScriptDef questDef = DefDatabase<QuestScriptDef>.GetNamedSilentFail("SandWorm_LeviathanQuest");
            if (questDef == null)
            {
                return false;
            }

            Slate slate = new Slate();
            slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(homeMap));
            slate.Set("map", homeMap);
            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate);
            if (quest != null && !quest.hidden)
            {
                QuestUtility.SendLetterQuestAvailable(quest, "SandWorm_Quest_Available_Label".Translate());
                return true;
            }

            return false;
        }

        public static bool CanFindDesertQuestTile()
        {
            return TryFindDesertQuestTile(out PlanetTile _);
        }

        public static bool TryFindDesertQuestTile(out PlanetTile tile)
        {
            Map sourceMap = Find.AnyPlayerHomeMap;
            System.Predicate<PlanetTile> validator = IsValidDesertQuestTile;
            if (sourceMap != null && sourceMap.Tile.Valid && TileFinder.TryFindNewSiteTile(out tile, sourceMap.Tile, 8, 36, allowCaravans: true, validator: validator))
            {
                return true;
            }

            if (TileFinder.TryFindNewSiteTile(out tile, 8, 36, allowCaravans: true, validator: validator))
            {
                return true;
            }

            if (TryFindAnyValidDesertQuestTile(out tile))
            {
                return true;
            }

            return TryCreateFallbackDesertQuestTile(out tile);
        }

        private static bool TryFindAnyValidDesertQuestTile(out PlanetTile tile)
        {
            List<PlanetTile> candidates = new List<PlanetTile>();
            PlanetLayer layer = Find.WorldGrid.Surface;
            for (int i = 0; i < layer.TilesCount; i++)
            {
                PlanetTile candidate = new PlanetTile(i, layer);
                if (IsValidDesertQuestTile(candidate))
                {
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count > 0)
            {
                tile = candidates.RandomElement();
                return true;
            }

            tile = PlanetTile.Invalid;
            return false;
        }

        private static bool TryCreateFallbackDesertQuestTile(out PlanetTile tile)
        {
            BiomeDef desertDef = DefDatabase<BiomeDef>.GetNamedSilentFail("Desert");
            if (desertDef == null)
            {
                tile = PlanetTile.Invalid;
                return false;
            }

            List<PlanetTile> candidates = new List<PlanetTile>();
            PlanetLayer layer = Find.WorldGrid.Surface;
            for (int i = 0; i < layer.TilesCount; i++)
            {
                PlanetTile candidate = new PlanetTile(i, layer);
                if (CanConvertToFallbackDesert(candidate))
                {
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count == 0)
            {
                tile = PlanetTile.Invalid;
                return false;
            }

            tile = candidates.RandomElement();
            Tile worldTile = tile.Tile;
            worldTile.PrimaryBiome = desertDef;
            worldTile.temperature = Mathf.Max(worldTile.temperature, 28f);
            worldTile.rainfall = Mathf.Min(worldTile.rainfall, 120f);
            worldTile.swampiness = 0f;
            Log.Message("[SandWorm] No natural valid desert tile was found; converted fallback world tile " + tile + " to desert for the leviathan site.");
            return true;
        }

        private static bool CanConvertToFallbackDesert(PlanetTile tile)
        {
            if (!tile.Valid || Find.WorldObjects.AnyWorldObjectAt(tile))
            {
                return false;
            }

            Tile worldTile = tile.Tile;
            if (worldTile == null || worldTile.WaterCovered || worldTile.PrimaryBiome?.isWaterBiome == true)
            {
                return false;
            }

            return !IsCoastalTile(tile) && !IsImpassableMountainTile(tile);
        }

        public static bool IsValidDesertQuestTile(PlanetTile tile)
        {
            if (!tile.Valid || Find.WorldObjects.AnyWorldObjectAt(tile))
            {
                return false;
            }

            BiomeDef biome = tile.Tile.PrimaryBiome;
            if (biome == null)
            {
                return false;
            }

            return (biome.defName == "Desert" || biome.defName == "ExtremeDesert")
                && !IsCoastalTile(tile)
                && !IsImpassableMountainTile(tile);
        }

        private static bool IsCoastalTile(PlanetTile tile)
        {
            if (tile.Tile.WaterCovered || tile.Tile.PrimaryBiome?.isWaterBiome == true)
            {
                return true;
            }

            List<PlanetTile> neighbors = new List<PlanetTile>();
            Find.WorldGrid.GetTileNeighbors(tile, neighbors);
            for (int i = 0; i < neighbors.Count; i++)
            {
                Tile neighborTile = neighbors[i].Tile;
                if (neighborTile.WaterCovered || neighborTile.PrimaryBiome?.isWaterBiome == true)
                {
                    return true;
                }
            }

            return false;
        }

        // IsImpassableMountainTile 负责过滤世界地图上 caravan 无法通行的高山格，避免沙虫据点刷到不可抵达位置。
        private static bool IsImpassableMountainTile(PlanetTile tile)
        {
            Tile worldTile = tile.Tile;
            return worldTile != null && worldTile.hilliness == Hilliness.Impassable;
        }

        public static IEnumerable<Thing> MakeSandHammerReward()
        {
            ThingDef sandHammerDef = DefDatabase<ThingDef>.GetNamedSilentFail(SandWormQuestDefs.SandHammerDefName);
            if (sandHammerDef == null)
            {
                yield break;
            }

            Thing building = ThingMaker.MakeThing(sandHammerDef);
            Thing minified = MinifyUtility.MakeMinified(building);
            minified.stackCount = 1;
            yield return minified;
        }

        public static bool CaravanHasSandHammer(Caravan caravan)
        {
            if (caravan == null)
            {
                return false;
            }

            ThingDef sandHammerDef = DefDatabase<ThingDef>.GetNamedSilentFail(SandWormQuestDefs.SandHammerDefName);
            if (sandHammerDef == null)
            {
                return false;
            }

            foreach (Thing thing in caravan.AllThings)
            {
                if (thing == null)
                {
                    continue;
                }

                if (thing.def == sandHammerDef)
                {
                    return true;
                }

                if (thing is MinifiedThing minifiedThing && minifiedThing.InnerThing?.def == sandHammerDef)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
