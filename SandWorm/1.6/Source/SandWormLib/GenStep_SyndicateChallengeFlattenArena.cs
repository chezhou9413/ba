using RimWorld;
using Verse;

namespace SandWormLib
{
    // GenStep_SyndicateChallengeFlattenArena 负责在地图内容生成期把辛迪加挑战场的高度与洞穴网格压成开放沙地。
    public sealed class GenStep_SyndicateChallengeFlattenArena : GenStep
    {
        private const float ArenaElevation = 0.2f;

        public override int SeedPart => 642193517;

        // Generate 负责只修改生成期网格数据，避免事后批量销毁山体和建筑造成卡顿。
        public override void Generate(Map map, GenStepParams parms)
        {
            if (map == null)
            {
                return;
            }

            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid caves = MapGenerator.Caves;
            foreach (IntVec3 cell in map.AllCells)
            {
                // 高度低于岩石阈值，后续地形步骤只会按沙漠生物群系生成沙地、软沙或少量土壤。
                elevation[cell] = ArenaElevation;
                // 洞穴网格归零，避免地形步骤把洞穴区域当作岩地处理。
                caves[cell] = 0f;
            }
        }
    }
}
