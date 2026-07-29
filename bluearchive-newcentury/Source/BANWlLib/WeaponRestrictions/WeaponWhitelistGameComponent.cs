using RimWorld;
using Verse;

namespace BANWlLib.WeaponRestrictions
{
    // 武器白名单读档组件，负责在游戏完整载入后清理地图 Pawn 的违规装备。
    public sealed class WeaponWhitelistGameComponent : GameComponent
    {
        // 创建读档组件，供 RimWorld 的 GameComponent 系统自动实例化。
        public WeaponWhitelistGameComponent(Game game)
        {
        }

        // 游戏读档完成后扫描已生成 Pawn，并将不在 Kind 白名单内的武器卸到脚下。
        public override void LoadedGame()
        {
            base.LoadedGame();

            var spawnedPawns = PawnsFinder.AllMaps_Spawned;
            for (int i = 0; i < spawnedPawns.Count; i++)
            {
                WeaponWhitelistUtility.DropDisallowedWeapons(spawnedPawns[i]);
            }
        }
    }
}
