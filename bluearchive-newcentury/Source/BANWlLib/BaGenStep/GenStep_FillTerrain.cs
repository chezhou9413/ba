using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.BaGenStep
{
    public class GenStep_FillTerrain : GenStep
    {
        // 可以通过XML传入的地形DefName，如果为空则默认沙地
        public TerrainDef terrainDef;

        // 无参构造函数 - 必需用于XML反序列化
        public GenStep_FillTerrain()
        {
        }

        public override int SeedPart => 123456789;

        public override void Generate(Map map, GenStepParams parms)
        {
            TerrainDef targetTerrain = terrainDef ?? TerrainDefOf.Sand;

            //填充地形。
            foreach (IntVec3 c in map.AllCells)
            {
                map.terrainGrid.SetTerrain(c, targetTerrain);
            }

            //清除非玩家物品，避免其他 GenStep 留下墙体。
            //此步骤必须在建筑生成前执行。
            List<Thing> toDestroy = new List<Thing>();
            foreach (Thing t in map.listerThings.AllThings)
            {
                if (t.def.destroyable && t.Faction != Faction.OfPlayer)
                {
                    toDestroy.Add(t);
                }
            }
            foreach (Thing t in toDestroy)
            {
                t.Destroy(DestroyMode.Vanish);
            }

            //清除遗迹屋顶。
            foreach (IntVec3 c in map.AllCells)
            {
                if (map.roofGrid.RoofAt(c) != null)
                {
                    map.roofGrid.SetRoof(c, null);
                }
            }
        }
    }
}
