using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.BaDef
{
    public class BaMapDef : Def
    {
        // 基础地面 (例如全是土地或沙地)
        public TerrainDef baseTerrainDef;
        public int mapSizeX;
        public int mapSizeZ;
        // 建筑列表
        public List<BuildPlace> buildPlaces; // 修正拼写
    }

    // 不需要继承 Def，只是一个数据节点
    public class BuildPlace
    {
        public ThingDef buildingDef;
        public ThingDef stuffDef;      
        public IntVec3 position;       
        public Rot4Info rot;
        public FactionDef factionDef;
    }

    public enum Rot4Info
    {
        North,
        East,
        South,
        West
    }
}