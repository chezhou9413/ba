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

            // 1. 填充地形
            foreach (IntVec3 c in map.AllCells)
            {
                map.terrainGrid.SetTerrain(c, targetTerrain);
            }

            // 2. 暴力清除所有非玩家物品 (防止隐形的 GenStep 生成了墙或屋顶)
            // 注意：这步操作要小心，确保是在你的建筑生成之前运行
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

            // 3. 清除屋顶 (遗迹通常带屋顶)
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
