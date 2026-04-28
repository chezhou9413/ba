using BANWlLib.BaDef;
using BANWlLib.MissionRunTime;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.BANWMap
{
    public static class CreateMap
    {
        public static void DestroyPocketMap(Map pocketMap)
        {
            if (pocketMap == null) return;
            if (pocketMap.IsPlayerHome)
            {
                return;
            }
            Map returnMap = Find.Maps.FirstOrDefault(m =>
                m.Tile == pocketMap.Tile &&
                m.IsPlayerHome &&
                m != pocketMap
            );
            if (Find.CurrentMap == pocketMap)
            {
                if (returnMap != null)
                {
                    Current.Game.CurrentMap = returnMap;
                    Find.CameraDriver.JumpToCurrentMapLoc(returnMap.Center);
                }
                else
                {
                    CameraJumper.TryJump(pocketMap.Tile);
                }
            }
            if (pocketMap.Parent != null)
            {
                pocketMap.Parent.Destroy();
            }
        }
        public static Map CreateSandPocketMap(BaMapDef baMapDef)
        {
            Map sourceMap = Find.CurrentMap;
            if (sourceMap == null)
            {
                return null;
            }
            IntVec3 mapSize = new IntVec3(baMapDef.mapSizeX, 1, baMapDef.mapSizeZ);
            WorldObjectDef mapParentDef = DefDatabase<WorldObjectDef>.GetNamedSilentFail("BANW_PocketMapParent");
            if (mapParentDef == null) mapParentDef = WorldObjectDefOf.Site;
            MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(mapParentDef);
            mapParent.Tile = sourceMap.Tile;
            mapParent.SetFaction(Faction.OfPlayer);
            Find.WorldObjects.Add(mapParent);
            MapGeneratorDef genDef = DefDatabase<MapGeneratorDef>.GetNamed("CZ_SandPocketMap");
            Map newMap = MapGenerator.GenerateMap(mapSize, mapParent, genDef, null, null);
            setMapTerrain(newMap, baMapDef.baseTerrainDef);
            setMapBuilder(newMap, baMapDef.buildPlaces);
            BaMissionUIComponent baMissionUIComponent = new BaMissionUIComponent(newMap);
            newMap.components.Add(baMissionUIComponent);
            if (newMap != null)
            {
                Current.Game.CurrentMap = newMap;
                Find.CameraDriver.JumpToCurrentMapLoc(newMap.Center);
            }
            return newMap;
        }

        public static void setMapTerrain(Map map,TerrainDef terrainDef)
        {
            TerrainDef targetTerrain = terrainDef ?? TerrainDefOf.Voidmetal;
            foreach (IntVec3 c in map.AllCells)
            {
                // 设置地形
                map.terrainGrid.SetTerrain(c, targetTerrain);
            }
        }

        public static void setMapBuilder(Map map, List<BuildPlace> BuildPlace)
        {
            if (BuildPlace == null) return;

            foreach (BuildPlace place in BuildPlace)
            {
                if (place.buildingDef == null)
                {
                    continue;
                }
                ThingDef stuffToUse = place.stuffDef;
                if (!place.buildingDef.MadeFromStuff)
                {
                    stuffToUse = null;
                }
                Thing thing = ThingMaker.MakeThing(place.buildingDef, stuffToUse);
                if (thing.def.CanHaveFaction)
                {
                    thing.SetFaction(Faction.OfPlayer);
                }
                if (place.position.InBounds(map))
                {
                    Rot4 rotation = ConvertRot(place.rot);
                    GenSpawn.Spawn(thing, place.position, map, rotation);
                }
            }
        }

        private static Rot4 ConvertRot(Rot4Info info)
        {
            switch (info)
            {
                case Rot4Info.North: return Rot4.North;
                case Rot4Info.East: return Rot4.East;
                case Rot4Info.South: return Rot4.South;
                case Rot4Info.West: return Rot4.West;
                default: return Rot4.North;
            }
        }
    }
}
