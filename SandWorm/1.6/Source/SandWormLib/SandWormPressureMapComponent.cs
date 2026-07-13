using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace SandWormLib
{
    public sealed class SandWormPressureMapComponent : MapComponent
    {
        private const int SegmentDamageIntervalTicks = 60;
        private const float SegmentDamagePerSecond = 20f;
        // 以下尺寸类参数为"基准值"（对应 sizeMultiplier = 1.0），运行时按沙虫自身尺寸缩放
        private const float BaseSegmentPushDistance = 2.25f;
        private const float BaseDamageWidthPadding = 4f;
        private const float HeadKillForwardOffset = 0f;
        private const float BaseHeadKillLength = 5f;
        private const float BaseHeadKillWidthPadding = 7f;

        // 沙地参数
        private const float BaseSandEdgePadding = 3f;
        private const float SandEdgeNoiseFrequency = 0.15f;
        private const float SandEdgeNoiseThreshold = 0.25f;
        private const int SandApplyIntervalTicks = 15;
        private const int SegmentsPerTickBatch = 3;
        private const int DebugCellRefreshIntervalTicks = 15;

        private readonly List<IntVec3> headKillCellsBuffer = new List<IntVec3>(96);
        private readonly List<Thing> headKillThingsBuffer = new List<Thing>(64);
        private readonly List<IntVec3> debugDamageCells = new List<IntVec3>(256);
        private readonly List<IntVec3> debugPushCells = new List<IntVec3>(256);
        private readonly List<IntVec3> debugHeadKillCells = new List<IntVec3>(96);
        private readonly HashSet<IntVec3> debugDamageCellSet = new HashSet<IntVec3>();
        private readonly HashSet<IntVec3> debugPushCellSet = new HashSet<IntVec3>();
        private readonly HashSet<IntVec3> debugHeadKillCellSet = new HashSet<IntVec3>();
        private readonly Dictionary<int, int> pawnLastPushTick = new Dictionary<int, int>();
        private readonly List<SandWormThing> activeWorms = new List<SandWormThing>(2);
        private TerrainDef softSandDef;
        private TerrainDef sandDef;
        private readonly Perlin sandEdgeNoise;
        private int sandSegmentCursor;
        private int lastDebugCellRefreshTick = -99999;
        private bool collectingDebugCells;

        public SandWormPressureMapComponent(Map map) : base(map)
        {
            sandEdgeNoise = new Perlin(
                SandEdgeNoiseFrequency, 2.0, 0.5, 4,
                map.uniqueID ^ 0x53414E44,
                QualityMode.Medium);
        }

        // 坐标系：wrapper Euler(90,0,0)，bone.rotation 含 90° X 旋转
        // 水平前进 = rotation * Vector3.up
        private static void GetSegmentAxes(Quaternion boneRotation, out Vector3 fwd, out Vector3 right)
        {
            fwd = boneRotation * Vector3.up;
            fwd.y = 0f;
            float sqr = fwd.sqrMagnitude;
            if (sqr > 0.0001f)
            {
                float inv = 1f / Mathf.Sqrt(sqr);
                fwd.x *= inv;
                fwd.z *= inv;
            }

            right = new Vector3(fwd.z, 0f, -fwd.x);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (activeWorms.Count == 0)
            {
                return;
            }

            int currentTick = Find.TickManager.TicksGame;
            collectingDebugCells = ShouldShowDebugCells() && currentTick - lastDebugCellRefreshTick >= DebugCellRefreshIntervalTicks;
            if (collectingDebugCells)
            {
                ClearDebugCells();
                lastDebugCellRefreshTick = currentTick;
            }

            for (int i = activeWorms.Count - 1; i >= 0; i--)
            {
                SandWormThing worm = activeWorms[i];
                if (!IsRegisteredWormValid(worm))
                {
                    activeWorms.RemoveAt(i);
                    continue;
                }

                if (!worm.SurfacePressureEnabled)
                {
                    continue;
                }

                ApplySegmentPressure(worm);
            }

            collectingDebugCells = false;
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();

            if (ShouldShowDebugCells())
            {
                DrawDebugCells();
            }
        }

        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();

            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (activeWorms.Count == 0)
            {
                return;
            }

            for (int i = activeWorms.Count - 1; i >= 0; i--)
            {
                SandWormThing worm = activeWorms[i];
                if (!IsRegisteredWormValid(worm))
                {
                    activeWorms.RemoveAt(i);
                    continue;
                }

                worm.DrawProjectedPathOnGUI();
                worm.DrawHitProxyDebugRectsOnGUI();
            }

        }

        internal void RegisterWorm(SandWormThing worm)
        {
            if (!IsRegisteredWormValid(worm) || activeWorms.Contains(worm))
            {
                return;
            }

            activeWorms.Add(worm);
        }

        internal void UnregisterWorm(SandWormThing worm)
        {
            if (worm == null)
            {
                return;
            }

            activeWorms.Remove(worm);
        }

        private void ClearDebugCells()
        {
            debugDamageCells.Clear();
            debugPushCells.Clear();
            debugHeadKillCells.Clear();
            debugDamageCellSet.Clear();
            debugPushCellSet.Clear();
            debugHeadKillCellSet.Clear();
        }

        private void DrawDebugCells()
        {
            if (debugDamageCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(debugDamageCells, Color.yellow);
            }

            if (debugPushCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(debugPushCells, Color.cyan);
            }

            if (debugHeadKillCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(debugHeadKillCells, Color.red);
            }
        }

        private static bool ShouldShowDebugCells()
        {
            return SandWormMod.Settings != null && SandWormMod.Settings.showPressureDebugCells;
        }

        private static float GetDamageWidthScale()
        {
            return SandWormMod.Settings != null ? Mathf.Clamp(SandWormMod.Settings.damageWidthScale, 0.1f, 1.5f) : 0.5f;
        }

        private static float GetPushWidthScale()
        {
            return SandWormMod.Settings != null ? Mathf.Clamp(SandWormMod.Settings.pushWidthScale, 0.1f, 1.5f) : 0.5f;
        }

        private static float GetHeadKillWidthScale()
        {
            return SandWormMod.Settings != null ? Mathf.Clamp(SandWormMod.Settings.headKillWidthScale, 0.1f, 1.5f) : 0.5f;
        }

        private static float GetAttackPowerMultiplier()
        {
            return SandWormMod.Settings != null ? Mathf.Clamp(SandWormMod.Settings.attackPowerMultiplier, SandWormSettings.MinMultiplier, SandWormSettings.MaxMultiplier) : 1f;
        }

        // IsHeadInstantKillEnabled 负责同时检查全局设置和当前沙虫类型是否允许头部秒杀。
        private static bool IsHeadInstantKillEnabled(SandWormThing worm)
        {
            return worm != null && worm.AllowsHeadInstantKill;
        }

        private static void AddUniqueDebugCell(List<IntVec3> cells, HashSet<IntVec3> cellSet, IntVec3 cell)
        {
            if (cellSet.Add(cell))
            {
                cells.Add(cell);
            }
        }

        private bool IsRegisteredWormValid(SandWormThing worm)
        {
            return worm != null
                && !worm.Destroyed
                && worm.Spawned
                && worm.Map == map;
        }

        private void ApplySegmentPressure(SandWormThing worm)
        {
            if (!worm.IsSegmentSystemReady)
            {
                return;
            }

            float sizeScale = worm.SizeMultiplier;
            float damageWidthScale = GetDamageWidthScale();
            float pushWidthScale = GetPushWidthScale();
            float headKillWidthScale = GetHeadKillWidthScale();
            float headKillLength = BaseHeadKillLength * sizeScale;
            float headKillWidthPadding = BaseHeadKillWidthPadding * sizeScale;
            float damageWidthPadding = BaseDamageWidthPadding * sizeScale;

            if (IsHeadInstantKillEnabled(worm) && worm.TryGetSegmentData(0, out Vector3 headCenter, out Quaternion headRotation, out Vector2 headSize))
            {
                GetSegmentAxes(headRotation, out Vector3 headFwd, out Vector3 headRight);
                Vector3 killCenter = headCenter + headFwd * HeadKillForwardOffset;
                Vector2 killSize = new Vector2(headSize.x + headKillWidthPadding, headKillLength);
                ApplyHeadKillZone(worm, killCenter, headFwd, headRight, killSize, headKillWidthScale);
            }

            int currentTick = Find.TickManager.TicksGame;
            bool dealDamageThisTick = currentTick % SegmentDamageIntervalTicks == 0;
            bool applySandThisTick = currentTick % SandApplyIntervalTicks == 0;

            if (currentTick % 120 == 0)
            {
                CleanupPushHistory(currentTick);
            }

            int segmentCount = worm.SegmentCount;
            int sandStart = -1;
            int sandEnd = -1;
            if (applySandThisTick)
            {
                sandStart = sandSegmentCursor % segmentCount;
                sandEnd = Mathf.Min(sandStart + SegmentsPerTickBatch, segmentCount);
                sandSegmentCursor = sandEnd >= segmentCount ? 0 : sandEnd;
            }

            for (int seg = 0; seg < segmentCount; seg++)
            {
                if (!worm.TryGetSegmentData(seg, out Vector3 center, out Quaternion rotation, out Vector2 size))
                {
                    continue;
                }

                GetSegmentAxes(rotation, out Vector3 fwd, out Vector3 right);

                float pushHalfW = size.x * 0.5f;
                float pushHalfL = size.y * 0.5f;
                float dmgHalfW = (size.x + damageWidthPadding) * 0.5f;
                float dmgHalfL = size.y * 0.5f;

                float scanHalfW = dmgHalfW * damageWidthScale;
                float scanHalfL = dmgHalfL;
                float scanRadius = Mathf.Sqrt(scanHalfW * scanHalfW + scanHalfL * scanHalfL);

                IntVec3 centerCell = center.ToIntVec3();
                int cellRadius = Mathf.CeilToInt(scanRadius) + 1;
                CellRect rect = CellRect.CenteredOn(centerCell, cellRadius);

                foreach (IntVec3 cell in rect)
                {
                    if (!cell.InBounds(map))
                    {
                        continue;
                    }

                    Vector3 cellCenter = cell.ToVector3Shifted();
                    Vector3 offset = cellCenter - center;
                    offset.y = 0f;

                    float lx = offset.x * right.x + offset.z * right.z;
                    float lz = offset.x * fwd.x + offset.z * fwd.z;
                    float absLx = Mathf.Abs(lx);
                    float absLz = Mathf.Abs(lz);

                    bool inDamage = absLx <= scanHalfW && absLz <= dmgHalfL;
                    if (!inDamage)
                    {
                        continue;
                    }

                    bool inPush = absLx <= pushHalfW * pushWidthScale && absLz <= pushHalfL;

                    if (collectingDebugCells)
                    {
                        AddUniqueDebugCell(debugDamageCells, debugDamageCellSet, cell);
                        if (inPush)
                        {
                            AddUniqueDebugCell(debugPushCells, debugPushCellSet, cell);
                        }
                    }

                    List<Thing> thingsAt = map.thingGrid.ThingsListAtFast(cell);
                    for (int ti = thingsAt.Count - 1; ti >= 0; ti--)
                    {
                        Pawn pawn = thingsAt[ti] as Pawn;
                        if (pawn == null || pawn.Destroyed || pawn.Dead || pawn.Downed || !pawn.Spawned)
                        {
                            continue;
                        }

                        if (inPush && !WasRecentlyPushed(pawn, currentTick))
                        {
                            Vector3 pOffset = pawn.DrawPos - center;
                            pOffset.y = 0f;
                            float plx = pOffset.x * right.x + pOffset.z * right.z;
                            float plz = pOffset.x * fwd.x + pOffset.z * fwd.z;
                            PushPawnOut(pawn, center, fwd, right, size, plx, plz, sizeScale);
                            pawnLastPushTick[pawn.thingIDNumber] = currentTick;
                        }

                        if (dealDamageThisTick)
                        {
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Crush, SegmentDamagePerSecond * GetAttackPowerMultiplier(), 0f, -1f, worm));
                        }
                    }
                }

                if (applySandThisTick && seg >= sandStart && seg < sandEnd)
                {
                    ApplySandTerrain(center, fwd, right, size, sizeScale);
                }
            }
        }

        private void ApplySandTerrain(Vector3 center, Vector3 fwd, Vector3 right, Vector2 pushSize, float sizeScale)
        {
            if (!IsWorldDestructionEnabled())
            {
                return;
            }

            EnsureSandDefs();
            if (softSandDef == null && sandDef == null)
            {
                return;
            }

            float sandEdgePadding = BaseSandEdgePadding * sizeScale;
            float innerHW = pushSize.x * 0.5f;
            float innerHL = pushSize.y * 0.5f;
            float outerHW = innerHW + sandEdgePadding;
            float outerHL = innerHL + sandEdgePadding;
            float radius = Mathf.Sqrt(outerHW * outerHW + outerHL * outerHL);

            IntVec3 centerCell = center.ToIntVec3();
            int cellRadius = Mathf.CeilToInt(radius) + 1;

            CellRect rect = CellRect.CenteredOn(centerCell, cellRadius);
            foreach (IntVec3 cell in rect)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                Vector3 cellCenter = cell.ToVector3Shifted();
                float ox = cellCenter.x - center.x;
                float oz = cellCenter.z - center.z;

                float lx = ox * right.x + oz * right.z;
                float lz = ox * fwd.x + oz * fwd.z;
                float absX = Mathf.Abs(lx);
                float absZ = Mathf.Abs(lz);

                if (absX > outerHW || absZ > outerHL)
                {
                    continue;
                }

                if (absX <= innerHW && absZ <= innerHL)
                {
                    SetSandAt(cell, softSandDef ?? sandDef);
                }
                else
                {
                    float edgeX = Mathf.Max(0f, absX - innerHW);
                    float edgeZ = Mathf.Max(0f, absZ - innerHL);
                    float edgeDist = Mathf.Max(edgeX, edgeZ);

                    float distFactor = 1f - edgeDist / sandEdgePadding;
                    float noiseVal = (float)sandEdgeNoise.GetValue(cellCenter.x, 0, cellCenter.z);
                    float noiseMapped = (noiseVal + 1f) * 0.5f;

                    if (distFactor * noiseMapped > SandEdgeNoiseThreshold)
                    {
                        TerrainDef terrain = edgeDist < sandEdgePadding * 0.5f && softSandDef != null
                            ? softSandDef
                            : (sandDef ?? softSandDef);
                        SetSandAt(cell, terrain);
                    }
                }
            }
        }

        private void SetSandAt(IntVec3 cell, TerrainDef terrain)
        {
            if (terrain == null)
            {
                return;
            }

            TerrainDef current = map.terrainGrid.TerrainAt(cell);
            if (current == terrain)
            {
                return;
            }

            if (current == softSandDef && terrain == sandDef)
            {
                return;
            }

            if ((current == softSandDef || current == sandDef) && terrain != softSandDef)
            {
                return;
            }

            map.terrainGrid.SetTerrain(cell, terrain);
        }

        private void EnsureSandDefs()
        {
            if (softSandDef == null)
            {
                softSandDef = DefDatabase<TerrainDef>.GetNamedSilentFail("SoftSand");
            }

            if (sandDef == null)
            {
                sandDef = DefDatabase<TerrainDef>.GetNamedSilentFail("Sand");
            }
        }

        private void ApplyHeadKillZone(SandWormThing worm, Vector3 center, Vector3 fwd, Vector3 right, Vector2 size, float headKillWidthScale)
        {
            float halfW = size.x * 0.5f * headKillWidthScale;
            float halfL = size.y * 0.5f;
            float radius = Mathf.Sqrt(halfW * halfW + halfL * halfL);
            IntVec3 centerCell = center.ToIntVec3();
            int cellRadius = Mathf.CeilToInt(radius) + 1;

            headKillCellsBuffer.Clear();
            headKillThingsBuffer.Clear();

            CellRect rect = CellRect.CenteredOn(centerCell, cellRadius);
            foreach (IntVec3 cell in rect)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                float ox = cell.x + 0.5f - center.x;
                float oz = cell.z + 0.5f - center.z;
                float lx = ox * right.x + oz * right.z;
                float lz = ox * fwd.x + oz * fwd.z;

                if (Mathf.Abs(lx) <= halfW && Mathf.Abs(lz) <= halfL)
                {
                    headKillCellsBuffer.Add(cell);
                    if (collectingDebugCells)
                    {
                        AddUniqueDebugCell(debugHeadKillCells, debugHeadKillCellSet, cell);
                    }

                    List<Thing> thingsAt = map.thingGrid.ThingsListAtFast(cell);
                    for (int ti = 0; ti < thingsAt.Count; ti++)
                    {
                        Thing thing = thingsAt[ti];
                        if (thing != null && !thing.Destroyed && !headKillThingsBuffer.Contains(thing) && !ShouldIgnoreHeadKillThing(worm, thing))
                        {
                            headKillThingsBuffer.Add(thing);
                        }
                    }
                }
            }

            bool allowWorldDestruction = IsWorldDestructionEnabled();

            for (int i = 0; i < headKillThingsBuffer.Count; i++)
            {
                Thing thing = headKillThingsBuffer[i];
                if (thing == null || thing.Destroyed)
                {
                    continue;
                }

                Pawn pawn = thing as Pawn;
                if (pawn != null && pawn.Spawned && !pawn.Dead)
                {
                    pawn.Kill(new DamageInfo(DamageDefOf.Crush, 999999f, 999f, -1f, worm));
                    Corpse corpse = pawn.Corpse;
                    if (corpse != null && corpse.Spawned && !corpse.Destroyed)
                    {
                        corpse.Destroy(DestroyMode.KillFinalize);
                    }

                    continue;
                }

                Corpse thingCorpse = thing as Corpse;
                if (thingCorpse != null && thingCorpse.Spawned)
                {
                    thingCorpse.Destroy(DestroyMode.KillFinalize);
                    continue;
                }

                if (allowWorldDestruction && ShouldDestroyThingInHeadZone(thing))
                {
                    thing.Destroy(DestroyMode.KillFinalize);
                }
            }

            if (allowWorldDestruction)
            {
                EnsureSandDefs();
                TerrainDef sandTarget = softSandDef ?? sandDef;
                for (int ci = 0; ci < headKillCellsBuffer.Count; ci++)
                {
                    IntVec3 cell = headKillCellsBuffer[ci];
                    map.roofGrid.SetRoof(cell, null);
                    if (sandTarget != null && map.terrainGrid.TerrainAt(cell) != sandTarget)
                    {
                        map.terrainGrid.SetTerrain(cell, sandTarget);
                    }
                }
            }
        }

        private static bool IsWorldDestructionEnabled()
        {
            return SandWormMod.Settings == null || SandWormMod.Settings.enableWorldDestruction;
        }

        private static bool ShouldDestroyThingInHeadZone(Thing thing)
        {
            if (thing is Pawn || thing is Corpse)
            {
                return false;
            }

            if (thing.def == null || !thing.Spawned || !thing.def.destroyable)
            {
                return false;
            }

            if (thing.def.category == ThingCategory.Item)
            {
                return true;
            }

            return thing.def.building != null || thing.def.mineable || thing.def.category == ThingCategory.Building;
        }

        private static bool ShouldIgnoreHeadKillThing(SandWormThing worm, Thing thing)
        {
            if (thing == worm)
            {
                return true;
            }

            SandWormHitProxyThing hitProxy = thing as SandWormHitProxyThing;
            return hitProxy != null && hitProxy.Owner == worm;
        }

        private void PushPawnOut(Pawn pawn, Vector3 center, Vector3 fwd, Vector3 right, Vector2 size, float localX, float localZ, float sizeScale)
        {
            float px = localX;
            float pz = localZ;
            if (px * px + pz * pz < 0.0001f)
            {
                pz = 1f;
            }

            float mag = Mathf.Sqrt(px * px + pz * pz);
            px /= mag;
            pz /= mag;

            Vector3 worldPush = right * px + fwd * pz;
            worldPush.y = 0f;

            float pushDist = Mathf.Max(size.x, size.y) * 0.5f + BaseSegmentPushDistance * sizeScale;
            Vector3 targetWorld = center + worldPush * pushDist;
            IntVec3 destCell = targetWorld.ToIntVec3();

            if (!destCell.InBounds(map) || !destCell.Standable(map))
            {
                destCell = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 3);
            }

            if (destCell.IsValid && destCell.InBounds(map) && destCell.Standable(map) && destCell != pawn.Position)
            {
                pawn.Position = destCell;
            }
        }

        private bool WasRecentlyPushed(Pawn pawn, int currentTick)
        {
            if (!pawnLastPushTick.TryGetValue(pawn.thingIDNumber, out int lastTick))
            {
                return false;
            }

            return currentTick - lastTick <= 60;
        }

        private void CleanupPushHistory(int currentTick)
        {
            List<int> toRemove = null;
            foreach (KeyValuePair<int, int> pair in pawnLastPushTick)
            {
                if (currentTick - pair.Value > 120)
                {
                    if (toRemove == null)
                    {
                        toRemove = new List<int>();
                    }

                    toRemove.Add(pair.Key);
                }
            }

            if (toRemove == null)
            {
                return;
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                pawnLastPushTick.Remove(toRemove[i]);
            }
        }
    }
}
