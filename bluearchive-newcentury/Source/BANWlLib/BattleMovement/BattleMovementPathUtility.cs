using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleMovement
{
    // 战斗位移路径工具，负责直线路径、可配置阻挡、范围目标和阵营过滤。
    public static class BattleMovementPathUtility
    {
        // 解析位移终点，负责沿直线扫描并停在第一个阻挡格之前。
        public static IntVec3 ResolveBlockedDestination(Pawn pawn, IntVec3 requestedCell)
        {
            return ResolveBlockedDestination(pawn, requestedCell, false, false);
        }

        // 解析位移终点，负责按穿墙和穿掩体配置沿直线扫描可到达终点。
        public static IntVec3 ResolveBlockedDestination(Pawn pawn, IntVec3 requestedCell, bool canPassWalls, bool canPassCover)
        {
            if (pawn?.Map == null || !requestedCell.IsValid)
            {
                return IntVec3.Invalid;
            }

            IntVec3 lastValid = pawn.Position;
            foreach (IntVec3 cell in LineCells(pawn.Position, requestedCell))
            {
                if (cell == pawn.Position)
                {
                    continue;
                }

                if (CanStandAt(pawn.Map, cell))
                {
                    lastValid = cell;
                    continue;
                }

                if (CanPassBlockedCell(pawn.Map, cell, canPassWalls, canPassCover))
                {
                    continue;
                }

                return lastValid;
            }

            return lastValid;
        }

        // 解析击退终点，负责沿击退方向逐格推进到阻挡前。
        public static IntVec3 ResolveKnockbackDestination(Pawn pawn, Vector3 direction, int distance)
        {
            if (pawn?.Map == null || distance <= 0)
            {
                return IntVec3.Invalid;
            }

            IntVec3 desiredCell = CellInDirection(pawn.Position, direction, distance);
            return ResolveBlockedDestination(pawn, desiredCell);
        }

        // 枚举路径内 Pawn，负责平面冲撞只对路径目标命中一次。
        public static IEnumerable<Pawn> PawnsInPath(Pawn caster, IntVec3 destination, int pathWidth)
        {
            HashSet<int> visited = new HashSet<int>();
            foreach (IntVec3 lineCell in LineCells(caster.Position, destination))
            {
                foreach (IntVec3 cell in WidthCells(caster.Map, lineCell, pathWidth))
                {
                    List<Thing> things = cell.GetThingList(caster.Map);
                    for (int i = 0; i < things.Count; i++)
                    {
                        Pawn pawn = things[i] as Pawn;
                        if (pawn == null || pawn == caster || pawn.Dead || visited.Contains(pawn.thingIDNumber))
                        {
                            continue;
                        }

                        visited.Add(pawn.thingIDNumber);
                        yield return pawn;
                    }
                }
            }
        }

        // 枚举范围内 Pawn，负责落地和瞬移终点范围结算。
        public static IEnumerable<Pawn> PawnsInRadius(Map map, IntVec3 center, float radius)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                List<Thing> things = cell.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    Pawn pawn = things[i] as Pawn;
                    if (pawn != null && !pawn.Dead)
                    {
                        yield return pawn;
                    }
                }
            }
        }

        // 判断 Pawn 是否符合阵营过滤，负责让击退独立于伤害动作配置。
        public static bool CanAffectPawn(Pawn caster, Pawn targetPawn, bool affectHostile, bool affectFriendly)
        {
            if (caster == null || targetPawn == null || targetPawn.Dead)
            {
                return false;
            }

            if (caster.Faction == null || targetPawn.Faction == null)
            {
                return affectHostile;
            }

            if (targetPawn.HostileTo(caster))
            {
                return affectHostile;
            }

            return affectFriendly;
        }

        // 计算方向向量，负责把两个格子转换成水平单位方向。
        public static Vector3 Direction(IntVec3 from, IntVec3 to)
        {
            Vector3 direction = (to - from).ToVector3().Yto0();
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }

            return direction;
        }

        // 判断格子能否站立，负责统一墙体、建筑和不可站立地形阻挡。
        private static bool CanStandAt(Map map, IntVec3 cell)
        {
            return cell.InBounds(map) && cell.Standable(map);
        }

        // 判断不可站立格是否允许穿过，负责区分墙体类阻挡和掩体类阻挡。
        private static bool CanPassBlockedCell(Map map, IntVec3 cell, bool canPassWalls, bool canPassCover)
        {
            if (!cell.InBounds(map))
            {
                return false;
            }

            if (IsCoverBlocker(map, cell))
            {
                return canPassCover;
            }

            if (IsWallBlocker(map, cell))
            {
                return canPassWalls;
            }

            return false;
        }

        // 判断格子是否属于墙体阻挡，负责把墙、封闭建筑和不可通行地形归入穿墙配置。
        private static bool IsWallBlocker(Map map, IntVec3 cell)
        {
            TerrainDef terrain = cell.GetTerrain(map);
            if (terrain != null && terrain.passability == Traversability.Impassable)
            {
                return true;
            }

            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing?.def == null)
                {
                    continue;
                }

                if (thing.def.passability == Traversability.Impassable || thing.def.Fillage == FillCategory.Full)
                {
                    return true;
                }
            }

            return false;
        }

        // 判断格子是否属于掩体阻挡，负责识别沙袋、路障等能提供掩体但不是墙的中间阻挡。
        private static bool IsCoverBlocker(Map map, IntVec3 cell)
        {
            Thing cover = cell.GetCover(map);
            if (cover?.def == null)
            {
                return false;
            }

            return cover.def.Fillage != FillCategory.Full && cover.def.passability != Traversability.Impassable;
        }

        // 生成直线路径格，负责用整数 Bresenham 算法覆盖起点到目标点。
        private static IEnumerable<IntVec3> LineCells(IntVec3 start, IntVec3 end)
        {
            int x0 = start.x;
            int z0 = start.z;
            int x1 = end.x;
            int z1 = end.z;
            int dx = Mathf.Abs(x1 - x0);
            int dz = Mathf.Abs(z1 - z0);
            int sx = x0 < x1 ? 1 : -1;
            int sz = z0 < z1 ? 1 : -1;
            int err = dx - dz;

            while (true)
            {
                yield return new IntVec3(x0, start.y, z0);
                if (x0 == x1 && z0 == z1)
                {
                    break;
                }

                int err2 = err * 2;
                if (err2 > -dz)
                {
                    err -= dz;
                    x0 += sx;
                }

                if (err2 < dx)
                {
                    err += dx;
                    z0 += sz;
                }
            }
        }

        // 生成路径宽度格，负责给冲撞路径提供可配置宽度。
        private static IEnumerable<IntVec3> WidthCells(Map map, IntVec3 center, int pathWidth)
        {
            int radius = Mathf.Max(0, pathWidth - 1);
            if (radius == 0)
            {
                yield return center;
                yield break;
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (cell.InBounds(map))
                {
                    yield return cell;
                }
            }
        }

        // 按方向计算目标格，负责把浮点方向转换成击退终点格。
        private static IntVec3 CellInDirection(IntVec3 start, Vector3 direction, int distance)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return start;
            }

            direction.Normalize();
            int x = start.x + Mathf.RoundToInt(direction.x * distance);
            int z = start.z + Mathf.RoundToInt(direction.z * distance);
            if (x == start.x && z == start.z)
            {
                if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
                {
                    x += direction.x >= 0f ? distance : -distance;
                }
                else
                {
                    z += direction.z >= 0f ? distance : -distance;
                }
            }

            return new IntVec3(x, start.y, z);
        }
    }
}
