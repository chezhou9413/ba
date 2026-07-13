using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SandWormLib
{
    // SandWormSyndicateChallengeState 负责管理辛迪加直启挑战的地图创建、倒计时、沙虫刷新、小人保护和失败清理。
    public sealed class SandWormSyndicateChallengeState : GameComponent
    {
        private const int SpawnDelayTicks = 600;
        private const int CheckIntervalTicks = 30;
        private const int ParticipantFailureCheckGraceTicks = 360;
        private const int RescueBloodLossTicks = 2500;
        private const int EdgeSpawnInset = 18;
        private const int ExtraWormMinDistance = 24;
        private const string ChallengeSiteDefName = "SandWorm_SyndicateChallengeSite";
        private const string WormDefName = "SandWorm_Thing";
        private const string SmallWormDefName = "SandWorm_SmallThing";
        private const string MoveSuppressionHediffDefName = "SandWorm_SyndicateMoveSuppression";
        private const string AimSuppressionHediffDefName = "SandWorm_SyndicateAimSuppression";
        private const string ShockwaveRockWallDefName = "SandWorm_ShockwaveRockWall";
        private const int ShockwaveFirstReadyDelayTicks = 900;
        private const int ShockwaveCooldownTicks = 1800;
        private const int ShockwaveDamageAmount = 100;
        private const int ShockwaveGlancingDamageAmount = 45;
        private const int ShockwaveWarningIndicatorTicks = 480;
        private const int ShockwaveWallPersistTicks = 420;
        private const int ShockwaveRingVisualTicks = 330;
        private const int ShockwaveDustFleckIntervalTicks = 10;
        private const int ShockwaveDustFleckCount = 24;
        private const int ShockwaveMaxWarningLanes = 5;
        private const float ShockwaveLaneHalfWidth = 4.2f;
        private const float ShockwaveRockVisualScaleFactor = 2.5f;
        private const int ShockwaveAmbientWallGroups = 34;
        private const int ShockwaveMaxWallProjectiles = 180;
        private const string ShockwaveDistortionMoteDefName = "SandWorm_ShockwaveDistortionMote";
        private const string BlastDryFleckDefName = "BlastDry";

        private static readonly FieldInfo HealthStateField = AccessTools.Field(typeof(Pawn_HealthTracker), "healthState");
        private static readonly List<Hediff> TmpHediffs = new List<Hediff>();
        private static readonly List<IntVec3> TmpShockwaveCells = new List<IntVec3>();
        private static readonly List<IntVec3> TmpShockwaveSources = new List<IntVec3>();
        private static FleckDef blastDryFleckDef;
        private SandWormSyndicateChallengeSite site;
        private List<Pawn> participants = new List<Pawn>();
        private List<Pawn> returnedPawns = new List<Pawn>();
        private List<Map> originMaps = new List<Map>();
        private List<IntVec3> originPositions = new List<IntVec3>();
        private SandWormChallengeRuntimeModifiers runtimeModifiers = new SandWormChallengeRuntimeModifiers();
        private SandWormChallengeResonanceState resonanceState = new SandWormChallengeResonanceState();
        private SandWormThing challengeBossWorm;
        private List<SandWormThing> challengeSmallWorms = new List<SandWormThing>();
        private List<Thing> shockwaveWalls = new List<Thing>();
        private List<SandWormShockwaveRockProjectile> pendingShockwaveWallProjectiles = new List<SandWormShockwaveRockProjectile>();
        private List<IntVec3> shockwaveRingSources = new List<IntVec3>();
        private List<IntVec3> shockwaveWarningSources = new List<IntVec3>();
        private List<SandWormShockwaveLane> shockwaveWarningLanes = new List<SandWormShockwaveLane>();
        private int startTick = -1;
        private int nextShockwaveReadyTick = -1;
        private int shockwaveRingStartTick = -1;
        private int shockwaveWarningEndTick = -1;
        private int shockwaveWallsClearTick = -1;
        private int participantFailureCheckStartTick = -1;
        private float shockwaveRingMaxRadius;
        private int lastShockwaveRingDustTick = -1;
        private bool wormSpawned;
        private bool cleanupQueued;

        // SandWormSyndicateChallengeState 负责让 RimWorld 在创建或读取存档时实例化挑战状态组件。
        public SandWormSyndicateChallengeState(Game game)
        {
        }

        public bool ChallengeActive => site != null && !cleanupQueued;

        public Map ChallengeMap => site?.Map;

        // TryStartChallenge 负责从 UI 直接创建挑战地图并投送已选择的小人。
        public bool TryStartChallenge(IEnumerable<Pawn> selectedPawns, IEnumerable<SandWormChallengeRiskDef> selectedRisks, int contractLevel)
        {
            if (ChallengeActive)
            {
                Messages.Message("SandWorm_SyndicateChallenge_AlreadyActive".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }

            List<Pawn> pawns = ValidParticipants(selectedPawns);
            if (pawns.Count == 0)
            {
                Messages.Message("SandWorm_Contract_StartNeedPawn".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }

            if (!SandWormQuestUtility.TryFindDesertQuestTile(out PlanetTile tile))
            {
                Messages.Message("SandWorm_Quest_NoDesertTile".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }

            WorldObjectDef siteDef = DefDatabase<WorldObjectDef>.GetNamedSilentFail(ChallengeSiteDefName);
            if (siteDef == null)
            {
                Log.Error("[SandWorm] Missing world object def: " + ChallengeSiteDefName);
                return false;
            }

            site = (SandWormSyndicateChallengeSite)WorldObjectMaker.MakeWorldObject(siteDef);
            site.Tile = tile;
            site.SetFaction(Faction.OfPlayer);
            site.forceRemoveWorldObjectWhenMapRemoved = true;
            Find.WorldObjects.Add(site);

            LongEventHandler.QueueLongEvent(delegate
            {
                Map map = GetOrGenerateMapUtility.GetOrGenerateMap(tile, SandWormSyndicateChallengeSite.ChallengeMapSize, siteDef);
                if (map == null)
                {
                    FailStartup();
                    return;
                }

                SandWormQuestUtility.ForceVanillaSandstorm(map);
                StartOnGeneratedMap(pawns, map, SandWormChallengeRuntimeModifiers.FromRisks(selectedRisks), contractLevel);
            }, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
            return true;
        }

        // GameComponentTick 负责推进倒计时、刷新沙虫、检查失败保护和处理异常清理。
        public override void GameComponentTick()
        {
            if (!ChallengeActive)
            {
                return;
            }

            Map map = ChallengeMap;
            if (map == null || site == null || site.Destroyed)
            {
                ReturnAllRemaining();
                FinishFailure();
                return;
            }

            if (!wormSpawned && Find.TickManager.TicksGame - startTick >= SpawnDelayTicks)
            {
                SpawnChallengeWorm(map);
            }

            TickShockwaveRockProjectiles(map);
            TickShockwaveWallCleanup();
            TickResonanceEscalation(map);

            if (CanCheckParticipantFailureStates() && Find.TickManager.TicksGame % CheckIntervalTicks == 0)
            {
                CheckChallengeBossState();
                CheckParticipantFailureStates();
            }
        }

        // GameComponentUpdate 负责绘制不需要保存的挑战视觉效果。
        public override void GameComponentUpdate()
        {
            if (!ChallengeActive || ChallengeMap == null || Find.CurrentMap != ChallengeMap || !WorldRendererUtility.DrawingMap)
            {
                return;
            }

            DrawShockwaveRockProjectiles(ChallengeMap);
            DrawShockwaveWarningIndicators(ChallengeMap);
            DrawShockwaveRingVisual(ChallengeMap);
        }

        // GameComponentOnGUI 负责在当前挑战地图上绘制倒计时提示。
        public override void GameComponentOnGUI()
        {
            if (!ChallengeActive || wormSpawned || ChallengeMap == null || Find.CurrentMap != ChallengeMap || !WorldRendererUtility.DrawingMap)
            {
                return;
            }

            int ticksLeft = Mathf.Max(0, SpawnDelayTicks - (Find.TickManager.TicksGame - startTick));
            int secondsLeft = Mathf.CeilToInt(ticksLeft / 60f);
            string text = "SandWorm_SyndicateChallenge_Countdown".Translate(secondsLeft);
            Rect rect = new Rect((UI.screenWidth - 360f) / 2f, 82f, 360f, Text.LineHeightOf(GameFont.Medium) + 18f);

            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            Color oldColor = GUI.color;

            Widgets.DrawBoxSolid(rect, new Color(0.10f, 0.06f, 0.04f, 0.76f));
            Widgets.DrawBox(rect, 1);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            GUI.color = new Color(1f, 0.76f, 0.33f, 1f);
            Widgets.Label(rect, text);

            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
            Text.WordWrap = oldWordWrap;
            GUI.color = oldColor;
        }

        // ExposeData 负责保存挑战地图、参战小人、返回状态、原地图位置和刷虫进度。
        public override void ExposeData()
        {
            Scribe_References.Look(ref site, "site");
            Scribe_Collections.Look(ref participants, "participants", LookMode.Reference);
            Scribe_Collections.Look(ref returnedPawns, "returnedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref originMaps, "originMaps", LookMode.Reference);
            Scribe_Collections.Look(ref originPositions, "originPositions", LookMode.Value);
            Scribe_Deep.Look(ref runtimeModifiers, "runtimeModifiers");
            Scribe_Deep.Look(ref resonanceState, "resonanceState");
            Scribe_References.Look(ref challengeBossWorm, "challengeBossWorm");
            Scribe_Collections.Look(ref challengeSmallWorms, "challengeSmallWorms", LookMode.Reference);
            Scribe_Collections.Look(ref shockwaveWalls, "shockwaveWalls", LookMode.Reference);
            Scribe_Collections.Look(ref pendingShockwaveWallProjectiles, "pendingShockwaveWallProjectiles", LookMode.Deep);
            Scribe_Collections.Look(ref shockwaveWarningSources, "shockwaveWarningSources", LookMode.Value);
            Scribe_Collections.Look(ref shockwaveWarningLanes, "shockwaveWarningLanes", LookMode.Deep);
            Scribe_Values.Look(ref startTick, "startTick", -1);
            Scribe_Values.Look(ref nextShockwaveReadyTick, "nextShockwaveReadyTick", -1);
            Scribe_Values.Look(ref shockwaveWarningEndTick, "shockwaveWarningEndTick", -1);
            Scribe_Values.Look(ref shockwaveWallsClearTick, "shockwaveWallsClearTick", -1);
            Scribe_Values.Look(ref participantFailureCheckStartTick, "participantFailureCheckStartTick", -1);
            Scribe_Values.Look(ref wormSpawned, "wormSpawned", defaultValue: false);
            Scribe_Values.Look(ref cleanupQueued, "cleanupQueued", defaultValue: false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                EnsureListsNotNull();
            }
        }

        // LoadedGame 负责在读取旧存档或异常存档时补齐挑战状态集合。
        public override void LoadedGame()
        {
            EnsureListsNotNull();
        }

        // FinalizeInit 负责在新游戏或读档初始化末尾保证集合字段可用。
        public override void FinalizeInit()
        {
            EnsureListsNotNull();
        }

        // TryProtectPawnFromDeath 负责给 Harmony 死亡前缀提供兜底保护，阻止参战小人真正死亡。
        public bool TryProtectPawnFromDeath(Pawn pawn)
        {
            if (!IsActiveParticipantOnChallengeMap(pawn))
            {
                return false;
            }

            ReturnParticipant(pawn, "SandWorm_SyndicateChallenge_Returned".Translate(pawn.LabelShortCap));
            return true;
        }

        // NotifyChallengeBossKilled 负责在直启挑战 Boss 死亡时清理挑战地图、停止定时器并回收仍在场的小人。
        public void NotifyChallengeBossKilled(SandWormThing worm)
        {
            if (!ChallengeActive || worm == null || worm != challengeBossWorm)
            {
                return;
            }

            FinishChallengeSuccess();
        }

        // TryGetParticipantRangeFactor 负责给射程 Harmony 补丁提供当前参战小人的挑战射程倍率。
        public bool TryGetParticipantRangeFactor(Pawn pawn, out float rangeFactor)
        {
            rangeFactor = 1f;
            if (!IsActiveParticipantOnChallengeMap(pawn))
            {
                return false;
            }

            rangeFactor = runtimeModifiers?.pawnRangeFactor ?? 1f;
            return rangeFactor < 0.999f;
        }

        // ValidParticipants 负责筛选仍可被投送的玩家自由殖民者。
        private static List<Pawn> ValidParticipants(IEnumerable<Pawn> selectedPawns)
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach (Pawn pawn in selectedPawns)
            {
                if (pawn != null && !pawn.Destroyed && !pawn.Dead && pawn.Spawned && pawn.Faction == Faction.OfPlayer && pawn.IsFreeColonist && !pawns.Contains(pawn))
                {
                    pawns.Add(pawn);
                }
            }

            return pawns;
        }

        // StartOnGeneratedMap 负责记录原始位置并把参战小人投送到挑战地图中心附近。
        private void StartOnGeneratedMap(List<Pawn> pawns, Map map, SandWormChallengeRuntimeModifiers modifiers, int contractLevel)
        {
            participants.Clear();
            returnedPawns.Clear();
            originMaps.Clear();
            originPositions.Clear();
            challengeSmallWorms.Clear();
            challengeBossWorm = null;
            ClearShockwaveWalls(DestroyMode.Vanish);
            ClearShockwaveRingVisual();
            ClearShockwaveWarningIndicators();
            runtimeModifiers = modifiers ?? new SandWormChallengeRuntimeModifiers();
            resonanceState.Reset();
            startTick = Find.TickManager.TicksGame;
            nextShockwaveReadyTick = -1;
            participantFailureCheckStartTick = -1;
            wormSpawned = false;
            cleanupQueued = false;

            IntVec3 center = map.Center;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                Map originMap = pawn.Map;
                IntVec3 originPosition = pawn.Position;
                IntVec3 spawnCell = FindSpawnCellNear(map, center, i);

                participants.Add(pawn);
                originMaps.Add(originMap);
                originPositions.Add(originPosition);

                pawn.jobs?.StopAll();
                pawn.pather?.StopDead();
                pawn.DeSpawn(DestroyMode.Vanish);
                GenSpawn.Spawn(pawn, spawnCell, map, WipeMode.VanishOrMoveAside);
                pawn.Notify_Teleported();
                pawn.drafter.Drafted = true;
                ApplyChallengeHediffs(pawn);
            }

            Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            CameraJumper.TryJump(center, map, CameraJumper.MovementMode.Cut);
            Find.LetterStack.ReceiveLetter(
                "SandWorm_SyndicateChallenge_Started_Label".Translate(),
                "SandWorm_SyndicateChallenge_Started_Text".Translate(pawns.Count, contractLevel),
                LetterDefOf.NeutralEvent,
                new LookTargets(pawns));
        }

        // FailStartup 负责在地图生成失败时清理半创建的隐藏世界对象。
        private void FailStartup()
        {
            Messages.Message("SandWorm_SyndicateChallenge_StartFailed".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            if (site != null && !site.Destroyed)
            {
                site.Destroy();
            }

            site = null;
            cleanupQueued = false;
        }

        // SpawnChallengeWorm 负责在倒计时结束后从地图边缘刷新一只沙海巨虫。
        private void SpawnChallengeWorm(Map map)
        {
            ThingDef wormDef = DefDatabase<ThingDef>.GetNamedSilentFail(WormDefName);
            ThingDef smallWormDef = DefDatabase<ThingDef>.GetNamedSilentFail(SmallWormDefName);
            if (wormDef == null)
            {
                Log.Error("[SandWorm] Missing thing def: " + WormDefName);
                return;
            }

            SandWormHitPointUtility.SyncConfiguredMaxHitPoints();
            SandWormQuestUtility.ForceAbnormalSandstorm(map);

            IntVec3 target = ParticipantsCenter(map);
            IntVec3 spawnCell = FindEdgeSpawnCell(map, target, wormDef);
            Thing worm = ThingMaker.MakeThing(wormDef);
            SandWormThing spawnedWorm = GenSpawn.Spawn(worm, spawnCell, map, WipeMode.VanishOrMoveAside) as SandWormThing;
            challengeBossWorm = spawnedWorm;
            spawnedWorm?.MarkSyndicateChallengeBoss(
                runtimeModifiers?.chargeCooldownFactor ?? 1f,
                runtimeModifiers?.maxIncomingDamagePerHit ?? -1f,
                runtimeModifiers?.enableShockwaveAttack ?? false);
            if (runtimeModifiers != null && runtimeModifiers.enableShockwaveAttack)
            {
                nextShockwaveReadyTick = Find.TickManager.TicksGame + ShockwaveFirstReadyDelayTicks;
            }
            resonanceState.Configure(runtimeModifiers?.resonanceEscalationLevel ?? 0, Find.TickManager.TicksGame);
            SpawnExtraSmallWorms(map, target, smallWormDef, spawnCell);
            wormSpawned = spawnedWorm != null;
            participantFailureCheckStartTick = Find.TickManager.TicksGame + ParticipantFailureCheckGraceTicks;
            Find.LetterStack.ReceiveLetter(
                "SandWorm_SyndicateChallenge_WormSpawned_Label".Translate(),
                "SandWorm_SyndicateChallenge_WormSpawned_Text".Translate(),
                LetterDefOf.ThreatBig,
                spawnedWorm);
        }

        // SpawnExtraSmallWorms 负责根据词条效果刷新额外小沙虫，并写入挑战专属强化。
        private void SpawnExtraSmallWorms(Map map, IntVec3 target, ThingDef smallWormDef, IntVec3 bossSpawnCell)
        {
            if (runtimeModifiers == null || runtimeModifiers.extraSmallWormCount <= 0)
            {
                return;
            }

            if (smallWormDef == null)
            {
                Log.Error("[SandWorm] Missing thing def: " + SmallWormDefName);
                return;
            }

            List<IntVec3> usedCells = new List<IntVec3>();
            if (bossSpawnCell.IsValid)
            {
                usedCells.Add(bossSpawnCell);
            }

            for (int i = 0; i < runtimeModifiers.extraSmallWormCount; i++)
            {
                IntVec3 cell = FindEdgeSpawnCell(map, target, smallWormDef, usedCells, ExtraWormMinDistance);
                Thing thing = ThingMaker.MakeThing(smallWormDef);
                SandWormThing smallWorm = GenSpawn.Spawn(thing, cell, map, WipeMode.VanishOrMoveAside) as SandWormThing;
                if (smallWorm == null)
                {
                    continue;
                }

                smallWorm.SetChallengeModifiers(
                    runtimeModifiers.smallWormHeadInstantKill,
                    runtimeModifiers.smallWormHitPointFactor);
                challengeSmallWorms.Add(smallWorm);
                usedCells.Add(cell);
            }
        }

        // CanStartShockwave 负责判断挑战大沙虫当前是否可以切入冲击波状态。
        public bool CanStartShockwave(SandWormThing worm)
        {
            return ChallengeActive
                && runtimeModifiers != null
                && runtimeModifiers.enableShockwaveAttack
                && worm != null
                && !worm.Destroyed
                && worm.Map == ChallengeMap
                && worm.Spawned
                && nextShockwaveReadyTick > 0
                && Find.TickManager.TicksGame >= nextShockwaveReadyTick;
        }

        // NotifyShockwaveFinished 负责在冲击波状态完成后写入下一次冷却时间。
        public void NotifyShockwaveFinished(SandWormThing worm)
        {
            if (worm == null || worm.Map != ChallengeMap)
            {
                return;
            }

            nextShockwaveReadyTick = Find.TickManager.TicksGame + ScaledShockwaveCooldownTicks();
        }

        // PerformShockwaveAttack 负责按沙虫全身源点生成预警飞石掩体，并提示玩家冲击波即将释放。
        public void PerformShockwaveAttack(SandWormThing worm, List<IntVec3> sourceCells)
        {
            Map map = ChallengeMap;
            if (map == null || worm == null || worm.Map != map)
            {
                return;
            }

            ClearShockwaveWalls(DestroyMode.Vanish);
            NormalizeShockwaveSources(sourceCells, map, worm);
            StartShockwaveWarningIndicators(map, sourceCells);
            SpawnShockwaveWalls(map, sourceCells);
            Messages.Message("SandWorm_SyndicateChallenge_ShockwaveWarning".Translate(), MessageTypeDefOf.ThreatSmall, historical: false);
        }

        // ResolveShockwaveDamage 负责在预警结束时按危险带和掩体状态结算冲击波伤害。
        public void ResolveShockwaveDamage(SandWormThing worm, List<IntVec3> sourceCells)
        {
            Map map = ChallengeMap;
            if (map == null || worm == null || worm.Map != map)
            {
                return;
            }

            NormalizeShockwaveSources(sourceCells, map, worm);
            LandAllShockwaveProjectiles(map);
            ThingDef distortionMote = DefDatabase<ThingDef>.GetNamedSilentFail(ShockwaveDistortionMoteDefName);
            for (int i = 0; i < sourceCells.Count; i++)
            {
                IntVec3 sourceCell = sourceCells[i];
                ThrowShockwaveReleaseDust(map, sourceCell);
            }

            if (distortionMote != null)
            {
                IntVec3 distortionCenter = AverageCells(sourceCells, worm.Position);
                MoteMaker.MakeStaticMote(distortionCenter.ToVector3Shifted(), map, distortionMote, Rand.Range(8.5f, 11.0f), makeOffscreen: true, exactRot: Rand.Range(0f, 360f));
            }

            ThrowShockwaveLaneReleaseBursts(map);
            StartShockwaveRingVisual(map, sourceCells);
            TryShakeCamera(0.35f, 120);
            TryShakeCamera(0.9f);

            for (int i = 0; i < participants.Count; i++)
            {
                Pawn pawn = participants[i];
                if (pawn == null || returnedPawns.Contains(pawn) || pawn.Map != map || !pawn.Spawned || pawn.Dead)
                {
                    continue;
                }

                if (!TryGetShockwaveLaneForPawn(pawn.Position, out SandWormShockwaveLane lane))
                {
                    ThrowShockwaveDodgeFeedback(map, pawn);
                    continue;
                }

                if (ShockwaveBlockedByCover(map, lane.SourceCell, pawn.Position))
                {
                    ThrowShockwaveCoverFeedback(map, pawn);
                    continue;
                }

                int damageAmount = ShockwaveDamageForLane(lane, pawn.Position);
                SandWormShockwaveDustUtility.ThrowPawnShockwaveHitFeedback(map, pawn, lane, ShockwaveRockVisualScaleFactor);
                FleckMaker.ThrowDustPuff(pawn.DrawPos, map, 2.2f * ShockwaveRockVisualScaleFactor);
                DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, damageAmount, 1f, -1f, worm);
                damageInfo.SetBodyRegion(BodyPartHeight.Middle, BodyPartDepth.Outside);
                pawn.TakeDamage(damageInfo);
                PushPawnFromShockwaveLane(map, pawn, lane);
                if (!IsParticipantStillFighting(pawn))
                {
                    ReturnParticipant(pawn, "SandWorm_SyndicateChallenge_Returned".Translate(pawn.LabelShortCap));
                }
            }

            ClearShockwaveWarningIndicators();
            shockwaveWallsClearTick = Find.TickManager.TicksGame + ShockwaveWallPersistTicks;
        }

        // StartShockwaveRingVisual 负责启动一次从沙虫全身源点错峰扩散到地图边缘的冲击波视觉。
        private void StartShockwaveRingVisual(Map map, List<IntVec3> sourceCells)
        {
            shockwaveRingSources.Clear();
            for (int i = 0; i < sourceCells.Count; i++)
            {
                if (sourceCells[i].IsValid && sourceCells[i].InBounds(map) && !shockwaveRingSources.Contains(sourceCells[i]))
                {
                    shockwaveRingSources.Add(sourceCells[i]);
                }
            }

            shockwaveRingStartTick = Find.TickManager.TicksGame;
            shockwaveRingMaxRadius = MaxDistanceToAnyMapCorner(map, shockwaveRingSources) + 8f;
            lastShockwaveRingDustTick = -1;
        }

        // DrawShockwaveRingVisual 负责在释放后的较长时间内只绘制尘土余波，避免明显的巨大圆圈遮挡玩家判断。
        private void DrawShockwaveRingVisual(Map map)
        {
            if (shockwaveRingSources.NullOrEmpty() || shockwaveRingStartTick < 0 || shockwaveRingMaxRadius <= 0f)
            {
                return;
            }

            int elapsed = Find.TickManager.TicksGame - shockwaveRingStartTick;
            if (elapsed < 0 || elapsed > ShockwaveRingVisualTicks)
            {
                ClearShockwaveRingVisual();
                return;
            }

            float progress = Mathf.Clamp01(elapsed / (float)ShockwaveRingVisualTicks);
            for (int i = 0; i < shockwaveRingSources.Count; i++)
            {
                float sourceDelay = (i % 5) * 0.035f;
                float sourceProgress = Mathf.Clamp01((progress - sourceDelay) / Mathf.Max(0.01f, 1f - sourceDelay));
                float sourceEasedProgress = 1f - (1f - sourceProgress) * (1f - sourceProgress);
                float radius = Mathf.Lerp(1.5f, shockwaveRingMaxRadius, sourceEasedProgress);
                Vector3 center = shockwaveRingSources[i].ToVector3Shifted();
                SandWormShockwaveVisualUtility.DrawReleaseWavefront(center, radius, sourceProgress);

                if (elapsed % ShockwaveDustFleckIntervalTicks == 0 && lastShockwaveRingDustTick != Find.TickManager.TicksGame)
                {
                    ThrowShockwaveRingDust(map, center, radius);
                }
            }

            if (elapsed % ShockwaveDustFleckIntervalTicks == 0 && lastShockwaveRingDustTick != Find.TickManager.TicksGame)
            {
                lastShockwaveRingDustTick = Find.TickManager.TicksGame;
            }
        }

        // ThrowShockwaveRingDust 负责沿当前冲击波推进范围稀疏喷出尘土，只保留地面余波而不画亮色线圈。
        private static void ThrowShockwaveRingDust(Map map, Vector3 center, float radius)
        {
            for (int i = 0; i < ShockwaveDustFleckCount; i++)
            {
                float angle = Rand.Range(0f, Mathf.PI * 2f);
                float scatteredRadius = Mathf.Max(1f, radius + Rand.Range(-4.5f, 3.5f));
                Vector3 loc = center + new Vector3(Mathf.Cos(angle) * scatteredRadius, 0f, Mathf.Sin(angle) * scatteredRadius);
                IntVec3 cell = loc.ToIntVec3();
                if (!cell.InBounds(map) || cell.Fogged(map))
                {
                    continue;
                }

                FleckMaker.ThrowDustPuff(cell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.55f), map, Rand.Range(1.6f, 2.8f));
            }
        }

        // ThrowShockwaveReleaseDust 负责在每个身体源点爆发厚重尘土，替代会形成巨大亮圈的冲击波线条。
        private static void ThrowShockwaveReleaseDust(Map map, IntVec3 sourceCell)
        {
            SandWormShockwaveDustUtility.ThrowSourceReleaseBurst(map, sourceCell, ShockwaveRockVisualScaleFactor, BlastDryFleckDef);
            for (int i = 0; i < 9; i++)
            {
                Vector3 loc = sourceCell.ToVector3Shifted() + Gen.RandomHorizontalVector(Rand.Range(0.4f, 3.2f));
                IntVec3 cell = loc.ToIntVec3();
                if (cell.InBounds(map) && !cell.Fogged(map))
                {
                    FleckMaker.ThrowDustPuffThick(loc, map, Rand.Range(2.0f, 3.6f), new Color(0.70f, 0.55f, 0.34f, 0.95f));
                }
            }
        }

        // ClearShockwaveRingVisual 负责清空已经播放结束的冲击波环形视觉状态。
        private void ClearShockwaveRingVisual()
        {
            shockwaveRingSources.Clear();
            shockwaveRingStartTick = -1;
            shockwaveRingMaxRadius = 0f;
            lastShockwaveRingDustTick = -1;
        }

        // MaxDistanceToAnyMapCorner 负责计算多源冲击波扩散到整张地图边缘所需的最大半径。
        private static float MaxDistanceToAnyMapCorner(Map map, List<IntVec3> sourceCells)
        {
            float maxDistance = 0f;
            for (int i = 0; i < sourceCells.Count; i++)
            {
                Vector3 source = sourceCells[i].ToVector3Shifted();
                maxDistance = Mathf.Max(maxDistance, (source - new IntVec3(0, 0, 0).ToVector3Shifted()).MagnitudeHorizontal());
                maxDistance = Mathf.Max(maxDistance, (source - new IntVec3(map.Size.x - 1, 0, 0).ToVector3Shifted()).MagnitudeHorizontal());
                maxDistance = Mathf.Max(maxDistance, (source - new IntVec3(0, 0, map.Size.z - 1).ToVector3Shifted()).MagnitudeHorizontal());
                maxDistance = Mathf.Max(maxDistance, (source - new IntVec3(map.Size.x - 1, 0, map.Size.z - 1).ToVector3Shifted()).MagnitudeHorizontal());
            }

            return maxDistance;
        }

        // DrawShockwaveWarningFlecks 负责在预警阶段持续播放碎石和尘土特效。
        public void DrawShockwaveWarningFlecks(SandWormThing worm, List<IntVec3> sourceCells)
        {
            Map map = ChallengeMap;
            if (map == null || worm == null || worm.Map != map)
            {
                return;
            }

            TryShakeCamera(0.22f);

            for (int i = 0; i < pendingShockwaveWallProjectiles.Count; i += 3)
            {
                SandWormShockwaveRockProjectile projectile = pendingShockwaveWallProjectiles[i];
                if (projectile != null && !projectile.Landed)
                {
                    FleckMaker.ThrowDustPuff(projectile.TargetCell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.35f), map, Rand.Range(1.35f, 2.05f));
                }
            }

            for (int i = 0; i < sourceCells.Count; i++)
            {
                IntVec3 sourceCell = sourceCells[i];
                if (sourceCell.IsValid && sourceCell.InBounds(map))
                {
                    FleckMaker.ThrowDustPuff(sourceCell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.7f), map, Rand.Range(1.4f, 2.3f));
                }
            }
        }

        // StartShockwaveWarningIndicators 负责记录本次冲击波源点，并按参战者位置锁定危险带。
        private void StartShockwaveWarningIndicators(Map map, List<IntVec3> sourceCells)
        {
            if (shockwaveWarningSources == null)
            {
                shockwaveWarningSources = new List<IntVec3>();
            }

            if (shockwaveWarningLanes == null)
            {
                shockwaveWarningLanes = new List<SandWormShockwaveLane>();
            }

            shockwaveWarningSources.Clear();
            for (int i = 0; i < sourceCells.Count; i++)
            {
                IntVec3 sourceCell = sourceCells[i];
                if (sourceCell.IsValid && sourceCell.InBounds(map) && !shockwaveWarningSources.Contains(sourceCell))
                {
                    shockwaveWarningSources.Add(sourceCell);
                }
            }

            BuildShockwaveWarningLanes(map, shockwaveWarningSources);
            shockwaveWarningEndTick = Find.TickManager.TicksGame + ShockwaveWarningIndicatorTicks;
        }

        // BuildShockwaveWarningLanes 负责根据参战小人的当前位置锁定有限数量的冲击危险带。
        private void BuildShockwaveWarningLanes(Map map, List<IntVec3> sourceCells)
        {
            shockwaveWarningLanes.Clear();
            if (map == null || sourceCells.NullOrEmpty())
            {
                return;
            }

            for (int i = 0; i < participants.Count && shockwaveWarningLanes.Count < ShockwaveMaxWarningLanes; i++)
            {
                Pawn pawn = participants[i];
                if (pawn == null || returnedPawns.Contains(pawn) || pawn.Map != map || !pawn.Spawned || pawn.Dead)
                {
                    continue;
                }

                IntVec3 sourceCell = NearestShockwaveSource(sourceCells, pawn.Position);
                SandWormShockwaveLane lane = new SandWormShockwaveLane(map, sourceCell, pawn.Position, ScaledShockwaveLaneHalfWidth());
                shockwaveWarningLanes.Add(lane);
            }
        }

        // AverageCells 负责计算一组地图格的平均中心，缺失时返回兜底格。
        private static IntVec3 AverageCells(List<IntVec3> cells, IntVec3 fallback)
        {
            if (cells.NullOrEmpty())
            {
                return fallback;
            }

            int count = 0;
            int x = 0;
            int z = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                IntVec3 cell = cells[i];
                if (!cell.IsValid)
                {
                    continue;
                }

                x += cell.x;
                z += cell.z;
                count++;
            }

            return count > 0 ? new IntVec3(x / count, 0, z / count) : fallback;
        }

        // DrawShockwaveWarningIndicators 负责在冲击波预警期间绘制锁定危险带和参战者受威胁状态。
        private void DrawShockwaveWarningIndicators(Map map)
        {
            if (!ShouldDrawShockwaveWarningLines() || shockwaveWarningEndTick <= Find.TickManager.TicksGame)
            {
                return;
            }

            DrawShockwaveWarningLanes(map);
            DrawShockwaveWarningArrowLines(map);
        }

        // DrawShockwaveWarningLanes 负责绘制每条危险带的中心流向和边界，让玩家可以明确走出危险范围。
        private void DrawShockwaveWarningLanes(Map map)
        {
            if (shockwaveWarningLanes.NullOrEmpty())
            {
                return;
            }

            for (int i = 0; i < shockwaveWarningLanes.Count; i++)
            {
                SandWormShockwaveLane lane = shockwaveWarningLanes[i];
                if (lane == null)
                {
                    continue;
                }

                DrawShockwaveLane(lane);
                if ((Find.TickManager.TicksGame + i * 7) % 18 == 0)
                {
                    ThrowShockwaveLaneDust(map, lane);
                }
            }
        }

        // DrawShockwaveWarningArrowLines 负责为每个参战小人绘制当前危险状态的短提示线。
        private void DrawShockwaveWarningArrowLines(Map map)
        {
            for (int i = 0; i < participants.Count; i++)
            {
                Pawn pawn = participants[i];
                if (pawn == null || returnedPawns.Contains(pawn) || pawn.Map != map || !pawn.Spawned || pawn.Dead)
                {
                    continue;
                }

                bool inLane = TryGetShockwaveLaneForPawn(pawn.Position, out SandWormShockwaveLane lane);
                if (!inLane)
                {
                    continue;
                }

                bool covered = ShockwaveBlockedByCover(map, lane.SourceCell, pawn.Position);
                DrawShockwaveArrowLine(lane.SourceCell.ToVector3Shifted(), pawn.DrawPos, covered);
            }
        }

        // DrawShockwaveLane 负责绘制一条地脉冲击危险带的中心线、边界线和运动箭头。
        private void DrawShockwaveLane(SandWormShockwaveLane lane)
        {
            int ticksLeft = Mathf.Max(0, shockwaveWarningEndTick - Find.TickManager.TicksGame);
            SandWormShockwaveVisualUtility.DrawWarningLane(lane, ticksLeft, ShockwaveWarningIndicatorTicks);
        }

        // DrawShockwaveArrowLine 负责绘制一条带运动箭头的冲击波预警线，并按掩体遮挡切换安全颜色。
        private static void DrawShockwaveArrowLine(Vector3 start, Vector3 end, bool covered)
        {
            SandWormShockwaveVisualUtility.DrawPawnThreatLink(start, end, covered);
        }

        // ShouldDrawShockwaveWarningLines 负责读取玩家设置，默认开启冲击波运动箭头指示线。
        private static bool ShouldDrawShockwaveWarningLines()
        {
            return SandWormMod.Settings == null || SandWormMod.Settings.showShockwaveWarningLines;
        }

        // ScaledShockwaveCooldownTicks 负责按词条倍率计算下一次地脉冲击冷却。
        private int ScaledShockwaveCooldownTicks()
        {
            float factor = runtimeModifiers != null ? runtimeModifiers.shockwaveCooldownFactor : 1f;
            return Mathf.Max(300, Mathf.RoundToInt(ShockwaveCooldownTicks * Mathf.Clamp(factor, 0.1f, 3f)));
        }

        // ScaledShockwaveLaneHalfWidth 负责按词条倍率计算地脉冲击危险带半宽。
        private float ScaledShockwaveLaneHalfWidth()
        {
            float factor = runtimeModifiers != null ? runtimeModifiers.shockwaveLaneWidthFactor : 1f;
            return ShockwaveLaneHalfWidth * Mathf.Clamp(factor, 0.25f, 3f);
        }

        // ShockwaveDamageFactor 负责返回词条叠加后的地脉冲击伤害倍率。
        private float ShockwaveDamageFactor()
        {
            return runtimeModifiers != null ? Mathf.Clamp(runtimeModifiers.shockwaveDamageFactor, 0.1f, 5f) : 1f;
        }

        // TryGetShockwaveLaneForPawn 负责查找当前小人所在的锁定危险带。
        private bool TryGetShockwaveLaneForPawn(IntVec3 pawnCell, out SandWormShockwaveLane lane)
        {
            lane = null;
            if (shockwaveWarningLanes.NullOrEmpty())
            {
                return false;
            }

            for (int i = 0; i < shockwaveWarningLanes.Count; i++)
            {
                SandWormShockwaveLane candidate = shockwaveWarningLanes[i];
                if (candidate != null && candidate.ContainsCell(pawnCell))
                {
                    lane = candidate;
                    return true;
                }
            }

            return false;
        }

        // ShockwaveDamageForLane 负责按小人与危险带中心线的距离区分正中命中和擦边命中。
        private int ShockwaveDamageForLane(SandWormShockwaveLane lane, IntVec3 pawnCell)
        {
            float lateralFraction = lane != null ? lane.LateralFraction(pawnCell) : 0f;
            int baseDamage = lateralFraction >= 0.72f ? ShockwaveGlancingDamageAmount : ShockwaveDamageAmount;
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * ShockwaveDamageFactor()));
        }

        // PushPawnFromShockwaveLane 负责在小人被冲击波命中后把位置轻微推离中心线。
        private static void PushPawnFromShockwaveLane(Map map, Pawn pawn, SandWormShockwaveLane lane)
        {
            if (map == null || pawn == null || lane == null || !pawn.Spawned)
            {
                return;
            }

            Vector3 offset = pawn.Position.ToVector3Shifted() - lane.Start;
            float sideSign = Vector3.Dot(offset, lane.Side) >= 0f ? 1f : -1f;
            IntVec3 destination = (pawn.Position.ToVector3Shifted() + lane.Side * sideSign * 3f + lane.Direction * 1.5f).ToIntVec3();
            if (destination.InBounds(map) && destination.Standable(map) && destination.GetFirstPawn(map) == null)
            {
                pawn.Position = destination;
                pawn.Notify_Teleported(endCurrentJob: false);
            }
        }

        // ThrowShockwaveDodgeFeedback 负责给已经离开危险带的小人播放安全反馈。
        private static void ThrowShockwaveDodgeFeedback(Map map, Pawn pawn)
        {
            if (map == null || pawn == null || !pawn.Spawned)
            {
                return;
            }

            FleckMaker.ThrowDustPuff(pawn.DrawPos + Gen.RandomHorizontalVector(0.45f), map, 1.2f);
        }

        // ThrowShockwaveCoverFeedback 负责给被碎石墙保护的小人播放更明显的掩体反馈。
        private static void ThrowShockwaveCoverFeedback(Map map, Pawn pawn)
        {
            if (map == null || pawn == null || !pawn.Spawned)
            {
                return;
            }

            FleckMaker.ThrowDustPuffThick(pawn.DrawPos + Gen.RandomHorizontalVector(0.55f), map, 1.8f, new Color(0.50f, 0.62f, 0.48f, 0.90f));
            SandWormShockwaveDustUtility.ThrowProtectedFeedback(map, pawn, ShockwaveRockVisualScaleFactor);
        }

        // ThrowShockwaveLaneDust 负责在危险带中心线附近间歇喷出裂地尘，增强预警可读性。
        private void ThrowShockwaveLaneDust(Map map, SandWormShockwaveLane lane)
        {
            if (map == null || lane == null)
            {
                return;
            }

            Vector3 loc = Vector3.Lerp(lane.Start, lane.End, Rand.Range(0.18f, 0.82f));
            loc += lane.Side * Rand.Range(-lane.HalfWidth, lane.HalfWidth);
            IntVec3 cell = loc.ToIntVec3();
            if (cell.InBounds(map) && !cell.Fogged(map))
            {
                FleckMaker.ThrowDustPuffThick(cell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.35f), map, Rand.Range(1.3f, 2.1f), new Color(0.78f, 0.50f, 0.28f, 0.85f));
            }

            int ticksLeft = Mathf.Max(0, shockwaveWarningEndTick - Find.TickManager.TicksGame);
            float charge = Mathf.Clamp01(1f - ticksLeft / (float)ShockwaveWarningIndicatorTicks);
            SandWormShockwaveDustUtility.ThrowWarningLaneDust(map, lane, charge, ShockwaveRockVisualScaleFactor);
        }

        // ThrowShockwaveLaneReleaseBursts 负责在冲击释放瞬间沿所有危险带播放推进尘爆。
        private void ThrowShockwaveLaneReleaseBursts(Map map)
        {
            if (map == null || shockwaveWarningLanes.NullOrEmpty())
            {
                return;
            }

            for (int i = 0; i < shockwaveWarningLanes.Count; i++)
            {
                SandWormShockwaveLane lane = shockwaveWarningLanes[i];
                if (lane != null)
                {
                    SandWormShockwaveDustUtility.ThrowReleaseLaneBurst(map, lane, ShockwaveRockVisualScaleFactor);
                }
            }
        }

        // ClearShockwaveWarningIndicators 负责清空冲击波预警线段状态。
        private void ClearShockwaveWarningIndicators()
        {
            shockwaveWarningSources?.Clear();
            shockwaveWarningLanes?.Clear();
            shockwaveWarningEndTick = -1;
        }

        // ClearShockwaveWalls 负责销毁并忘记当前冲击波生成的临时碎石墙。
        private void ClearShockwaveWalls(DestroyMode mode)
        {
            shockwaveWallsClearTick = -1;
            if (pendingShockwaveWallProjectiles == null)
            {
                pendingShockwaveWallProjectiles = new List<SandWormShockwaveRockProjectile>();
            }
            else
            {
                pendingShockwaveWallProjectiles.Clear();
            }

            if (shockwaveWalls == null)
            {
                shockwaveWalls = new List<Thing>();
                return;
            }

            for (int i = 0; i < shockwaveWalls.Count; i++)
            {
                Thing wall = shockwaveWalls[i];
                if (wall == null || wall.Destroyed)
                {
                    continue;
                }

                Map map = wall.MapHeld;
                IntVec3 cell = wall.PositionHeld;
                if (map != null && cell.IsValid)
                {
                    FleckMaker.ThrowDustPuff(cell.ToVector3Shifted(), map, 2.2f * ShockwaveRockVisualScaleFactor);
                }

                wall.Destroy(mode);
            }

            shockwaveWalls.Clear();
        }

        // TickShockwaveWallCleanup 负责在冲击波释放后一段时间再清理碎石墙，保留掩体成功反馈。
        private void TickShockwaveWallCleanup()
        {
            if (shockwaveWallsClearTick <= 0 || Find.TickManager.TicksGame < shockwaveWallsClearTick)
            {
                return;
            }

            ClearShockwaveWalls(DestroyMode.Vanish);
        }

        // TickResonanceEscalation 负责在战斗拖延到阈值后刷新小沙虫并施加额外地脉压力。
        private void TickResonanceEscalation(Map map)
        {
            if (map == null || resonanceState == null || !resonanceState.CanTrigger(Find.TickManager.TicksGame))
            {
                return;
            }

            if (!IsChallengeBossStillActive())
            {
                FinishChallengeSuccess();
                return;
            }

            SpawnResonanceSmallWorm(map);
            if (resonanceState.Level >= 2 && runtimeModifiers != null && runtimeModifiers.enableShockwaveAttack && nextShockwaveReadyTick > 0)
            {
                nextShockwaveReadyTick = Mathf.Min(nextShockwaveReadyTick, Find.TickManager.TicksGame + 900);
            }

            resonanceState.NotifyTriggered(Find.TickManager.TicksGame);
            TryShakeCamera(resonanceState.Level >= 2 ? 0.36f : 0.24f, 80);
            Messages.Message("SandWorm_SyndicateChallenge_ResonancePressure".Translate(resonanceState.TriggeredCount), MessageTypeDefOf.ThreatSmall, historical: false);
        }

        // SpawnResonanceSmallWorm 负责为共振倒计时刷新一条继承当前挑战小沙虫强化的增援。
        private void SpawnResonanceSmallWorm(Map map)
        {
            ThingDef smallWormDef = DefDatabase<ThingDef>.GetNamedSilentFail(SmallWormDefName);
            if (smallWormDef == null)
            {
                Log.Error("[SandWorm] Missing thing def: " + SmallWormDefName);
                return;
            }

            IntVec3 target = ParticipantsCenter(map);
            List<IntVec3> usedCells = new List<IntVec3>();
            if (challengeBossWorm != null && challengeBossWorm.Spawned)
            {
                usedCells.Add(challengeBossWorm.Position);
            }

            for (int i = 0; i < challengeSmallWorms.Count; i++)
            {
                SandWormThing worm = challengeSmallWorms[i];
                if (worm != null && worm.Spawned)
                {
                    usedCells.Add(worm.Position);
                }
            }

            IntVec3 cell = FindEdgeSpawnCell(map, target, smallWormDef, usedCells, ExtraWormMinDistance);
            Thing thing = ThingMaker.MakeThing(smallWormDef);
            SandWormThing smallWorm = GenSpawn.Spawn(thing, cell, map, WipeMode.VanishOrMoveAside) as SandWormThing;
            if (smallWorm == null)
            {
                return;
            }

            smallWorm.SetChallengeModifiers(
                runtimeModifiers != null && runtimeModifiers.smallWormHeadInstantKill,
                runtimeModifiers?.smallWormHitPointFactor ?? 1f);
            challengeSmallWorms.Add(smallWorm);
            FleckMaker.ThrowDustPuffThick(cell.ToVector3Shifted(), map, 5.2f, new Color(0.72f, 0.55f, 0.34f, 0.95f));
        }

        // SpawnShockwaveWalls 负责按全身源点调度飞石，在参战小人与沙虫之间落地形成临时掩体。
        private void SpawnShockwaveWalls(Map map, List<IntVec3> sourceCells)
        {
            ThingDef wallDef = DefDatabase<ThingDef>.GetNamedSilentFail(ShockwaveRockWallDefName);
            if (wallDef == null)
            {
                Log.Error("[SandWorm] Missing thing def: " + ShockwaveRockWallDefName);
                return;
            }

            TmpShockwaveCells.Clear();
            for (int i = 0; i < participants.Count; i++)
            {
                Pawn pawn = participants[i];
                if (pawn == null || returnedPawns.Contains(pawn) || pawn.Map != map || !pawn.Spawned || pawn.Dead)
                {
                    continue;
                }

                IntVec3 sourceCell = NearestShockwaveSource(sourceCells, pawn.Position);
                AddCoverCellsForPawn(map, sourceCell, pawn.Position);
            }

            AddAmbientShockwaveWallCells(map, sourceCells);
            for (int i = 0; i < TmpShockwaveCells.Count; i++)
            {
                IntVec3 cell = TmpShockwaveCells[i];
                IntVec3 sourceCell = NearestShockwaveSource(sourceCells, cell);
                ScheduleShockwaveWallProjectile(cell, map, sourceCell);
            }

            TmpShockwaveCells.Clear();
        }

        // AddCoverCellsForPawn 负责沿沙虫到小人的连线放置横向墙段，保证掩体出现在可读的位置。
        private static void AddCoverCellsForPawn(Map map, IntVec3 sourceCell, IntVec3 pawnCell)
        {
            Vector3 source = sourceCell.ToVector3Shifted();
            Vector3 pawn = pawnCell.ToVector3Shifted();
            Vector3 forward = pawn - source;
            forward.y = 0f;
            if (forward.sqrMagnitude < 16f)
            {
                return;
            }

            forward.Normalize();
            Vector3 perpendicular = new Vector3(-forward.z, 0f, forward.x);
            Vector3 center = Vector3.Lerp(source, pawn, 0.48f);
            for (int row = -1; row <= 1; row++)
            {
                for (int width = -2; width <= 2; width++)
                {
                    Vector3 offset = perpendicular * width + forward * row;
                    IntVec3 cell = (center + offset).ToIntVec3();
                    if (cell != pawnCell && cell != sourceCell && cell.InBounds(map) && !TmpShockwaveCells.Contains(cell))
                    {
                        TmpShockwaveCells.Add(cell);
                    }
                }
            }
        }

        // AddAmbientShockwaveWallCells 负责围绕多段身体源点补充随机碎石墙段，让地层隆起覆盖整条沙虫。
        private static void AddAmbientShockwaveWallCells(Map map, List<IntVec3> sourceCells)
        {
            for (int i = 0; i < ShockwaveAmbientWallGroups; i++)
            {
                IntVec3 sourceCell = sourceCells.RandomElementWithFallback(map.Center);
                Vector3 centerVec = sourceCell.ToVector3Shifted() + Gen.RandomHorizontalVector(Rand.Range(12f, Mathf.Max(13f, map.Size.x * 0.38f)));
                IntVec3 center = centerVec.ToIntVec3();
                center.x = Mathf.Clamp(center.x, 4, map.Size.x - 5);
                center.z = Mathf.Clamp(center.z, 4, map.Size.z - 5);
                Vector3 direction = Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0f, 360f));
                Vector3 perpendicular = new Vector3(-direction.z, 0f, direction.x);
                int halfWidth = Rand.RangeInclusive(1, 3);
                for (int width = -halfWidth; width <= halfWidth; width++)
                {
                    IntVec3 cell = (center.ToVector3Shifted() + perpendicular * width).ToIntVec3();
                    if (cell != sourceCell && cell.InBounds(map) && !TmpShockwaveCells.Contains(cell))
                    {
                        TmpShockwaveCells.Add(cell);
                    }
                }
            }
        }

        // ScheduleShockwaveWallProjectile 负责给目标格创建一块贝塞尔飞石，飞石落地后才真正生成墙体。
        private void ScheduleShockwaveWallProjectile(IntVec3 cell, Map map, IntVec3 sourceCell)
        {
            if (pendingShockwaveWallProjectiles.Count >= ShockwaveMaxWallProjectiles || !CanPlaceShockwaveWall(cell, map, sourceCell))
            {
                return;
            }

            float distance = (cell.ToVector3Shifted() - sourceCell.ToVector3Shifted()).MagnitudeHorizontal();
            int duration = Mathf.Clamp(Mathf.RoundToInt(distance * 2.8f) + Rand.RangeInclusive(88, 142), 150, 320);
            int startDelay = Rand.RangeInclusive(0, 130);
            SandWormShockwaveRockProjectile projectile = new SandWormShockwaveRockProjectile(
                sourceCell,
                cell,
                Find.TickManager.TicksGame + startDelay,
                duration,
                Rand.Range(0f, 360f),
                Rand.Range(-7f, 7f),
                Rand.Range(0.68f, 1.05f) * ShockwaveRockVisualScaleFactor);
            pendingShockwaveWallProjectiles.Add(projectile);
            ThrowShockwaveWallLaunchBurst(map, sourceCell);
        }

        // LandShockwaveWallProjectile 负责在飞石落地时再次检查落点，成功则生成临时墙，失败则只播放砸地尘土。
        private void LandShockwaveWallProjectile(SandWormShockwaveRockProjectile projectile, Map map)
        {
            if (projectile == null || projectile.Landed || map == null)
            {
                return;
            }

            projectile.MarkLanded();
            IntVec3 cell = projectile.TargetCell;
            if (!CanPlaceShockwaveWall(cell, map, projectile.SourceCell))
            {
                ThrowShockwaveWallLandingDust(map, cell, false);
                return;
            }

            ThingDef wallDef = DefDatabase<ThingDef>.GetNamedSilentFail(ShockwaveRockWallDefName);
            if (wallDef == null)
            {
                return;
            }

            Thing wall = ThingMaker.MakeThing(wallDef, wallDef.MadeFromStuff ? DefDatabase<ThingDef>.GetNamedSilentFail("BlocksSandstone") : null);
            Thing spawned = GenSpawn.Spawn(wall, cell, map, WipeMode.Vanish);
            if (spawned != null)
            {
                shockwaveWalls.Add(spawned);
                ThrowShockwaveWallLandingDust(map, cell, true);
                TryShakeCamera(0.12f);
            }
        }

        // ThrowShockwaveWallLandingDust 负责在飞石砸落时生成厚重中心尘和周围碎尘，强化墙体从落点砸出的重量感。
        private static void ThrowShockwaveWallLandingDust(Map map, IntVec3 cell, bool wallSpawned)
        {
            Vector3 center = cell.ToVector3Shifted();
            Color dustColor = new Color(0.75f, 0.58f, 0.36f, 1f);
            FleckDef blastDry = BlastDryFleckDef;
            if (blastDry != null && cell.ShouldSpawnMotesAt(map))
            {
                FleckMaker.Static(center, map, blastDry, (wallSpawned ? 2.65f : 2.05f) * ShockwaveRockVisualScaleFactor);
            }

            FleckMaker.ThrowAirPuffUp(center, map);
            FleckMaker.ThrowDustPuffThick(center, map, (wallSpawned ? 5.8f : 4.4f) * ShockwaveRockVisualScaleFactor, dustColor);
            FleckMaker.ThrowTornadoDustPuff(center + Gen.RandomHorizontalVector(0.30f), map, (wallSpawned ? 2.35f : 1.75f) * ShockwaveRockVisualScaleFactor, new Color(0.62f, 0.50f, 0.38f, 0.92f));

            int dustCount = wallSpawned ? 18 : 12;
            for (int i = 0; i < dustCount; i++)
            {
                Vector3 loc = center + Gen.RandomHorizontalVector(Rand.Range(0.45f, wallSpawned ? 3.00f : 2.20f));
                FleckMaker.ThrowDustPuff(loc, map, Rand.Range(1.75f, 3.15f) * ShockwaveRockVisualScaleFactor);
                if (i % 3 == 0)
                {
                    FleckMaker.ThrowDustPuffThick(loc + Gen.RandomHorizontalVector(0.28f), map, Rand.Range(0.85f, 1.45f) * ShockwaveRockVisualScaleFactor, dustColor);
                }
            }
        }

        // ThrowShockwaveWallLaunchBurst 负责在飞石从沙虫身体附近破土时喷出短促裂地尘。
        private static void ThrowShockwaveWallLaunchBurst(Map map, IntVec3 sourceCell)
        {
            if (map == null || !sourceCell.InBounds(map) || sourceCell.Fogged(map))
            {
                return;
            }

            Vector3 center = sourceCell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.65f);
            FleckDef blastDry = BlastDryFleckDef;
            if (blastDry != null && sourceCell.ShouldSpawnMotesAt(map))
            {
                FleckMaker.Static(center, map, blastDry, Rand.Range(0.95f, 1.35f) * ShockwaveRockVisualScaleFactor);
            }

            FleckMaker.ThrowAirPuffUp(center, map);
            FleckMaker.ThrowDustPuffThick(center, map, Rand.Range(2.6f, 3.7f) * ShockwaveRockVisualScaleFactor, new Color(0.72f, 0.55f, 0.34f, 0.95f));
            for (int i = 0; i < 5; i++)
            {
                FleckMaker.ThrowDustPuff(center + Gen.RandomHorizontalVector(Rand.Range(0.35f, 1.45f)), map, Rand.Range(1.15f, 1.85f) * ShockwaveRockVisualScaleFactor);
            }
        }

        // BlastDryFleckDef 负责缓存原版干燥爆裂尘 Fleck，缺失时允许表现自动降级为普通尘土。
        private static FleckDef BlastDryFleckDef
        {
            get
            {
                if (blastDryFleckDef == null)
                {
                    blastDryFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(BlastDryFleckDefName);
                }

                return blastDryFleckDef;
            }
        }

        // CanPlaceShockwaveWall 负责过滤会覆盖小人、沙虫、已有建筑或不可用地形的碎石墙落点。
        private bool CanPlaceShockwaveWall(IntVec3 cell, Map map, IntVec3 sourceCell)
        {
            if (!cell.InBounds(map) || cell.Fogged(map) || cell == sourceCell || cell.GetEdifice(map) != null || cell.GetFirstPawn(map) != null)
            {
                return false;
            }

            if (cell.Impassable(map) || !cell.Standable(map))
            {
                return false;
            }

            for (int i = 0; i < participants.Count; i++)
            {
                Pawn pawn = participants[i];
                if (pawn != null && pawn.Map == map && pawn.Spawned && pawn.Position == cell)
                {
                    return false;
                }
            }

            return true;
        }

        // ShockwaveBlockedByCover 负责沿冲击源到小人之间检查是否有碎石墙或其他不可视阻挡物。
        private static bool ShockwaveBlockedByCover(Map map, IntVec3 sourceCell, IntVec3 pawnCell)
        {
            foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(sourceCell, pawnCell))
            {
                if (cell == sourceCell || cell == pawnCell)
                {
                    continue;
                }

                if (!cell.InBounds(map))
                {
                    return true;
                }

                Thing edifice = cell.GetEdifice(map);
                if (edifice != null && edifice.def.passability == Traversability.Impassable)
                {
                    return true;
                }

                if (!cell.CanBeSeenOverFast(map))
                {
                    return true;
                }
            }

            return false;
        }

        // TickShockwaveRockProjectiles 负责推进飞石落地，并在抵达目标格时生成或跳过墙体。
        private void TickShockwaveRockProjectiles(Map map)
        {
            if (pendingShockwaveWallProjectiles == null || pendingShockwaveWallProjectiles.Count == 0)
            {
                return;
            }

            int tick = Find.TickManager.TicksGame;
            for (int i = pendingShockwaveWallProjectiles.Count - 1; i >= 0; i--)
            {
                SandWormShockwaveRockProjectile projectile = pendingShockwaveWallProjectiles[i];
                if (projectile == null || projectile.Landed)
                {
                    pendingShockwaveWallProjectiles.RemoveAt(i);
                    continue;
                }

                if (projectile.ReadyToLand(tick))
                {
                    LandShockwaveWallProjectile(projectile, map);
                    pendingShockwaveWallProjectiles.RemoveAt(i);
                }
            }
        }

        // DrawShockwaveRockProjectiles 负责在地图更新阶段绘制所有尚未落地的贝塞尔飞石。
        private void DrawShockwaveRockProjectiles(Map map)
        {
            if (pendingShockwaveWallProjectiles == null || pendingShockwaveWallProjectiles.Count == 0)
            {
                return;
            }

            int tick = Find.TickManager.TicksGame;
            for (int i = 0; i < pendingShockwaveWallProjectiles.Count; i++)
            {
                pendingShockwaveWallProjectiles[i]?.Draw(map, tick);
            }
        }

        // LandAllShockwaveProjectiles 负责在伤害结算前强制完成所有飞石，保证掩体判定稳定。
        private void LandAllShockwaveProjectiles(Map map)
        {
            if (pendingShockwaveWallProjectiles == null || pendingShockwaveWallProjectiles.Count == 0)
            {
                return;
            }

            for (int i = 0; i < pendingShockwaveWallProjectiles.Count; i++)
            {
                LandShockwaveWallProjectile(pendingShockwaveWallProjectiles[i], map);
            }

            pendingShockwaveWallProjectiles.Clear();
        }

        // NormalizeShockwaveSources 负责清洗冲击源列表，缺失时回退到沙虫当前位置。
        private static void NormalizeShockwaveSources(List<IntVec3> sourceCells, Map map, SandWormThing worm)
        {
            if (sourceCells == null)
            {
                return;
            }

            TmpShockwaveSources.Clear();
            for (int i = 0; i < sourceCells.Count; i++)
            {
                IntVec3 cell = sourceCells[i];
                if (cell.IsValid && cell.InBounds(map) && !TmpShockwaveSources.Contains(cell))
                {
                    TmpShockwaveSources.Add(cell);
                }
            }

            if (TmpShockwaveSources.Count == 0)
            {
                IntVec3 fallback = worm != null ? worm.Position : map.Center;
                if (fallback.IsValid && fallback.InBounds(map))
                {
                    TmpShockwaveSources.Add(fallback);
                }
            }

            sourceCells.Clear();
            sourceCells.AddRange(TmpShockwaveSources);
            TmpShockwaveSources.Clear();
        }

        // NearestShockwaveSource 负责为指定目标格找到最近的沙虫身体冲击源点。
        private static IntVec3 NearestShockwaveSource(List<IntVec3> sourceCells, IntVec3 targetCell)
        {
            if (sourceCells.NullOrEmpty())
            {
                return targetCell;
            }

            IntVec3 best = sourceCells[0];
            float bestDistance = (best - targetCell).LengthHorizontalSquared;
            for (int i = 1; i < sourceCells.Count; i++)
            {
                float distance = (sourceCells[i] - targetCell).LengthHorizontalSquared;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = sourceCells[i];
                }
            }

            return best;
        }

        // TryShakeCamera 负责只在玩家正在查看挑战地图时播放冲击波震屏。
        private void TryShakeCamera(float magnitude, int durationTicks = 0)
        {
            if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap != ChallengeMap || !WorldRendererUtility.DrawingMap)
            {
                return;
            }

            if (durationTicks > 0)
            {
                Find.CameraDriver?.shaker?.DoShake(magnitude, durationTicks);
            }
            else
            {
                Find.CameraDriver?.shaker?.DoShake(magnitude);
            }
        }

        // CheckParticipantFailureStates 负责把倒地、濒死或离开挑战地图的小人满血送回。
        private void CheckParticipantFailureStates()
        {
            for (int i = 0; i < participants.Count; i++)
            {
                Pawn pawn = participants[i];
                if (pawn == null || returnedPawns.Contains(pawn))
                {
                    continue;
                }

                if (!IsParticipantStillFighting(pawn))
                {
                    ReturnParticipant(pawn, "SandWorm_SyndicateChallenge_Returned".Translate(pawn?.LabelShortCap ?? string.Empty));
                }
            }

            if (AllParticipantsReturned())
            {
                FinishFailure();
            }
        }

        // CheckChallengeBossState 负责在 Boss 已进入死亡下沉或被销毁时收束直启挑战。
        private void CheckChallengeBossState()
        {
            if (wormSpawned && challengeBossWorm != null && !IsChallengeBossStillActive())
            {
                FinishChallengeSuccess();
            }
        }

        // IsParticipantStillFighting 负责判断小人是否仍然能继续挑战。
        private bool IsParticipantStillFighting(Pawn pawn)
        {
            if (pawn == null || pawn.Destroyed || pawn.Dead || pawn.Map != ChallengeMap || !pawn.Spawned || pawn.Downed)
            {
                return false;
            }

            if (HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) <= RescueBloodLossTicks)
            {
                return false;
            }

            Hediff lifeThreatening = pawn.health.hediffSet.hediffs.Find(hediff => hediff.IsCurrentlyLifeThreatening || hediff.IsLethal);
            return lifeThreatening == null;
        }

        // IsChallengeBossStillActive 负责判断直启挑战 Boss 是否仍然能继续战斗。
        private bool IsChallengeBossStillActive()
        {
            return challengeBossWorm != null
                && !challengeBossWorm.Destroyed
                && !challengeBossWorm.IsDying
                && challengeBossWorm.Spawned
                && challengeBossWorm.Map == ChallengeMap;
        }

        // ReturnParticipant 负责恢复小人健康并把他送回原地图或任意主基地。
        private void ReturnParticipant(Pawn pawn, string message)
        {
            if (pawn == null || returnedPawns.Contains(pawn))
            {
                return;
            }

            int index = participants.IndexOf(pawn);
            Map destinationMap = index >= 0 && index < originMaps.Count ? originMaps[index] : null;
            IntVec3 destinationCell = index >= 0 && index < originPositions.Count ? originPositions[index] : IntVec3.Invalid;
            if (destinationMap == null || !Find.Maps.Contains(destinationMap))
            {
                destinationMap = Find.AnyPlayerHomeMap;
                destinationCell = destinationMap != null ? DropCellFinder.TradeDropSpot(destinationMap) : IntVec3.Invalid;
            }

            if (destinationMap == null)
            {
                returnedPawns.Add(pawn);
                return;
            }

            RestorePawnForReturn(pawn);
            if (pawn.Spawned)
            {
                pawn.DeSpawn(DestroyMode.Vanish);
            }

            IntVec3 cell = FindSpawnCellNear(destinationMap, destinationCell.IsValid ? destinationCell : destinationMap.Center, index);
            GenSpawn.Spawn(pawn, cell, destinationMap, WipeMode.VanishOrMoveAside);
            pawn.Notify_Teleported();
            pawn.drafter.Drafted = false;
            returnedPawns.Add(pawn);
            Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent, historical: false);
        }

        // ApplyChallengeHediffs 负责给参战小人施加通过 Hediff 实现的挑战限制。
        private void ApplyChallengeHediffs(Pawn pawn)
        {
            RemoveChallengeHediffs(pawn);
            if (pawn == null || pawn.health == null || runtimeModifiers == null)
            {
                return;
            }

            if (runtimeModifiers.pawnMoveSuppressionLevel > 0)
            {
                ApplyChallengeHediff(pawn, MoveSuppressionHediffDefName, Mathf.Clamp(runtimeModifiers.pawnMoveSuppressionLevel, 1, 3));
            }

            if (runtimeModifiers.pawnAimSuppressionLevel > 0)
            {
                ApplyChallengeHediff(pawn, AimSuppressionHediffDefName, Mathf.Clamp(runtimeModifiers.pawnAimSuppressionLevel, 1, 2));
            }
        }

        // ApplyChallengeHediff 负责按 DefName 给参战小人添加指定严重度的挑战临时 Hediff。
        private static void ApplyChallengeHediff(Pawn pawn, string hediffDefName, float severity)
        {
            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffDefName);
            if (hediffDef == null)
            {
                Log.Error("[SandWorm] Missing hediff def: " + hediffDefName);
                return;
            }

            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            hediff.Severity = severity;
            pawn.health.AddHediff(hediff);
        }

        // RemoveChallengeHediffs 负责移除辛迪加挑战给小人添加的临时 Hediff。
        private static void RemoveChallengeHediffs(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return;
            }

            RemoveChallengeHediff(pawn, MoveSuppressionHediffDefName);
            RemoveChallengeHediff(pawn, AimSuppressionHediffDefName);
        }

        // RemoveChallengeHediff 负责从小人身上移除指定 DefName 的挑战临时 Hediff。
        private static void RemoveChallengeHediff(Pawn pawn, string hediffDefName)
        {
            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffDefName);
            if (hediffDef == null)
            {
                return;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            while (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            }
        }

        // RestorePawnForReturn 负责清除挑战伤害、挑战 Hediff 和倒地状态，让小人满血回到原地图。
        private static void RestorePawnForReturn(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
            {
                return;
            }

            RemoveChallengeHediffs(pawn);
            TmpHediffs.Clear();
            TmpHediffs.AddRange(pawn.health.hediffSet.hediffs);
            for (int i = 0; i < TmpHediffs.Count; i++)
            {
                Hediff hediff = TmpHediffs[i];
                if (hediff == null || !pawn.health.hediffSet.hediffs.Contains(hediff))
                {
                    continue;
                }

                if (hediff is Hediff_Injury injury && !injury.IsPermanent())
                {
                    pawn.health.RemoveHediff(hediff);
                }
                else if (hediff is Hediff_MissingPart missingPart && missingPart.Part != null)
                {
                    pawn.health.RestorePart(missingPart.Part);
                }
                else if (hediff.def == HediffDefOf.BloodLoss)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }

            TmpHediffs.Clear();
            pawn.health.forceDowned = false;
            HealthStateField?.SetValue(pawn.health, PawnHealthState.Mobile);
            pawn.health.hediffSet.DirtyCache();
            pawn.health.capacities.Notify_CapacityLevelsDirty();
            pawn.health.summaryHealth.Notify_HealthChanged();
            pawn.jobs?.StopAll();
            pawn.pather?.StopDead();
            pawn.stances?.CancelBusyStanceHard();
            if (pawn.mindState?.mentalStateHandler?.CurState != null)
            {
                pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
            }
        }

        // ReturnAllRemaining 负责异常清图时把所有尚未返回的小人送回。
        private void ReturnAllRemaining()
        {
            for (int i = 0; i < participants.Count; i++)
            {
                Pawn pawn = participants[i];
                ReturnParticipant(pawn, "SandWorm_SyndicateChallenge_Returned".Translate(pawn?.LabelShortCap ?? string.Empty));
            }
        }

        // FinishFailure 负责在全员返回后宣告失败并销毁临时地图。
        private void FinishFailure()
        {
            if (cleanupQueued)
            {
                return;
            }

            cleanupQueued = true;
            Find.LetterStack.ReceiveLetter(
                "SandWorm_SyndicateChallenge_Failed_Label".Translate(),
                "SandWorm_SyndicateChallenge_Failed_Text".Translate(),
                LetterDefOf.NegativeEvent,
                new LookTargets(participants));
            SandWormQuestUtility.ClearAbnormalSandstormAfterKill(ChallengeMap);
            ClearShockwaveWalls(DestroyMode.Vanish);
            ClearShockwaveRingVisual();
            ClearShockwaveWarningIndicators();

            if (site != null && !site.Destroyed)
            {
                site.MarkForRemoval();
            }

            ClearState();
        }

        // FinishChallengeSuccess 负责在直启挑战 Boss 被击杀后回收剩余小人并关闭临时挑战地图。
        private void FinishChallengeSuccess()
        {
            if (cleanupQueued)
            {
                return;
            }

            cleanupQueued = true;
            Map map = ChallengeMap;
            for (int i = 0; i < participants.Count; i++)
            {
                Pawn pawn = participants[i];
                ReturnParticipant(pawn, "SandWorm_SyndicateChallenge_Returned".Translate(pawn?.LabelShortCap ?? string.Empty));
            }

            SandWormQuestUtility.ClearAbnormalSandstormAfterKill(map);
            ClearShockwaveWalls(DestroyMode.Vanish);
            ClearShockwaveRingVisual();
            ClearShockwaveWarningIndicators();
            resonanceState.Reset();
            if (site != null && !site.Destroyed)
            {
                site.MarkForRemoval();
            }

            ClearState();
        }

        // ClearState 负责清空当前挑战引用，避免旧实例继续参与 tick。
        private void ClearState()
        {
            site = null;
            participants.Clear();
            returnedPawns.Clear();
            originMaps.Clear();
            originPositions.Clear();
            challengeBossWorm = null;
            challengeSmallWorms.Clear();
            ClearShockwaveWalls(DestroyMode.Vanish);
            ClearShockwaveRingVisual();
            ClearShockwaveWarningIndicators();
            runtimeModifiers = new SandWormChallengeRuntimeModifiers();
            resonanceState.Reset();
            startTick = -1;
            nextShockwaveReadyTick = -1;
            participantFailureCheckStartTick = -1;
            wormSpawned = false;
            cleanupQueued = false;
        }

        // CanCheckParticipantFailureStates 负责在沙虫刷新完成并经过短暂稳定窗口后才允许失败检查。
        private bool CanCheckParticipantFailureStates()
        {
            return wormSpawned
                && participantFailureCheckStartTick > 0
                && Find.TickManager.TicksGame >= participantFailureCheckStartTick;
        }

        // AllParticipantsReturned 负责判断本次挑战是否已经没有参战者留在挑战地图。
        private bool AllParticipantsReturned()
        {
            if (participants.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < participants.Count; i++)
            {
                if (!returnedPawns.Contains(participants[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // IsActiveParticipantOnChallengeMap 负责判断 Harmony 捕获到的小人是否属于当前挑战。
        private bool IsActiveParticipantOnChallengeMap(Pawn pawn)
        {
            return ChallengeActive
                && pawn != null
                && !returnedPawns.Contains(pawn)
                && participants.Contains(pawn)
                && pawn.Map == ChallengeMap;
        }

        // ParticipantsCenter 负责计算沙虫刷新时远离队伍的参考中心。
        private IntVec3 ParticipantsCenter(Map map)
        {
            int count = 0;
            int x = 0;
            int z = 0;
            for (int i = 0; i < participants.Count; i++)
            {
                Pawn pawn = participants[i];
                if (pawn != null && pawn.Spawned && pawn.Map == map && !returnedPawns.Contains(pawn))
                {
                    x += pawn.Position.x;
                    z += pawn.Position.z;
                    count++;
                }
            }

            return count > 0 ? new IntVec3(x / count, 0, z / count) : map.Center;
        }

        // FindSpawnCellNear 负责在目标点附近寻找可站立的投送格。
        private static IntVec3 FindSpawnCellNear(Map map, IntVec3 center, int index)
        {
            if (center.IsValid && center.InBounds(map) && center.Standable(map))
            {
                return CellFinder.RandomClosewalkCellNear(center, map, 8 + index);
            }

            if (CellFinderLoose.TryGetRandomCellWith(cell => cell.Standable(map) && !cell.Fogged(map), map, 1000, out IntVec3 result))
            {
                return result;
            }

            return CellFinder.RandomCell(map);
        }

        // FindEdgeSpawnCell 负责在地图边缘寻找离队伍尽量远的沙虫刷新点。
        private static IntVec3 FindEdgeSpawnCell(Map map, IntVec3 targetCell, ThingDef wormDef)
        {
            return FindEdgeSpawnCell(map, targetCell, wormDef, null, 0);
        }

        // FindEdgeSpawnCell 负责在地图边缘寻找离队伍尽量远、且避开已用落点的沙虫刷新点。
        private static IntVec3 FindEdgeSpawnCell(Map map, IntVec3 targetCell, ThingDef wormDef, List<IntVec3> usedCells, int minDistance)
        {
            IntVec3 bestCell = IntVec3.Invalid;
            float bestDistanceSq = -1f;
            CellRect edgeRect = CellRect.WholeMap(map).ContractedBy(1);

            for (int i = 0; i < 360; i++)
            {
                IntVec3 candidate = InsetFromEdge(CellFinder.RandomEdgeCell(map), map);
                if (!candidate.InBounds(map) || candidate.Fogged(map))
                {
                    continue;
                }

                if (candidate.Standable(map) && GenSpawn.CanSpawnAt(wormDef, candidate, map, Rot4.North, canWipeEdifices: false))
                {
                    if (IsTooCloseToUsedSpawnCell(candidate, usedCells, minDistance))
                    {
                        continue;
                    }

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
            IntVec3 fallback = new IntVec3(x, 0, z);
            if (!IsTooCloseToUsedSpawnCell(fallback, usedCells, Mathf.Max(6, minDistance / 2)))
            {
                return fallback;
            }

            if (CellFinderLoose.TryGetRandomCellWith(cell =>
                cell.Standable(map)
                && !cell.Fogged(map)
                && GenSpawn.CanSpawnAt(wormDef, cell, map, Rot4.North, canWipeEdifices: false)
                && !IsTooCloseToUsedSpawnCell(cell, usedCells, Mathf.Max(6, minDistance / 2)), map, 1000, out IntVec3 result))
            {
                return result;
            }

            return fallback;
        }

        // IsTooCloseToUsedSpawnCell 负责防止多只挑战沙虫挤在同一个边缘刷新点。
        private static bool IsTooCloseToUsedSpawnCell(IntVec3 candidate, List<IntVec3> usedCells, int minDistance)
        {
            if (usedCells == null || minDistance <= 0)
            {
                return false;
            }

            int minDistanceSquared = minDistance * minDistance;
            for (int i = 0; i < usedCells.Count; i++)
            {
                IntVec3 usedCell = usedCells[i];
                if (usedCell.IsValid && (candidate - usedCell).LengthHorizontalSquared < minDistanceSquared)
                {
                    return true;
                }
            }

            return false;
        }

        // InsetFromEdge 负责把边缘随机格向地图内部收缩，避免沙虫出生在不可用边界。
        private static IntVec3 InsetFromEdge(IntVec3 cell, Map map)
        {
            IntVec3 clamped = new IntVec3(
                Mathf.Clamp(cell.x, 1, map.Size.x - 2),
                0,
                Mathf.Clamp(cell.z, 1, map.Size.z - 2));

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

            return clamped;
        }

        // EnsureListsNotNull 负责在读档后修复可能为空的集合字段。
        private void EnsureListsNotNull()
        {
            if (participants == null)
            {
                participants = new List<Pawn>();
            }

            if (returnedPawns == null)
            {
                returnedPawns = new List<Pawn>();
            }

            if (originMaps == null)
            {
                originMaps = new List<Map>();
            }

            if (originPositions == null)
            {
                originPositions = new List<IntVec3>();
            }

            if (challengeSmallWorms == null)
            {
                challengeSmallWorms = new List<SandWormThing>();
            }

            if (shockwaveWalls == null)
            {
                shockwaveWalls = new List<Thing>();
            }

            if (pendingShockwaveWallProjectiles == null)
            {
                pendingShockwaveWallProjectiles = new List<SandWormShockwaveRockProjectile>();
            }

            if (shockwaveRingSources == null)
            {
                shockwaveRingSources = new List<IntVec3>();
            }

            if (shockwaveWarningSources == null)
            {
                shockwaveWarningSources = new List<IntVec3>();
            }

            if (shockwaveWarningLanes == null)
            {
                shockwaveWarningLanes = new List<SandWormShockwaveLane>();
            }

            if (runtimeModifiers == null)
            {
                runtimeModifiers = new SandWormChallengeRuntimeModifiers();
            }

            if (resonanceState == null)
            {
                resonanceState = new SandWormChallengeResonanceState();
            }

            participants.RemoveAll(pawn => pawn == null);
            returnedPawns.RemoveAll(pawn => pawn == null);
            challengeSmallWorms.RemoveAll(worm => worm == null || worm.Destroyed);
            shockwaveWalls.RemoveAll(wall => wall == null || wall.Destroyed);
            pendingShockwaveWallProjectiles.RemoveAll(projectile => projectile == null || projectile.Landed);
            shockwaveWarningSources.RemoveAll(cell => !cell.IsValid || ChallengeMap != null && !cell.InBounds(ChallengeMap));
            shockwaveWarningLanes.RemoveAll(lane => lane == null);
            while (originMaps.Count < participants.Count)
            {
                originMaps.Add(Find.AnyPlayerHomeMap);
            }

            while (originPositions.Count < participants.Count)
            {
                Map map = Find.AnyPlayerHomeMap;
                originPositions.Add(map != null ? map.Center : IntVec3.Invalid);
            }
        }
    }

    // SandWormSyndicateChallengeDeathPatch 负责在参战小人真正死亡前拦截并改为挑战失败送回。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class SandWormSyndicateChallengeDeathPatch
    {
        // Prefix 负责阻止原版 Kill 继续执行，避免挑战副本里产生真实死亡和尸体。
        public static bool Prefix(Pawn __instance)
        {
            SandWormSyndicateChallengeState state = Current.Game?.GetComponent<SandWormSyndicateChallengeState>();
            return state == null || !state.TryProtectPawnFromDeath(__instance);
        }
    }
}
