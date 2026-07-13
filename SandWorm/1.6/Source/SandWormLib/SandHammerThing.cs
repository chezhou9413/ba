using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandHammerThing 负责沙锤部署、震动倒计时、分层动画和最终召唤沙虫。
    public sealed class SandHammerThing : Building
    {
        private const int SpawnDelayTicks = 15000;
        private const int TopHoldTicks = 120;
        private const int FallTicks = 12;
        private const int BottomHoldTicks = 60;
        private const int RiseTicks = 48;
        private const float PulseBaseScale = 1.2f;
        private const float PulseMaxScale = 4f;
        private const int EdgeSpawnInset = 18;
        private const float HammerDrawSize = 1f;
        private const float TopDownOffset = 0.084f;
        private const float BottomAltitudeOffset = 0.004f;
        private const float TopAltitudeOffset = 0.008f;

        private bool activated;
        private int activationTick = -1;
        private bool wrongMapMessageShown;
        private int visualAnimationStartTick = -1;

        private static FleckDef shockwaveDef;
        private static Graphic bottomGraphic;
        private static Graphic topGraphic;

        private SandWormLeviathanSite LeviathanSite
        {
            get
            {
                return Map?.Parent as SandWormLeviathanSite;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            // visualAnimationStartTick 负责记录视觉动画的统一起点。
            // 不能使用 thingIDNumber，因为物体 ID 往往远大于当前游戏 tick，
            // 会导致 elapsed 长期为负数，使动画永远返回 0。
            if (visualAnimationStartTick < 0)
            {
                visualAnimationStartTick = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            }

            if (!respawningAfterLoad && !activated)
            {
                TryActivate();
            }
        }

        // Tick 负责维护召唤倒计时；建筑放下后始终可以播放待机动画，但只有目标地图才会进入召唤流程。
        protected override void Tick()
        {
            base.Tick();

            TickPulseEffect();

            if (!activated || activationTick < 0 || Spawned == false || Map == null)
            {
                if (!activated && Spawned && Map != null && !wrongMapMessageShown)
                {
                    TryActivate();
                }

                return;
            }

            if (Find.TickManager.TicksGame - activationTick < SpawnDelayTicks)
            {
                return;
            }

            activated = false;
            SpawnSandWorm();
        }

        // DrawAt 负责在建筑真正放置后手动绘制上下两层贴图，让地图上的实体始终使用动画绘制逻辑。
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            DrawHammerLayer(GetBottomGraphic(), drawLoc, BottomAltitudeOffset, 0f);
            float animationOffset = GetTopAnimationOffset();
            DrawHammerLayer(GetTopGraphic(), drawLoc, TopAltitudeOffset, animationOffset);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref activated, "activated", defaultValue: false);
            Scribe_Values.Look(ref activationTick, "activationTick", -1);
            Scribe_Values.Look(ref wrongMapMessageShown, "wrongMapMessageShown", defaultValue: false);
            Scribe_Values.Look(ref visualAnimationStartTick, "visualAnimationStartTick", -1);
        }

        private void TryActivate()
        {
            if (Map == null || !Spawned)
            {
                return;
            }

            if (!CanActivateOnCurrentMap())
            {
                wrongMapMessageShown = true;
                Messages.Message("SandWorm_Quest_SandHammer_WrongMap".Translate(), this, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            activated = true;
            activationTick = Find.TickManager.TicksGame;
            wrongMapMessageShown = false;
            Messages.Message("SandWorm_Quest_SandHammer_Activated".Translate(), this, MessageTypeDefOf.PositiveEvent);
            TickPulseEffect();
        }

        private bool CanActivateOnCurrentMap()
        {
            return LeviathanSite != null;
        }

        // GetTopAnimationOffset 负责计算沙锤顶部的统一连续动画；无论是否能召唤，都持续循环，不再包含长时间静止段。
        private float GetTopAnimationOffset()
        {
            int cycleStartTick = visualAnimationStartTick;
            int elapsed = Find.TickManager.TicksGame - cycleStartTick;
            if (elapsed < 0)
            {
                return 0f;
            }

            return GetHammerCycleOffset(elapsed, TopDownOffset);
        }

        // DrawHammerLayer 负责用固定 1x1 尺寸绘制一层沙锤贴图。
        private void DrawHammerLayer(Graphic graphic, Vector3 drawLoc, float altitudeOffset, float zOffset)
        {
            if (graphic == null)
            {
                base.DrawAt(drawLoc);
                return;
            }

            Vector3 pos = drawLoc;
            pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor() + altitudeOffset;
            pos.z += zOffset;

            graphic.Draw(pos, Rotation, this);
        }

        // GetBottomGraphic 延迟加载沙锤底座贴图；蓝图、UI 和建造预览走 ThingDef 的完整静态纹理，只有已放置实体才走这里。
        private static Graphic GetBottomGraphic()
        {
            if (bottomGraphic == null)
            {
                bottomGraphic = GraphicDatabase.Get<Graphic_Single>(
                    "Things/SandWorm/SandHammer_Bottom",
                    ShaderDatabase.Cutout,
                    new Vector2(HammerDrawSize, HammerDrawSize),
                    Color.white);
            }

            return bottomGraphic;
        }

        // GetTopGraphic 延迟加载沙锤上半部分贴图。
        private static Graphic GetTopGraphic()
        {
            if (topGraphic == null)
            {
                topGraphic = GraphicDatabase.Get<Graphic_Single>(
                    "Things/SandWorm/SandHammer_Top",
                    ShaderDatabase.Cutout,
                    new Vector2(HammerDrawSize, HammerDrawSize),
                    Color.white);
            }

            return topGraphic;
        }

        private void TickPulseEffect()
        {
            if (!Spawned || Map == null || Find.TickManager == null)
            {
                return;
            }

            int elapsed = Find.TickManager.TicksGame - visualAnimationStartTick;
            if (elapsed < 0 || !IsHammerImpactTick(elapsed))
            {
                return;
            }

            FleckDef fleck = shockwaveDef;
            if (fleck == null)
            {
                fleck = DefDatabase<FleckDef>.GetNamedSilentFail("ShockwaveFast");
                shockwaveDef = fleck;
            }

            if (fleck == null)
            {
                return;
            }

            float scale = PulseBaseScale;
            if (activated && activationTick >= 0)
            {
                float progress = Mathf.Clamp01((float)(Find.TickManager.TicksGame - activationTick) / SpawnDelayTicks);
                scale = Mathf.Lerp(PulseBaseScale, PulseMaxScale, progress);
            }

            FleckMaker.Static(DrawPos, Map, fleck, scale);
        }

        // GetHammerCycleOffset 负责按“顶部停顿、快速下坠、底部停顿、缓慢回升”的四段式节奏计算位移。
        private static float GetHammerCycleOffset(int elapsedTicks, float maxOffset)
        {
            if (maxOffset <= 0f)
            {
                return 0f;
            }

            int cycleTick = elapsedTicks % GetHammerCycleLength();
            if (cycleTick < TopHoldTicks)
            {
                return 0f;
            }

            cycleTick -= TopHoldTicks;
            if (cycleTick < FallTicks)
            {
                float fallProgress = (cycleTick + 1f) / FallTicks;
                return -Mathf.Lerp(0f, maxOffset, fallProgress);
            }

            cycleTick -= FallTicks;
            if (cycleTick < BottomHoldTicks)
            {
                return -maxOffset;
            }

            cycleTick -= BottomHoldTicks;
            if (cycleTick < RiseTicks)
            {
                float riseProgress = (cycleTick + 1f) / RiseTicks;
                float easedRise = Mathf.SmoothStep(0f, 1f, riseProgress);
                return -Mathf.Lerp(maxOffset, 0f, easedRise);
            }

            return 0f;
        }

        // IsHammerImpactTick 判断当前 tick 是否刚好进入最低点停顿阶段；声波特效在这个时刻触发以和砸地瞬间对齐。
        private static bool IsHammerImpactTick(int elapsedTicks)
        {
            if (elapsedTicks < 0)
            {
                return false;
            }

            return elapsedTicks % GetHammerCycleLength() == TopHoldTicks + FallTicks;
        }

        // GetHammerCycleLength 返回一次完整机械敲击循环的总时长。
        private static int GetHammerCycleLength()
        {
            return TopHoldTicks + FallTicks + BottomHoldTicks + RiseTicks;
        }

        private void SpawnSandWorm()
        {
            ThingDef wormDef = DefDatabase<ThingDef>.GetNamedSilentFail("SandWorm_Thing");
            if (wormDef == null)
            {
                return;
            }

            Map spawnMap = Map;
            IntVec3 hammerPosition = Position;
            if (spawnMap == null)
            {
                return;
            }

            SandWormHitPointUtility.SyncConfiguredMaxHitPoints();
            SandWormQuestUtility.ForceAbnormalSandstorm(spawnMap);
            LeviathanSite?.NotifyLeviathanSpawned();

            IntVec3 spawnCell = FindEdgeSpawnCell(spawnMap, hammerPosition, wormDef);
            Thing worm = ThingMaker.MakeThing(wormDef);
            Thing spawnedWorm = GenSpawn.Spawn(worm, spawnCell, spawnMap, WipeMode.VanishOrMoveAside);
            Find.LetterStack.ReceiveLetter(
                "SandWorm_Quest_Spawned_Label".Translate(),
                "SandWorm_Quest_Spawned_Text".Translate(),
                LetterDefOf.ThreatBig,
                spawnedWorm);
            Destroy(DestroyMode.Vanish);
        }

        private static IntVec3 FindEdgeSpawnCell(Map map, IntVec3 targetCell, ThingDef wormDef)
        {
            IntVec3 bestCell = IntVec3.Invalid;
            float bestDistanceSq = -1f;
            CellRect edgeRect = CellRect.WholeMap(map).ContractedBy(1);

            for (int i = 0; i < 200; i++)
            {
                IntVec3 candidate = InsetFromEdge(CellFinder.RandomEdgeCell(map), map);
                if (!candidate.InBounds(map) || candidate.Fogged(map))
                {
                    continue;
                }

                if (candidate.Standable(map) && GenSpawn.CanSpawnAt(wormDef, candidate, map, Rot4.North, canWipeEdifices: false))
                {
                    float distanceSq = (candidate - targetCell).LengthHorizontalSquared;
                    if (distanceSq > bestDistanceSq)
                    {
                        bestDistanceSq = distanceSq;
                        bestCell = candidate;
                    }
                }
            }

            if (bestCell.IsValid)
            {
                return bestCell;
            }

            int x = targetCell.x < map.Size.x / 2 ? edgeRect.maxX - EdgeSpawnInset : edgeRect.minX + EdgeSpawnInset;
            int z = Mathf.Clamp(targetCell.z, edgeRect.minZ, edgeRect.maxZ);
            return new IntVec3(x, 0, z);
        }

        private static IntVec3 InsetFromEdge(IntVec3 cell, Map map)
        {
            IntVec3 clamped = ClampToUsableMapCell(cell, map);
            int minX = EdgeSpawnInset;
            int maxX = map.Size.x - 1 - EdgeSpawnInset;
            int minZ = EdgeSpawnInset;
            int maxZ = map.Size.z - 1 - EdgeSpawnInset;

            if (clamped.x <= 1)
            {
                clamped.x = minX;
            }
            else if (clamped.x >= map.Size.x - 2)
            {
                clamped.x = maxX;
            }

            if (clamped.z <= 1)
            {
                clamped.z = minZ;
            }
            else if (clamped.z >= map.Size.z - 2)
            {
                clamped.z = maxZ;
            }

            return ClampToUsableMapCell(clamped, map);
        }

        private static IntVec3 ClampToUsableMapCell(IntVec3 cell, Map map)
        {
            return new IntVec3(
                Mathf.Clamp(cell.x, 1, map.Size.x - 2),
                0,
                Mathf.Clamp(cell.z, 1, map.Size.z - 2));
        }

        private static float GetYawToward(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                return Rand.Range(0f, 360f);
            }

            direction.Normalize();
            return Mathf.Atan2(-direction.x, direction.z) * Mathf.Rad2Deg;
        }
    }
}
