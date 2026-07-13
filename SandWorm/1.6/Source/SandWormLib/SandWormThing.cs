// ════════════════════════════════════════════════════════
//  SandWormThing.cs 改造：基于距离插值的骨骼跟随 & 边缘转向修复
// ════════════════════════════════════════════════════════

using System.Collections.Generic;
using ChezhouLib.ALLmap;
using RimWorld;
using RimWorld.Planet;
using SandWormLib.SandWormBehavior;
using UnityEngine;
using Verse.AI;
using Verse;
using Verse.Sound;

namespace SandWormLib
{
    // SandWormThing 负责提供沙虫类实体的通用视觉、行为、命中代理和伤害承接逻辑。
    public class SandWormThing : ThingWithComps, IAttackTarget
    {
        private const string PrefabKey = SandWormAssets.RuchongPrefabKey;
        private const string EntranceSongDefName = "SandWorm_LeviathanEntranceSong";
        private const string RockMoveSoundDefName = "SandWorm_LeviathanRockMove";
        private const string RockChargeSoundDefName = "SandWorm_LeviathanRockCharge";
        private const int WanderRockSoundIntervalTicks = 240;
        private const int ChargeRockSoundIntervalTicks = 90;

        // ── 基准视觉参数（对应 sizeMultiplier = 1.0）──
        private const float BaseVisualScale = 8f;
        private const float BaseHeightOffset = -6f;
        private const int SpawnDestroyGraceTicks = 180;
        internal const float ViewTiltXMaxDeg = -25f;
        internal const float BankMaxDeg = 12f;
        internal const float BankLerp = 0.015f;
        private const float WormParticleSortingFudge = 8f;

        // ── 骨骼/轨迹/命中区基准值 ──
        private const int BoneCount = 19;
        private const int BaseHistoryLength = 2400;
        private const float BaseHitProxyWidth = 28f;
        private const float BaseHitProxyLength = 40f;

        // ── 路径绘制 ──
        private const int PathDrawStep = 24;
        private const float PathDrawAltitude = 0.2f;
        private const int ProjectedPathTicks = 180;
        private const int ProjectedPathStep = 6;
        private const float ProjectedPathLineWidth = 2.5f;
        private const float TargetLineWidth = 3.5f;

        // ── Wander / Charge 基准参数（预测路径使用；真实行为由 Wander/Charge 类持有） ──
        private const float BaseWanderMoveSpeed = 0.042f;
        private const float BaseWanderMaxTurnPerTick = 0.12f;
        private const float WanderNoiseSpeed = 0.004f;
        private const float WanderTurnBlend = 0.08f;
        private const float WanderReturnMargin = 12f;
        private const float WanderReturnForce = 0.18f;
        private const float WanderReturnThresholdFactor = 0.18f;
        private const float WanderBiasTowardPlayerForce = 0.006f;
        private const float WanderBiasTowardPlayerMaxTurn = 0.04f;
        private const int WanderMinTicksBeforeCharge = 240;

        private const float BaseChargeMoveSpeed = 0.224f;
        private const float BaseChargeMaxTurnPerTick = 0.65f;
        private const float ChargeTurnBlend = 0.18f;
        private const float ChargeDistance = float.MaxValue;
        private const int ChargeBoundaryMargin = 8;
        private const int ChargeMaxDurationTicks = 600;
        private const int ChargeMinCommitTicks = 30;
        private const int ChargeCooldownTicks = 900;
        private const float ChargeLockAngleThreshold = 7f;
        private const int TargetSearchCacheIntervalTicks = 30;

        // ── 尺寸系统 ──
        private float sizeMultiplier = 1f;
        private bool challengeHeadInstantKillOverride;
        private float challengeHitPointFactor = 1f;
        private float challengeChargeCooldownFactor = 1f;
        private float challengeMaxIncomingDamagePerHit = -1f;
        private bool challengeShockwaveEnabled;
        private bool challengeSupportWorm;
        private bool suppressQuestKillReward;
        private int historyLength;
        private float hitProxyWidthScaled;
        private float hitProxyLengthScaled;

        public float SizeMultiplier => sizeMultiplier;
        public float VisualScale => BaseVisualScale * sizeMultiplier;
        protected bool ChallengeHeadInstantKillOverride => challengeHeadInstantKillOverride;
        protected virtual float InitialSizeMultiplier => 1f;
        protected virtual int BaseMaxHitPoints => 50000;
        protected virtual string HitProxyDefName => "SandWorm_HitProxy";
        internal virtual bool AllowsHeadInstantKill => challengeHeadInstantKillOverride || SandWormMod.Settings == null || SandWormMod.Settings.enableHeadInstantKill;
        internal float HitProxyWidthScaled => hitProxyWidthScaled;
        internal float HitProxyLengthScaled => hitProxyLengthScaled;
        internal float MoveSpeedScale => sizeMultiplier * GetMoveSpeedMultiplier();
        internal float ChargeCooldownFactor => challengeChargeCooldownFactor;
        internal bool ChallengeShockwaveEnabled => challengeShockwaveEnabled;
        internal float TurnRateScale => 1f / Mathf.Sqrt(Mathf.Max(0.1f, sizeMultiplier));
        internal static float HeightOffset => BaseHeightOffset + GetModelYOffset();

        // ── 预测路径缓存 ──
        // 缓存的是世界坐标（相机可能移动，屏幕坐标不能缓存）
        private Vector3[] cachedProjectedPoints;
        private int cachedProjectedPointCount;
        private int lastProjectedRefreshTick = -99999;
        private int lastHitProxyDebugLogTick = -99999;
        private int lastBiasTargetRefreshTick = -99999;
        private int lastChargeTargetRefreshTick = -99999;
        private Vector3? cachedBiasTargetPosition;
        private Thing cachedChargeTarget;

        // ── HitProxy 降频同步 ──
        private int lastHitProxySyncTick = -99999;

        // ── 骨骼查找缓存 ──
        private readonly Dictionary<string, Transform> boneLookupCache = new Dictionary<string, Transform>();

        private List<SandWormHitProxyThing> hitProxies = new List<SandWormHitProxyThing>(BoneCount);

        private Transform[] bones;
        private Quaternion[] boneOffsets;

        // 每个 bone 到头部的固定距离（模型空间，不随速度变化）
        private float[] boneDistances;

        private Quaternion[] rotHistory;
        private Vector3[] posHistory;
        private int historyHead;

        private GameObject wrapperObject;
        private GameObject instance;
        private Transform headTransform;

        private Vector3 exactPos;
        private Vector3 velocity;
        private float headYawDeg;
        private float headPitchDeg;    // 俯仰角（local空间：正=世界朝下，负=世界朝上）
        private float bankDeg;
        private float noiseOffset;
        private float noiseTime;
        private bool surfacePressureEnabled = true;
        private bool isDying;
        private float deathSinkOffsetY;
        private bool hasForcedInitialHeading;
        private float forcedInitialHeadingDeg;

        private Vector3 mapCenter;
        private float mapRadius;

        private SandWormBehaviorContext behaviorContext;
        private SandWormStateMachine stateMachine;
        private SandWormBlackboard blackboard = new SandWormBlackboard();
        private SongDef entranceSongDef;
        private SoundDef rockMoveSoundDef;
        private SoundDef rockChargeSoundDef;
        private int nextRockSoundTick;
        private bool hitProxiesInitialized;
        private bool pendingVisualInit;
        private int spawnDestroyGraceUntilTick = -1;
        private const float DeathSinkTargetY = -70f;
        private const float DeathSinkSpeedPerTick = 0.023809524f;


        public override Vector3 DrawPos => exactPos;

        Thing IAttackTarget.Thing => this;

        public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

        public float TargetPriorityFactor => isDying ? 0f : 2f;

        internal Vector3 ExactPos
        {
            get => exactPos;
            set => exactPos = value;
        }

        internal Vector3 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        internal float HeadYawDeg
        {
            get => headYawDeg;
            set => headYawDeg = value;
        }

        internal float NoiseOffset => noiseOffset;

        internal float NoiseTime
        {
            get => noiseTime;
            set => noiseTime = value;
        }

        internal Vector3 MapCenter => mapCenter;

        internal float MapRadius => mapRadius;

        internal Transform HeadTransform => headTransform;
        internal bool IsDying => isDying;
        internal bool SurfacePressureEnabled
        {
            get => surfacePressureEnabled;
            set => surfacePressureEnabled = value;
        }

        internal string CurrentBehaviorId => stateMachine?.CurrentBehaviorId;
        internal SandWormBlackboard Blackboard => blackboard;

        public void SetInitialHeading(float yawDeg)
        {
            hasForcedInitialHeading = true;
            forcedInitialHeadingDeg = yawDeg;
        }

        // MarkSyndicateChallengeBoss 负责把直启挑战的大沙虫标记为挑战敌人，并写入挑战词条影响。
        public void MarkSyndicateChallengeBoss(float chargeCooldownFactor, float maxIncomingDamagePerHit, bool enableShockwaveAttack)
        {
            challengeSupportWorm = false;
            suppressQuestKillReward = true;
            challengeChargeCooldownFactor = Mathf.Clamp(chargeCooldownFactor, 0.1f, 1f);
            challengeMaxIncomingDamagePerHit = maxIncomingDamagePerHit > 0f ? maxIncomingDamagePerHit : -1f;
            challengeShockwaveEnabled = enableShockwaveAttack;
        }

        // SetChallengeModifiers 负责写入挑战词条给支援沙虫施加的临时强化。
        public void SetChallengeModifiers(bool allowHeadInstantKill, float hitPointFactor)
        {
            challengeSupportWorm = true;
            suppressQuestKillReward = true;
            challengeHeadInstantKillOverride = allowHeadInstantKill;
            challengeHitPointFactor = Mathf.Max(1f, hitPointFactor);
            challengeChargeCooldownFactor = 1f;
            challengeMaxIncomingDamagePerHit = -1f;
            challengeShockwaveEnabled = false;
            int boostedHitPoints = Mathf.RoundToInt(MaxHitPoints * challengeHitPointFactor);
            if (boostedHitPoints > HitPoints)
            {
                HitPoints = boostedHitPoints;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Faction mechFaction = Find.FactionManager?.FirstFactionOfDef(FactionDefOf.Mechanoid);
            if (mechFaction != null && Faction != mechFaction)
            {
                SetFaction(mechFaction);
            }

            // 读档的沙虫保留存档里的尺寸；新生成时使用当前类型的默认尺寸。
            if (!respawningAfterLoad)
            {
                sizeMultiplier = InitialSizeMultiplier;
            }
            else if (sizeMultiplier <= 0f)
            {
                sizeMultiplier = InitialSizeMultiplier;
            }
            RecomputeSizeDerived();

            if (!respawningAfterLoad)
            {
                HitPoints = MaxHitPoints;
                spawnDestroyGraceUntilTick = Find.TickManager.TicksGame + SpawnDestroyGraceTicks;
            }

            exactPos = base.DrawPos;
            velocity = Vector3.zero;
            headYawDeg = !respawningAfterLoad && hasForcedInitialHeading ? forcedInitialHeadingDeg : Rand.Range(0f, 360f);
            headPitchDeg = 0f;
            noiseOffset = Rand.Range(0f, 1000f);
            noiseTime = 0f;
            surfacePressureEnabled = true;
            isDying = false;
            deathSinkOffsetY = 0f;

            mapCenter = new Vector3(map.Size.x * 0.5f, 0f, map.Size.z * 0.5f);
            mapRadius = Mathf.Sqrt(map.Size.x * map.Size.x + map.Size.z * map.Size.z) * 0.5f;

            behaviorContext = new SandWormBehaviorContext(this);
            stateMachine = new SandWormStateMachine(behaviorContext);
            RegisterDefaultBehaviors();

            SetBehavior(SandWormBehaviorIds.Wander);
            hitProxiesInitialized = false;
            pendingVisualInit = true;
            nextRockSoundTick = Find.TickManager.TicksGame;

            if (!respawningAfterLoad)
            {
                EnsureVisualsInitialized();
            }

            map.GetComponent<SandWormPressureMapComponent>()?.RegisterWorm(this);
            map.attackTargetsCache.UpdateTarget(this);
            EnsureEntranceSong();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            stateMachine?.Shutdown();
            DespawnHitProxies(mode);
            if (Spawned && Map != null)
            {
                Map.GetComponent<SandWormPressureMapComponent>()?.UnregisterWorm(this);
                Map.attackTargetsCache.UpdateTarget(this);
            }
            DestroyInstance();
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!isDying && Spawned)
            {
                if (mode == DestroyMode.Vanish && Find.TickManager.TicksGame < spawnDestroyGraceUntilTick && HitPoints > 0)
                {
                    Log.Warning("[SandWorm] Ignored early non-lethal vanish destroy during spawn grace period.");
                    return;
                }

                BeginDeathSink();
                return;
            }

            stateMachine?.Shutdown();
            DespawnHitProxies(mode);
            if (Map != null)
            {
                Map.GetComponent<SandWormPressureMapComponent>()?.UnregisterWorm(this);
            }
            DestroyInstance();
            base.Destroy(mode);
        }

        protected override void Tick()
        {
            base.Tick();
            if (!Spawned)
            {
                return;
            }

            if (isDying)
            {
                ClampHitPointsToCurrentMax();
                TickDeathSink();
                return;
            }

            EnsureEntranceSong();
            ClampHitPointsToCurrentMax();

            if (pendingVisualInit || instance == null || headTransform == null)
            {
                EnsureVisualsInitialized();
            }

            if (instance == null || headTransform == null || stateMachine?.CurrentBehavior == null)
            {
                return;
            }

            stateMachine.Tick();
            TryPlayMovementRockSound();
            RecordHistory();
            ApplyFollow();
            EnsureHitProxiesInitialized();

            // 代理位置同步降频：每 N tick 一次。敌方 AI 锁定的反应有 N/60 秒延迟，几乎无感。
            int syncInterval = SandWormMod.Settings != null
                ? Mathf.Max(1, SandWormMod.Settings.hitProxySyncIntervalTicks)
                : 10;
            int nowTick = Find.TickManager.TicksGame;
            if (nowTick - lastHitProxySyncTick >= syncInterval)
            {
                UpdateHitProxyTransforms();
                lastHitProxySyncTick = nowTick;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
        }

        public override void DrawGUIOverlay()
        {
            if (isDying)
            {
                return;
            }

            base.DrawGUIOverlay();
        }

        public override void DrawExtraSelectionOverlays()
        {
            if (isDying)
            {
                return;
            }

            base.DrawExtraSelectionOverlays();
            DrawHitProxyDebugRects();
        }

        private void DrawHitProxyDebugRects()
        {
            if (SandWormMod.Settings == null || !SandWormMod.Settings.showDeveloperSettings || !SandWormMod.Settings.showHitProxyDebugRects)
            {
                return;
            }

            if (!Spawned || Map == null || Find.CurrentMap != Map || hitProxies == null)
            {
                return;
            }

            for (int i = 0; i < hitProxies.Count; i++)
            {
                SandWormHitProxyThing proxy = hitProxies[i];
                if (proxy != null && !proxy.Destroyed && proxy.Spawned)
                {
                    proxy.DrawDebugRect();
                }
            }
        }

        public override string GetInspectString()
        {
            if (isDying)
            {
                return string.Empty;
            }

            return base.GetInspectString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref sizeMultiplier, "sizeMultiplier", 1f);
            Scribe_Values.Look(ref challengeHeadInstantKillOverride, "challengeHeadInstantKillOverride", defaultValue: false);
            Scribe_Values.Look(ref challengeHitPointFactor, "challengeHitPointFactor", 1f);
            Scribe_Values.Look(ref challengeChargeCooldownFactor, "challengeChargeCooldownFactor", 1f);
            Scribe_Values.Look(ref challengeMaxIncomingDamagePerHit, "challengeMaxIncomingDamagePerHit", -1f);
            Scribe_Values.Look(ref challengeShockwaveEnabled, "challengeShockwaveEnabled", defaultValue: false);
            Scribe_Values.Look(ref challengeSupportWorm, "challengeSupportWorm", defaultValue: false);
            Scribe_Values.Look(ref suppressQuestKillReward, "suppressQuestKillReward", defaultValue: false);
            Scribe_Collections.Look(ref hitProxies, "hitProxies", LookMode.Reference);
            Scribe_Values.Look(ref hitProxiesInitialized, "hitProxiesInitialized", false);
            Scribe_Values.Look(ref pendingVisualInit, "pendingVisualInit", false);
            Scribe_Values.Look(ref isDying, "isDying", false);
            Scribe_Values.Look(ref deathSinkOffsetY, "deathSinkOffsetY", 0f);
            Scribe_Values.Look(ref spawnDestroyGraceUntilTick, "spawnDestroyGraceUntilTick", -1);
            Scribe_Values.Look(ref hasForcedInitialHeading, "hasForcedInitialHeading", false);
            Scribe_Values.Look(ref forcedInitialHeadingDeg, "forcedInitialHeadingDeg", 0f);
            Scribe_Deep.Look(ref blackboard, "blackboard");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (sizeMultiplier <= 0f)
                {
                    sizeMultiplier = 1f;
                }
                RecomputeSizeDerived();
            }
        }

        private void RecomputeSizeDerived()
        {
            historyLength = Mathf.Max(BoneCount * 8, Mathf.RoundToInt(BaseHistoryLength * sizeMultiplier));
            hitProxyWidthScaled = BaseHitProxyWidth * sizeMultiplier;
            hitProxyLengthScaled = BaseHitProxyLength * sizeMultiplier;
        }

        public override void PostMapInit()
        {
            base.PostMapInit();
            if (Map != null)
            {
                Map.attackTargetsCache.UpdateTarget(this);
            }


        }

        private void EnsureVisualsInitialized()
        {
            if (!Spawned || Map == null)
            {
                return;
            }

            if (instance != null && headTransform != null && bones != null && rotHistory != null && posHistory != null)
            {
                pendingVisualInit = false;
                return;
            }

            CreateInstance();
            if (instance == null || headTransform == null)
            {
                pendingVisualInit = true;
                return;
            }

            InitBones();
            InitHistory();
            pendingVisualInit = false;
        }

        // RegisterDefaultBehaviors 负责登记当前沙虫类型默认可用的行为状态。
        protected virtual void RegisterDefaultBehaviors()
        {
            stateMachine.Register(new SandWormWanderBehavior());
            stateMachine.Register(new SandWormChargeBehavior());
            stateMachine.Register(new SandWormShockwaveBehavior());
        }

        internal void SetBehavior(string behaviorId)
        {
            stateMachine?.SetBehavior(behaviorId);
        }

        private void CreateInstance()
        {
            DestroyInstance();

            if (!abDatabase.prefabDataBase.TryGetValue(PrefabKey, out GameObject prefab) || prefab == null)
            {
                Log.Error("[SandWorm] Missing prefab: " + PrefabKey);
                return;
            }

            wrapperObject = new GameObject("SandWormWrapper");
            wrapperObject.transform.position = exactPos + new Vector3(0f, HeightOffset, 0f);
            wrapperObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            wrapperObject.transform.localScale = Vector3.one * VisualScale;
            SandWormVisualStateManager.Register(wrapperObject, Map);

            instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, wrapperObject.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            ConfigureInstanceRenderers(instance);

            headTransform = instance.transform;
            SetHeadRotation(headYawDeg);
            bankDeg = 0f;
        }

        private void DestroyInstance()
        {
            if (wrapperObject != null)
            {
                SandWormVisualStateManager.Unregister(wrapperObject);
                Object.Destroy(wrapperObject);
                wrapperObject = null;
            }

            instance = null;
            headTransform = null;
            bones = null;
            boneOffsets = null;
            boneDistances = null;
            rotHistory = null;
            posHistory = null;
            boneLookupCache.Clear();
        }

        // ConfigureInstanceRenderers 负责整理沙虫实例的渲染器，并允许派生类型覆写材质。
        protected virtual void ConfigureInstanceRenderers(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                ApplyInstanceMaterial(renderers[i]);
            }

            ParticleSystemRenderer[] particleRenderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                particleRenderers[i].sortingFudge = WormParticleSortingFudge;
            }
        }

        // ApplyInstanceMaterial 负责让不同沙虫类型可以在复用同一预制体时替换自己的运行时材质。
        protected virtual void ApplyInstanceMaterial(Renderer renderer)
        {
        }

        private void InitBones()
        {
            if (headTransform == null)
            {
                return;
            }

            bones = new Transform[BoneCount];
            boneOffsets = new Quaternion[BoneCount];
            boneDistances = new float[BoneCount];

            // 一次性建立 Transform 字典，避免 FindDeep 每次都全树扫描
            BuildBoneLookupCache();

            for (int i = 0; i < BoneCount; i++)
            {
                string name = "Worm_" + (i + 1).ToString("D2");
                boneLookupCache.TryGetValue(name, out Transform bone);
                bones[i] = bone;
                if (bones[i] != null)
                {
                    boneOffsets[i] = Quaternion.Inverse(headTransform.rotation) * bones[i].rotation;
                }
                else
                {
                    Log.Warning("[SandWorm] Missing bone: " + name);
                    boneOffsets[i] = Quaternion.identity;
                }
            }

            float cumulativeDistance = 0f;
            float fallbackSegment = 1.8f * sizeMultiplier;
            for (int i = 0; i < BoneCount; i++)
            {
                if (bones[i] == null)
                {
                    boneDistances[i] = i > 0 ? boneDistances[i - 1] + fallbackSegment : fallbackSegment;
                    continue;
                }

                Vector3 previousPosition;
                if (i == 0)
                {
                    previousPosition = headTransform.position;
                }
                else if (bones[i - 1] != null)
                {
                    previousPosition = bones[i - 1].position;
                }
                else
                {
                    previousPosition = headTransform.position;
                }

                float segmentDistance = Vector3.Distance(previousPosition, bones[i].position);
                cumulativeDistance += segmentDistance;
                boneDistances[i] = cumulativeDistance;
            }
        }

        private void BuildBoneLookupCache()
        {
            boneLookupCache.Clear();
            Transform[] all = headTransform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform t = all[i];
                if (t != null && !boneLookupCache.ContainsKey(t.name))
                {
                    boneLookupCache[t.name] = t;
                }
            }
        }

        private void InitHistory()
        {
            rotHistory = new Quaternion[historyLength];
            posHistory = new Vector3[historyLength];

            Quaternion initRot = headTransform != null ? headTransform.rotation : Quaternion.identity;
            Vector3 initPos = wrapperObject != null ? wrapperObject.transform.position : headTransform != null ? headTransform.position : Vector3.zero;

            Vector3 forward = GetWorldForward();
            if (forward.sqrMagnitude < 0.001f)
            {
                float yawRad = headYawDeg * Mathf.Deg2Rad;
                forward = new Vector3(-Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
            }
            else
            {
                forward.Normalize();
            }

            float totalLength = boneDistances != null && boneDistances.Length > 0
                ? boneDistances[boneDistances.Length - 1]
                : BoneCount * 1.8f * sizeMultiplier;
            float stepDist = totalLength / Mathf.Max(1, historyLength - 1);

            for (int i = 0; i < historyLength; i++)
            {
                float ticksBehind = historyLength - 1 - i;
                posHistory[i] = initPos - forward * (ticksBehind * stepDist);
                rotHistory[i] = initRot;
            }

            historyHead = historyLength - 1;
        }

        private void RecordHistory()
        {
            historyHead = (historyHead + 1) % historyLength;
            Quaternion baseWrapper = Quaternion.Euler(90f, 0f, 0f);
            rotHistory[historyHead] = baseWrapper * headTransform.localRotation;
            posHistory[historyHead] = headTransform.position;
        }

        private void ApplyFollow()
        {
            if (bones == null || rotHistory == null || posHistory == null || boneDistances == null)
            {
                return;
            }

            int boneIdx = 0;
            float accumDist = 0f;
            int cursor = historyHead;
            int maxSteps = historyLength - 1;

            for (int step = 0; step < maxSteps && boneIdx < BoneCount; step++)
            {
                int prev = (cursor - 1 + historyLength) % historyLength;
                float segDist = Vector3.Distance(posHistory[cursor], posHistory[prev]);
                float nextAccum = accumDist + segDist;

                while (boneIdx < BoneCount && boneDistances[boneIdx] <= nextAccum)
                {
                    if (bones[boneIdx] == null)
                    {
                        boneIdx++;
                        continue;
                    }

                    float targetDist = boneDistances[boneIdx];
                    float t = segDist > 0.0001f ? (targetDist - accumDist) / segDist : 0f;

                    Vector3 targetPos = Vector3.Lerp(posHistory[cursor], posHistory[prev], t);
                    Quaternion targetRot = Quaternion.Slerp(rotHistory[cursor], rotHistory[prev], t)
                                           * boneOffsets[boneIdx];

                    // 直接写入：历史数组本身就是"真实过去位置"，再做 Lerp 只会让身体
                    // 永远追不上头部造成视觉拖拉（这是原版"不顺畅"的主要原因）。
                    bones[boneIdx].position = targetPos;
                    bones[boneIdx].rotation = targetRot;

                    boneIdx++;
                }

                accumDist = nextAccum;
                cursor = prev;
            }

            if (boneIdx < BoneCount)
            {
                Vector3 tailPos = posHistory[cursor];
                Quaternion tailRot = rotHistory[cursor];
                for (int i = boneIdx; i < BoneCount; i++)
                {
                    if (bones[i] == null) continue;
                    Quaternion rot = tailRot * boneOffsets[i];
                    bones[i].position = tailPos;
                    bones[i].rotation = rot;
                }
            }
        }

        internal void SetHeadRotation(float yawDeg)
        {
            if (headTransform == null)
            {
                return;
            }

            headYawDeg = yawDeg;
            headTransform.localRotation = Quaternion.Euler(headPitchDeg, 0f, yawDeg);
        }

        internal void SetHeadPitchDeg(float pitchDeg)
        {
            headPitchDeg = pitchDeg;
            if (headTransform != null)
            {
                headTransform.localRotation = Quaternion.Euler(headPitchDeg, 0f, headYawDeg);
            }
        }

        internal void SetBankDeg(float deg)
        {
            bankDeg = deg;
        }

        internal void SetHeadLocalRotation(Quaternion localRot)
        {
            if (headTransform == null) return;

            headTransform.localRotation = localRot;

            Vector3 localUp = localRot * Vector3.up;
            headPitchDeg = -Mathf.Asin(Mathf.Clamp(localUp.z, -1f, 1f)) * Mathf.Rad2Deg;
        }

        internal void UpdateWrapperDirect(Vector3 worldPos, float overrideBankDeg)
        {
            if (wrapperObject == null) return;

            bankDeg = overrideBankDeg;

            wrapperObject.transform.position = worldPos + new Vector3(0f, HeightOffset, 0f);
            wrapperObject.transform.rotation = Quaternion.Euler(90f, 0f, bankDeg);
        }

        // [FIX] 核心修复点：修复了因为 Yaw 计算体系导致的回旋力方向反转的问题
        internal float GetReturnToCenterTurn(float thresholdFactor, float returnMargin, float returnForce)
        {
            Vector3 toCenter = mapCenter - exactPos;
            toCenter.y = 0f;

            float distanceFromCenter = toCenter.magnitude;
            float threshold = mapRadius * thresholdFactor;
            if (distanceFromCenter < threshold)
            {
                return 0f;
            }

            float excess = distanceFromCenter - threshold;
            float maxExcess = mapRadius * (1f - thresholdFactor) + returnMargin;
            float strength = Mathf.Clamp01(excess / maxExcess) * returnForce;

            Vector3 forward = GetWorldForward();
            if (forward.sqrMagnitude < 0.001f)
            {
                return 0f;
            }

            forward.Normalize();

            float signedAngle = GetSignedAngleTo(mapCenter);
            float absSigned = signedAngle < 0f ? -signedAngle : signedAngle;

            if (absSigned < 1f)
            {
                return 0f;
            }

            // 加上负号，修复转向方向，让沙虫向着中心正确转弯
            return Mathf.Clamp(-signedAngle, -strength, strength);
        }

        internal float GetBiasTowardPlayerTurn(float biasForce, float maxBiasTurn)
        {
            if (Map == null)
            {
                return 0f;
            }

            Vector3? targetPosition = GetCachedBiasTargetPosition();
            if (!targetPosition.HasValue)
            {
                return 0f;
            }

            float angleToTarget = GetSignedAngleTo(targetPosition.Value);
            return Mathf.Clamp(angleToTarget * biasForce, -maxBiasTurn, maxBiasTurn);
        }

        // GetCachedBiasTargetPosition 缓存沙虫游荡时的偏向目标，避免每 tick 扫描全图 Pawn 和殖民地建筑。
        private Vector3? GetCachedBiasTargetPosition()
        {
            int nowTick = Find.TickManager.TicksGame;
            if (cachedBiasTargetPosition.HasValue && nowTick - lastBiasTargetRefreshTick < TargetSearchCacheIntervalTicks)
            {
                return cachedBiasTargetPosition;
            }

            lastBiasTargetRefreshTick = nowTick;
            cachedBiasTargetPosition = FindBiasTargetPosition();
            return cachedBiasTargetPosition;
        }

        // FindBiasTargetPosition 在主线程只读扫描玩家单位和建筑，选择最近的游荡偏向目标。
        private Vector3? FindBiasTargetPosition()
        {
            Pawn closestPawn = null;
            float closestDistSq = float.MaxValue;

            IReadOnlyList<Pawn> pawns = Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned)
                {
                    continue;
                }

                if (pawn.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                float distSq = (pawn.DrawPos - exactPos).sqrMagnitude;
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestPawn = pawn;
                }
            }

            if (closestPawn == null)
            {
                List<Building> buildings = Map.listerBuildings.allBuildingsColonist;
                if (buildings.Count > 0)
                {
                    float closestBldgDistSq = float.MaxValue;
                    Vector3 closestBldgPos = mapCenter;
                    for (int i = 0; i < buildings.Count; i++)
                    {
                        Building b = buildings[i];
                        if (b == null || b.Destroyed || !b.Spawned) continue;
                        float dSq = (b.DrawPos - exactPos).sqrMagnitude;
                        if (dSq < closestBldgDistSq)
                        {
                            closestBldgDistSq = dSq;
                            closestBldgPos = b.DrawPos;
                        }
                    }

                    return closestBldgPos;
                }

                return null;
            }

            return closestPawn.DrawPos;
        }

        internal float GetSignedAngleTo(Vector3 worldTarget)
        {
            Vector3 toTarget = worldTarget - exactPos;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            Vector3 forward = GetWorldForward();
            if (forward.sqrMagnitude < 0.0001f)
            {
                float yawRad = headYawDeg * Mathf.Deg2Rad;
                forward = new Vector3(-Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
            }

            forward.Normalize();
            toTarget.Normalize();

            return -Vector3.SignedAngle(forward, toTarget, Vector3.up);
        }

        internal Thing FindPreferredChargeTarget(float maxDistance)
        {
            if (Map == null)
            {
                return null;
            }

            int nowTick = Find.TickManager.TicksGame;
            if (cachedChargeTarget != null
                && nowTick - lastChargeTargetRefreshTick < TargetSearchCacheIntervalTicks
                && IsValidChargeTarget(cachedChargeTarget, maxDistance))
            {
                return cachedChargeTarget;
            }

            lastChargeTargetRefreshTick = nowTick;
            cachedChargeTarget = FindPreferredChargeTargetUncached(maxDistance);
            return cachedChargeTarget;
        }

        // FindPreferredChargeTargetUncached 在主线程扫描可冲锋目标，结果由 FindPreferredChargeTarget 降频缓存。
        private Thing FindPreferredChargeTargetUncached(float maxDistance)
        {
            Pawn bestPlayerPawn = null;
            float bestPlayerPawnDistanceSq = float.MaxValue;
            Pawn bestOtherPawn = null;
            float bestOtherPawnDistanceSq = float.MaxValue;
            IReadOnlyList<Pawn> pawns = Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!IsValidChargeTarget(pawn, maxDistance))
                {
                    continue;
                }

                float distanceSq = (pawn.DrawPos - exactPos).sqrMagnitude;
                if (pawn.Faction == Faction.OfPlayer)
                {
                    if (distanceSq >= bestPlayerPawnDistanceSq)
                    {
                        continue;
                    }

                    bestPlayerPawn = pawn;
                    bestPlayerPawnDistanceSq = distanceSq;
                }
                else
                {
                    if (distanceSq >= bestOtherPawnDistanceSq)
                    {
                        continue;
                    }

                    bestOtherPawn = pawn;
                    bestOtherPawnDistanceSq = distanceSq;
                }
            }

            if (bestPlayerPawn != null)
            {
                return bestPlayerPawn;
            }

            if (bestOtherPawn != null)
            {
                return bestOtherPawn;
            }

            Building bestTurret = null;
            float bestTurretDistanceSq = float.MaxValue;
            List<Building> buildings = Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building building = buildings[i];
                if (!IsValidChargeTarget(building, maxDistance))
                {
                    continue;
                }

                if (building.def?.building == null || !building.def.building.IsTurret)
                {
                    continue;
                }

                float distanceSq = (building.DrawPos - exactPos).sqrMagnitude;
                if (distanceSq >= bestTurretDistanceSq)
                {
                    continue;
                }

                bestTurret = building;
                bestTurretDistanceSq = distanceSq;
            }

            return bestTurret;
        }

        internal bool IsValidChargeTarget(Thing target, float maxDistance)
        {
            if (target == null || target.Destroyed || !target.Spawned || target.Map != Map)
            {
                return false;
            }

            if ((target.DrawPos - exactPos).sqrMagnitude > maxDistance * maxDistance)
            {
                return false;
            }

            if (target == this || target is SandWormHitProxyThing)
            {
                return false;
            }

            if (target is Pawn pawn)
            {
                return !pawn.Dead
                    && !pawn.Downed;
            }

            Building buildingTarget = target as Building;
            return buildingTarget != null
                && buildingTarget.def?.building != null
                && buildingTarget.def.building.IsTurret;
        }

        internal void ApplyMovementAndVisuals(
            float moveSpeed,
            float maxTurnPerTick,
            float turnBlend,
            float totalTurn,
            float heightOffset,
            float bankMaxDeg,
            float viewTiltXMaxDeg,
            float bankLerp,
            int positionSyncIntervalTicks)
        {
            if (headPitchDeg != 0f)
            {
                headPitchDeg = 0f;
                if (headTransform != null)
                {
                    headTransform.localRotation = Quaternion.Euler(0f, 0f, headYawDeg);
                }
            }

            Vector3 worldForward = GetWorldForward();
            if (worldForward.sqrMagnitude < 0.0001f)
            {
                float yawRad = headYawDeg * Mathf.Deg2Rad;
                worldForward = new Vector3(-Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
            }
            else
            {
                worldForward.Normalize();
            }

            if (velocity.sqrMagnitude < 0.000001f)
            {
                velocity = worldForward * moveSpeed;
            }
            else
            {
                float maxTurnRad = maxTurnPerTick * Mathf.Deg2Rad;
                Vector3 targetVelocity = worldForward * moveSpeed;
                velocity = Vector3.RotateTowards(velocity, targetVelocity, maxTurnRad, moveSpeed);
                velocity = Vector3.Lerp(velocity, targetVelocity, turnBlend);
            }

            exactPos += velocity;

            if (wrapperObject != null)
            {
                wrapperObject.transform.position = exactPos + new Vector3(0f, heightOffset, 0f);

                float turnRatio = totalTurn / maxTurnPerTick;
                float deadZone = 0.2f;
                float filteredTurn = Mathf.Abs(turnRatio) < deadZone
                    ? 0f
                    : Mathf.Sign(turnRatio) * (Mathf.Abs(turnRatio) - deadZone) / (1f - deadZone);

                float targetBank = filteredTurn * bankMaxDeg;
                float targetTilt = viewTiltXMaxDeg * Mathf.Abs(filteredTurn);

                bankDeg = Mathf.Lerp(bankDeg, targetBank, bankLerp);
                float currentTilt = wrapperObject.transform.rotation.eulerAngles.x - 90f;
                float smoothTilt = Mathf.Lerp(currentTilt, targetTilt, bankLerp);

                wrapperObject.transform.rotation = Quaternion.Euler(90f + smoothTilt, 0f, bankDeg);
            }

            if (Find.TickManager.TicksGame % positionSyncIntervalTicks == 0)
            {
                IntVec3 newCell = exactPos.ToIntVec3();
                if (newCell.InBounds(Map))
                {
                    Position = newCell;
                }
            }
        }

        private void EnsureEntranceSong()
        {
            if (Find.MusicManagerPlay == null)
            {
                return;
            }

            if (entranceSongDef == null)
            {
                entranceSongDef = DefDatabase<SongDef>.GetNamedSilentFail(EntranceSongDefName);
            }
            if (entranceSongDef == null)
            {
                return;
            }

            MusicManagerPlay manager = Find.MusicManagerPlay;
            if (!manager.IsPlaying || manager.CurrentSong != entranceSongDef)
            {
                manager.ForcePlaySong(entranceSongDef, ignorePrefsVolume: false);
            }
        }

        private void ClampHitPointsToCurrentMax()
        {
            int maxHitPoints = Mathf.RoundToInt(MaxHitPoints * Mathf.Max(1f, challengeHitPointFactor));
            if (HitPoints > maxHitPoints)
            {
                HitPoints = maxHitPoints;
            }
        }

        private void TryPlayMovementRockSound()
        {
            if (Map == null || Find.TickManager.TicksGame < nextRockSoundTick)
            {
                return;
            }

            bool isCharging = CurrentBehaviorId == SandWormBehaviorIds.Charge;
            SoundDef soundDef = isCharging ? GetRockChargeSoundDef() : GetRockMoveSoundDef();
            if (soundDef == null)
            {
                nextRockSoundTick = Find.TickManager.TicksGame + 120;
                return;
            }

            soundDef.PlayOneShot(new TargetInfo(Position, Map));
            nextRockSoundTick = Find.TickManager.TicksGame + (isCharging ? ChargeRockSoundIntervalTicks : WanderRockSoundIntervalTicks);
        }

        private SoundDef GetRockMoveSoundDef()
        {
            if (rockMoveSoundDef == null)
            {
                rockMoveSoundDef = DefDatabase<SoundDef>.GetNamedSilentFail(RockMoveSoundDefName);
            }
            return rockMoveSoundDef;
        }

        private SoundDef GetRockChargeSoundDef()
        {
            if (rockChargeSoundDef == null)
            {
                rockChargeSoundDef = DefDatabase<SoundDef>.GetNamedSilentFail(RockChargeSoundDefName);
            }
            return rockChargeSoundDef;
        }

        internal bool IsOutsideMapBounds(float margin = 0f)
        {
            if (Map == null)
            {
                return false;
            }

            return exactPos.x < margin
                || exactPos.z < margin
                || exactPos.x > Map.Size.x - 1f - margin
                || exactPos.z > Map.Size.z - 1f - margin;
        }

        internal Vector3 GetRecoveryTarget(float inset)
        {
            if (Map == null)
            {
                return mapCenter;
            }

            float minX = inset;
            float maxX = Map.Size.x - 1f - inset;
            float minZ = inset;
            float maxZ = Map.Size.z - 1f - inset;

            float targetX = Mathf.Clamp(exactPos.x, minX, maxX);
            float targetZ = Mathf.Clamp(exactPos.z, minZ, maxZ);
            Vector3 recoveryTarget = new Vector3(targetX, exactPos.y, targetZ);
            if ((recoveryTarget - exactPos).sqrMagnitude < 0.01f)
            {
                recoveryTarget = new Vector3(mapCenter.x, exactPos.y, mapCenter.z);
            }

            return recoveryTarget;
        }

        private Vector3 GetWorldForward()
        {
            if (headTransform == null)
            {
                return Vector3.zero;
            }

            Vector3 worldForward = headTransform.up;
            worldForward.y = 0f;
            return worldForward;
        }

        internal void Notify_HitProxyDamaged(SandWormHitProxyThing proxy, DamageInfo dinfo)
        {
            if (Destroyed || isDying)
            {
                return;
            }

            blackboard.LastDamageTick = Find.TickManager.TicksGame;
            TakeDamage(dinfo);
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            if (isDying)
            {
                absorbed = true;
                return;
            }

            if (dinfo.Category == DamageInfo.SourceCategory.Collapse)
            {
                absorbed = true;
                return;
            }

            dinfo.SetAmount(dinfo.Amount * GetIncomingDamageMultiplier());
            ApplyChallengeDamageCap(ref dinfo);
            base.PreApplyDamage(ref dinfo, out absorbed);
        }

        // ApplyChallengeDamageCap 负责对辛迪加挑战大沙虫应用单次承伤上限。
        private void ApplyChallengeDamageCap(ref DamageInfo dinfo)
        {
            if (challengeMaxIncomingDamagePerHit > 0f && dinfo.Amount > challengeMaxIncomingDamagePerHit)
            {
                dinfo.SetAmount(challengeMaxIncomingDamagePerHit);
            }
        }

        internal int SegmentCount => bones?.Length ?? 0;

        internal bool IsSegmentSystemReady => Spawned && Map != null && bones != null && headTransform != null;

        internal bool TryGetSegmentData(int boneIndex, out Vector3 center, out Quaternion rotation, out Vector2 size)
        {
            center = Vector3.zero;
            rotation = Quaternion.identity;
            size = Vector2.zero;

            if (!IsSegmentSystemReady || boneIndex < 0 || boneIndex >= bones.Length)
            {
                return false;
            }

            Transform bone = bones[boneIndex];
            if (bone == null)
            {
                return false;
            }

            center = bone.position;
            rotation = bone.rotation;
            size = GetHitProxySizeForBone(boneIndex);
            return true;
        }

        // CollectShockwaveSourceCells 负责从沙虫全身骨骼采样冲击波源点，让蓄力、飞石和伤害不再只来自头部。
        internal void CollectShockwaveSourceCells(List<IntVec3> outCells, int maxCount)
        {
            if (outCells == null)
            {
                return;
            }

            maxCount = Mathf.Max(1, maxCount);
            if (!IsSegmentSystemReady || SegmentCount <= 0)
            {
                AddShockwaveSourceCell(outCells, ExactPos.ToIntVec3(), maxCount);
                return;
            }

            int segmentCount = SegmentCount;
            int sampleCount = Mathf.Clamp(maxCount, 1, segmentCount);
            for (int i = 0; i < sampleCount; i++)
            {
                float t = sampleCount == 1 ? 0f : i / (float)(sampleCount - 1);
                int boneIndex = Mathf.Clamp(Mathf.RoundToInt(t * (segmentCount - 1)), 0, segmentCount - 1);
                if (!TryGetSegmentData(boneIndex, out Vector3 center, out Quaternion _, out Vector2 _))
                {
                    continue;
                }

                AddShockwaveSourceCell(outCells, center.ToIntVec3(), maxCount);
            }

            if (outCells.Count == 0)
            {
                AddShockwaveSourceCell(outCells, ExactPos.ToIntVec3(), maxCount);
            }
        }

        // AddShockwaveSourceCell 负责过滤重复和越界源点，避免同一身体段重复生成大量特效。
        private void AddShockwaveSourceCell(List<IntVec3> outCells, IntVec3 cell, int maxCount)
        {
            if (Map != null)
            {
                cell.x = Mathf.Clamp(cell.x, 0, Map.Size.x - 1);
                cell.z = Mathf.Clamp(cell.z, 0, Map.Size.z - 1);
            }

            if (!cell.IsValid || outCells.Count >= maxCount || outCells.Contains(cell))
            {
                return;
            }

            outCells.Add(cell);
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            return Destroyed || !Spawned || isDying;
        }

        internal bool TryGetBestHitProxyForShot(Vector3 shotOrigin, IntVec3 targetCell, out SandWormHitProxyThing bestProxy)
        {
            bestProxy = null;
            if (!Spawned || Map == null || hitProxies == null)
            {
                return false;
            }

            float bestScore = float.MaxValue;
            Vector3 targetPoint = targetCell.ToVector3Shifted();
            for (int i = 0; i < hitProxies.Count; i++)
            {
                SandWormHitProxyThing proxy = hitProxies[i];
                if (proxy == null || proxy.Destroyed || !proxy.Spawned || proxy.Map != Map)
                {
                    continue;
                }

                Vector3 proxyPoint = proxy.DrawPos;
                float targetDistance = (proxyPoint - targetPoint).sqrMagnitude;
                float originDistance = (proxyPoint - shotOrigin).sqrMagnitude * 0.015f;
                float score = targetDistance + originDistance;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestProxy = proxy;
                }
            }

            return bestProxy != null;
        }

        private Vector2 GetHitProxySizeForBone(int boneIndex)
        {
            float t = BoneCount <= 1 ? 0f : boneIndex / (float)(BoneCount - 1);
            float width = Mathf.Lerp(hitProxyWidthScaled, hitProxyWidthScaled * 0.55f, t);
            float headShorten = 15f * sizeMultiplier;
            float baseLength = boneIndex == 0 ? hitProxyLengthScaled - headShorten : hitProxyLengthScaled;
            float length = Mathf.Lerp(baseLength, baseLength * 0.6f, t);
            return new Vector2(width, length);
        }

        private void EnsureHitProxiesInitialized()
        {
            if (hitProxiesInitialized || !IsSegmentSystemReady)
            {
                return;
            }

            EnsureHitProxies();
            hitProxiesInitialized = true;
        }

        private void EnsureHitProxies()
        {
            while (hitProxies.Count < BoneCount)
                hitProxies.Add(null);

            ThingDef proxyDef = DefDatabase<ThingDef>.GetNamedSilentFail(HitProxyDefName);
            if (proxyDef == null)
            {
                Log.Error("[SandWorm] Missing ThingDef: " + HitProxyDefName);
                return;
            }

            for (int i = 0; i < BoneCount; i++)
            {
                if (!TryGetSegmentData(i, out Vector3 center, out Quaternion rotation, out Vector2 size))
                    continue;

                SandWormHitProxyThing proxy = hitProxies[i];
                if (proxy != null && !proxy.Destroyed)
                {
                    if (!proxy.Spawned)
                    {
                        proxy.AttachToOwner(this, i);
                        proxy.UpdateTransform(center, rotation, size);
                        TrySpawnHitProxy(proxy, center);
                    }
                    continue;
                }

                SandWormHitProxyThing newProxy = ThingMaker.MakeThing(proxyDef) as SandWormHitProxyThing;
                if (newProxy == null)
                {
                    Log.Error("[SandWorm] " + HitProxyDefName + " did not produce SandWormHitProxyThing.");
                    continue;
                }

                newProxy.AttachToOwner(this, i);
                newProxy.UpdateTransform(center, rotation, size);
                if (Faction != null && newProxy.Faction != Faction)
                {
                    newProxy.SetFaction(Faction);
                }
                if (TrySpawnHitProxy(newProxy, center))
                {
                    hitProxies[i] = newProxy;
                }
                else if (!newProxy.Destroyed)
                {
                    newProxy.Destroy(DestroyMode.Vanish);
                }
            }
        }

        private void UpdateHitProxyTransforms()
        {
            if (!hitProxiesInitialized || !IsSegmentSystemReady)
            {
                return;
            }

            for (int i = 0; i < hitProxies.Count; i++)
            {
                SandWormHitProxyThing proxy = hitProxies[i];
                if (proxy == null || proxy.Destroyed)
                {
                    CreateMissingHitProxy(i);
                    continue;
                }

                if (!TryGetSegmentData(i, out Vector3 center, out Quaternion rotation, out Vector2 size))
                {
                    continue;
                }

                if (!proxy.Spawned)
                {
                    proxy.AttachToOwner(this, i);
                    proxy.UpdateTransform(center, rotation, size);
                    TrySpawnHitProxy(proxy, center);
                    continue;
                }

                proxy.UpdateTransform(center, rotation, size);
            }
        }

        private void CreateMissingHitProxy(int boneIndex)
        {
            ThingDef proxyDef = DefDatabase<ThingDef>.GetNamedSilentFail(HitProxyDefName);
            if (proxyDef == null || !TryGetSegmentData(boneIndex, out Vector3 center, out Quaternion rotation, out Vector2 size))
            {
                return;
            }

            SandWormHitProxyThing newProxy = ThingMaker.MakeThing(proxyDef) as SandWormHitProxyThing;
            if (newProxy == null)
            {
                return;
            }

            newProxy.AttachToOwner(this, boneIndex);
            newProxy.UpdateTransform(center, rotation, size);
            if (Faction != null && newProxy.Faction != Faction)
            {
                newProxy.SetFaction(Faction);
            }

            if (TrySpawnHitProxy(newProxy, center))
            {
                hitProxies[boneIndex] = newProxy;
            }
            else if (!newProxy.Destroyed)
            {
                newProxy.Destroy(DestroyMode.Vanish);
            }
        }

        private void DespawnHitProxies(DestroyMode mode)
        {
            for (int i = 0; i < hitProxies.Count; i++)
            {
                SandWormHitProxyThing proxy = hitProxies[i];
                if (proxy == null)
                {
                    continue;
                }

                if (!proxy.Destroyed)
                {
                    if (proxy.Spawned)
                    {
                        proxy.DeSpawn(mode);
                    }
                    else
                    {
                        proxy.Destroy(mode);
                    }
                }

                hitProxies[i] = null;
            }

            hitProxiesInitialized = false;
        }

        private void DestroyHitProxiesImmediately()
        {
            for (int i = 0; i < hitProxies.Count; i++)
            {
                SandWormHitProxyThing proxy = hitProxies[i];
                if (proxy == null)
                {
                    continue;
                }

                if (!proxy.Destroyed)
                {
                    proxy.Destroy(DestroyMode.Vanish);
                }

                hitProxies[i] = null;
            }

            hitProxiesInitialized = false;
        }

        private bool TrySpawnHitProxy(SandWormHitProxyThing proxy, Vector3 center)
        {
            if (proxy == null || proxy.Destroyed || proxy.Spawned || Map == null)
            {
                return false;
            }

            IntVec3 cell = ClampCellToMap(center.ToIntVec3());
            GenSpawn.Spawn(proxy, cell, Map, WipeMode.Vanish);
            return proxy.Spawned;
        }

        private IntVec3 ClampCellToMap(IntVec3 cell)
        {
            if (Map == null)
            {
                return cell;
            }

            ThingDef proxyDef = DefDatabase<ThingDef>.GetNamedSilentFail(HitProxyDefName);
            IntVec2 size = proxyDef?.size ?? new IntVec2(1, 1);
            int halfWidth = size.x / 2;
            int halfHeight = size.z / 2;

            int minX = halfWidth;
            int maxX = Mathf.Max(minX, Map.Size.x - (size.x - halfWidth));
            int minZ = halfHeight;
            int maxZ = Mathf.Max(minZ, Map.Size.z - (size.z - halfHeight));

            int x = Mathf.Clamp(cell.x, minX, maxX);
            int z = Mathf.Clamp(cell.z, minZ, maxZ);
            return new IntVec3(x, 0, z);
        }

        // BeginDeathSink 负责在沙虫生命结束时关闭行为、销毁命中代理并启动下沉死亡演出。
        private void BeginDeathSink()
        {
            if (isDying)
            {
                return;
            }

            isDying = true;
            deathSinkOffsetY = 0f;
            surfacePressureEnabled = false;
            velocity = Vector3.zero;
            stateMachine?.Shutdown();
            DestroyHitProxiesImmediately();
            if (Spawned && Map != null)
            {
                if (suppressQuestKillReward && !challengeSupportWorm)
                {
                    Current.Game?.GetComponent<SandWormSyndicateChallengeState>()?.NotifyChallengeBossKilled(this);
                }

                if (!suppressQuestKillReward)
                {
                    SandWormLeviathanSite site = Map.Parent as SandWormLeviathanSite;
                    site?.NotifyLeviathanKilled();
                    if (site == null)
                    {
                        SandWormQuestUtility.ClearAbnormalSandstormAfterKill(Map);
                        SandWormQuestUtility.StopLeviathanEntranceSong();
                    }
                    SandWormQuestUtility.NotifyLeviathanKilled(site ?? Map.Parent);
                }
                Map.attackTargetsCache.UpdateTarget(this);
            }
        }

        // TickDeathSink 负责推进沙虫下沉死亡演出，并在完全沉没后销毁 Thing。
        private void TickDeathSink()
        {
            if (wrapperObject == null)
            {
                base.Destroy(DestroyMode.Vanish);
                return;
            }

            deathSinkOffsetY -= DeathSinkSpeedPerTick;
            Vector3 basePos = exactPos + new Vector3(0f, HeightOffset, 0f);
            wrapperObject.transform.position = new Vector3(basePos.x, basePos.y + deathSinkOffsetY, basePos.z);

            if (basePos.y + deathSinkOffsetY <= DeathSinkTargetY)
            {
                base.Destroy(DestroyMode.Vanish);
            }
        }

        private void DrawPathOverlay()
        {
            if (!Spawned || Map == null || posHistory == null || stateMachine?.CurrentBehaviorDef == null)
            {
                return;
            }

            if (!stateMachine.CurrentBehaviorDef.DrawPath)
            {
                return;
            }

            Color color = GetPathColor();
            Vector3? previous = null;
            for (int step = 0; step < historyLength; step += PathDrawStep)
            {
                int index = (historyHead - step + historyLength) % historyLength;
                Vector3 point = posHistory[index] + Vector3.up * PathDrawAltitude;
                if (previous.HasValue)
                {
                    GenDraw.DrawLineBetween(previous.Value, point, SolidColorMaterials.SimpleSolidColorMaterial(color), 0.12f);
                }

                previous = point;
            }

        }

        private void RefreshDebugPath()
        {
            if (!Spawned || Map == null || posHistory == null || stateMachine?.CurrentBehaviorDef == null)
            {
                return;
            }

            if (!stateMachine.CurrentBehaviorDef.DrawPath)
            {
                return;
            }

            SimpleColor color = CurrentBehaviorId == SandWormBehaviorIds.Charge
                ? SimpleColor.Red
                : SimpleColor.Cyan;

            IntVec3? previousCell = null;
            for (int step = 0; step < historyLength; step += PathDrawStep)
            {
                int index = (historyHead - step + historyLength) % historyLength;
                IntVec3 currentCell = posHistory[index].ToIntVec3();
                if (previousCell.HasValue && previousCell.Value != currentCell)
                {
                    Map.debugDrawer.FlashLine(previousCell.Value, currentCell, 2, color);
                }

                previousCell = currentCell;
            }
        }

        private Color GetPathColor()
        {
            switch (CurrentBehaviorId)
            {
                case SandWormBehaviorIds.Charge:
                    return new Color(1f, 0.25f, 0.2f, 0.95f);
                default:
                    return new Color(0.3f, 0.9f, 0.85f, 0.9f);
            }
        }

        private Color GetTargetLineColor()
        {
            switch (CurrentBehaviorId)
            {
                case SandWormBehaviorIds.Charge:
                    return new Color(1f, 0.85f, 0.2f, 0.95f);
                default:
                    return new Color(1f, 0.65f, 0.15f, 0.9f);
            }
        }

        internal void DrawProjectedPathOnGUI()
        {
            if (!ShouldShowProjectedPath() || !Spawned || Map == null || stateMachine?.CurrentBehaviorDef == null || !stateMachine.CurrentBehaviorDef.DrawPath)
            {
                return;
            }

            DrawTargetingLineOnGUI();

            int refreshInterval = SandWormMod.Settings != null
                ? Mathf.Max(1, SandWormMod.Settings.projectedPathRefreshTicks)
                : 20;
            int nowTick = Find.TickManager.TicksGame;
            if (cachedProjectedPoints == null || nowTick - lastProjectedRefreshTick >= refreshInterval)
            {
                RebuildProjectedPathCache();
                lastProjectedRefreshTick = nowTick;
            }

            // 从缓存（世界坐标）逐帧重算屏幕坐标 —— 相机移动时也能正确跟随
            if (cachedProjectedPointCount < 2) return;
            Color color = GetPathColor();
            Vector2 previous = cachedProjectedPoints[0].MapToUIPosition();
            for (int i = 1; i < cachedProjectedPointCount; i++)
            {
                Vector2 current = cachedProjectedPoints[i].MapToUIPosition();
                Widgets.DrawLine(previous, current, color, ProjectedPathLineWidth);
                previous = current;
            }
        }

        internal void DrawHitProxyDebugRectsOnGUI()
        {
            if (SandWormMod.Settings == null || !SandWormMod.Settings.showDeveloperSettings || !SandWormMod.Settings.showHitProxyDebugRects)
            {
                return;
            }

            if (!Spawned || Map == null || Find.CurrentMap != Map || hitProxies == null)
            {
                return;
            }

            LogHitProxyDebugInfoThrottled();

            for (int i = 0; i < BoneCount; i++)
            {
                DrawHitProxyRectOnGUI(i);
            }
        }

        private void DrawHitProxyRectOnGUI(int boneIndex)
        {
            bool hasSegment = TryGetSegmentData(boneIndex, out Vector3 center, out Quaternion rotation, out Vector2 size);
            SandWormHitProxyThing proxy = boneIndex >= 0 && boneIndex < hitProxies.Count ? hitProxies[boneIndex] : null;
            bool proxySpawned = proxy != null && !proxy.Destroyed && proxy.Spawned;
            if (!hasSegment && !proxySpawned)
            {
                return;
            }

            if (!hasSegment)
            {
                center = proxy.DrawPos;
                rotation = proxy.WorldRotation;
                size = proxy.DebugSize;
            }

            float halfWidth = Mathf.Max(0.5f, size.x * 0.5f);
            float halfLength = Mathf.Max(0.5f, size.y * 0.5f);
            Vector3 right = rotation * Vector3.right;
            Vector3 forward = rotation * Vector3.forward;
            right.y = 0f;
            forward.y = 0f;
            if (right.sqrMagnitude < 0.001f || forward.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
                forward = Vector3.forward;
            }

            right.Normalize();
            forward.Normalize();
            Vector2 cornerA = (center + right * halfWidth + forward * halfLength).MapToUIPosition();
            Vector2 cornerB = (center - right * halfWidth + forward * halfLength).MapToUIPosition();
            Vector2 cornerC = (center - right * halfWidth - forward * halfLength).MapToUIPosition();
            Vector2 cornerD = (center + right * halfWidth - forward * halfLength).MapToUIPosition();
            bool offMap = IsWorldPointOutsideMap(center);
            Color color = boneIndex == 0
                ? new Color(1f, 0.1f, 0.1f, 0.95f)
                : proxySpawned ? new Color(0.1f, 1f, 0.2f, 0.85f) : new Color(1f, 0.75f, 0.1f, 0.95f);
            if (offMap)
            {
                color = new Color(1f, 0.25f, 1f, 0.95f);
            }
            Widgets.DrawLine(cornerA, cornerB, color, 2f);
            Widgets.DrawLine(cornerB, cornerC, color, 2f);
            Widgets.DrawLine(cornerC, cornerD, color, 2f);
            Widgets.DrawLine(cornerD, cornerA, color, 2f);

            Vector2 centerUi = center.MapToUIPosition();
            Widgets.DrawLine(centerUi + new Vector2(-5f, 0f), centerUi + new Vector2(5f, 0f), Color.white, 1.5f);
            Widgets.DrawLine(centerUi + new Vector2(0f, -5f), centerUi + new Vector2(0f, 5f), Color.white, 1.5f);
        }

        private bool IsWorldPointOutsideMap(Vector3 point)
        {
            if (Map == null)
            {
                return true;
            }

            return point.x < 0f || point.z < 0f || point.x >= Map.Size.x || point.z >= Map.Size.z;
        }

        private void LogHitProxyDebugInfoThrottled()
        {
            int nowTick = Find.TickManager.TicksGame;
            if (nowTick - lastHitProxyDebugLogTick < 120)
            {
                return;
            }

            lastHitProxyDebugLogTick = nowTick;
            int spawnedCount = 0;
            int missingCount = 0;
            int offMapBoneCount = 0;
            int clampedProxyCount = 0;
            string sample = string.Empty;
            for (int i = 0; i < BoneCount; i++)
            {
                bool hasSegment = TryGetSegmentData(i, out Vector3 center, out Quaternion _, out Vector2 size);
                SandWormHitProxyThing proxy = i < hitProxies.Count ? hitProxies[i] : null;
                bool proxySpawned = proxy != null && !proxy.Destroyed && proxy.Spawned;
                if (hasSegment && IsWorldPointOutsideMap(center))
                {
                    offMapBoneCount++;
                }

                if (proxySpawned)
                {
                    spawnedCount++;
                    if (hasSegment && proxy.Position != ClampCellToMap(center.ToIntVec3()))
                    {
                        clampedProxyCount++;
                    }
                }
                else
                {
                    missingCount++;
                }

                if (sample.Length == 0 || i == 0 || i == hitProxies.Count / 2 || i == hitProxies.Count - 1)
                {
                    if (sample.Length > 0)
                    {
                        sample += "; ";
                    }

                    sample += "#" + i
                        + " bone=" + (hasSegment ? center.ToString("F1") : "missing")
                        + " offMap=" + (hasSegment && IsWorldPointOutsideMap(center))
                        + " proxy=" + (proxySpawned ? proxy.Position.ToString() : "missing")
                        + " size=" + size.ToString("F1");
                }
            }

            Log.Message("[SandWorm] HitProxy debug: total=" + BoneCount
                + ", spawned=" + spawnedCount
                + ", missing=" + missingCount
                + ", offMapBones=" + offMapBoneCount
                + ", clampedProxyMismatch=" + clampedProxyCount
                + ", wormPos=" + exactPos.ToString("F1")
                + ", samples=" + sample);
        }

        private void RebuildProjectedPathCache()
        {
            PredictedPathState predicted = new PredictedPathState
            {
                Position = exactPos,
                Velocity = velocity,
                YawDeg = headYawDeg,
                NoiseTime = noiseTime,
                BehaviorId = CurrentBehaviorId,
                CurrentTarget = blackboard.CurrentTarget,
                ChargeLocked = blackboard.ChargeLocked,
                ChargeDirection = blackboard.ChargeDirection,
                CurrentTick = Find.TickManager.TicksGame,
                LastBehaviorChangeTick = blackboard.LastBehaviorChangeTick,
                LastChargeTargetAcquireTick = blackboard.LastChargeTargetAcquireTick
            };

            int capacity = 1 + ProjectedPathTicks / ProjectedPathStep;
            if (cachedProjectedPoints == null || cachedProjectedPoints.Length < capacity)
            {
                cachedProjectedPoints = new Vector3[capacity];
            }

            int count = 0;
            cachedProjectedPoints[count++] = predicted.Position;
            for (int tick = 0; tick < ProjectedPathTicks; tick++)
            {
                SimulateProjectedPathTick(ref predicted);
                if ((tick + 1) % ProjectedPathStep == 0 && count < capacity)
                {
                    cachedProjectedPoints[count++] = predicted.Position;
                }
            }
            cachedProjectedPointCount = count;
        }

        private void DrawTargetingLineOnGUI()
        {
            Thing target = blackboard.CurrentTarget;
            if (!IsValidChargeTarget(target, ChargeDistance))
            {
                return;
            }

            Vector2 start = exactPos.MapToUIPosition();
            Vector2 end = target.DrawPos.MapToUIPosition();
            Widgets.DrawLine(start, end, GetTargetLineColor(), TargetLineWidth);
        }

        private void SimulateProjectedPathTick(ref PredictedPathState state)
        {
            if (state.BehaviorId == SandWormBehaviorIds.Charge)
            {
                SimulateChargeTick(ref state);
            }
            else
            {
                SimulateWanderTick(ref state);
            }

            state.CurrentTick++;
        }

        private void SimulateWanderTick(ref PredictedPathState state)
        {
            float moveSpeed = BaseWanderMoveSpeed * MoveSpeedScale;
            float maxTurn = BaseWanderMaxTurnPerTick * TurnRateScale;

            state.NoiseTime += WanderNoiseSpeed;

            float noiseTurn =
                (Mathf.PerlinNoise(state.NoiseTime + noiseOffset, 0f) - 0.5f) * 2f * maxTurn;
            float returnTurn = GetPredictedReturnToCenterTurn(
                state.Position,
                state.YawDeg,
                WanderReturnThresholdFactor,
                WanderReturnMargin,
                WanderReturnForce);
            float biasTurn = GetPredictedBiasTowardPlayerTurn(
                state.Position,
                state.YawDeg,
                WanderBiasTowardPlayerForce,
                WanderBiasTowardPlayerMaxTurn);
            float totalTurn = noiseTurn + returnTurn + biasTurn;

            state.YawDeg += totalTurn;
            ApplyPredictedMovement(ref state, moveSpeed, maxTurn, WanderTurnBlend);

            int ticksInBehavior = state.CurrentTick - state.LastBehaviorChangeTick + 1;
            if (ticksInBehavior < WanderMinTicksBeforeCharge)
            {
                return;
            }

            if (state.CurrentTick + 1 < state.LastChargeTargetAcquireTick)
            {
                return;
            }

            Thing target = FindPreferredChargeTargetProjected(state.Position, float.MaxValue);
            if (target == null)
            {
                return;
            }

            state.CurrentTarget = target;
            state.BehaviorId = SandWormBehaviorIds.Charge;
            state.ChargeLocked = false;
            state.ChargeDirection = Vector3.zero;
            state.LastBehaviorChangeTick = state.CurrentTick + 1;
        }

        private void SimulateChargeTick(ref PredictedPathState state)
        {
            float moveSpeed = BaseChargeMoveSpeed * MoveSpeedScale;
            float maxTurn = BaseChargeMaxTurnPerTick * TurnRateScale;

            Thing target = IsValidChargeTargetProjected(state.CurrentTarget, state.Position, ChargeDistance)
                ? state.CurrentTarget
                : null;

            Vector3 chargeDirection = state.ChargeDirection;
            if (!state.ChargeLocked && target != null)
            {
                float angle = GetSignedAngleToProjected(state.Position, state.YawDeg, target.DrawPos);
                float turn = Mathf.Clamp(angle * 0.04f, -maxTurn, maxTurn);
                state.YawDeg += turn;
                if (Mathf.Abs(angle) <= ChargeLockAngleThreshold)
                {
                    state.ChargeDirection = GetWorldForwardFromYaw(state.YawDeg);
                    state.ChargeLocked = true;
                }

                ApplyPredictedMovement(ref state, moveSpeed, maxTurn, ChargeTurnBlend);
            }
            else if (chargeDirection.sqrMagnitude > 0.001f || state.ChargeLocked)
            {
                if (chargeDirection.sqrMagnitude < 0.001f)
                {
                    chargeDirection = GetWorldForwardFromYaw(state.YawDeg);
                    state.ChargeDirection = chargeDirection;
                }

                state.YawDeg = Mathf.Atan2(-chargeDirection.x, chargeDirection.z) * Mathf.Rad2Deg;
                ApplyPredictedMovement(ref state, moveSpeed, maxTurn, ChargeTurnBlend);
            }

            int elapsed = state.CurrentTick - state.LastBehaviorChangeTick + 1;
            if (elapsed < ChargeMinCommitTicks)
            {
                state.CurrentTarget = target;
                return;
            }

            if (elapsed < ChargeMaxDurationTicks
                && IsValidChargeTargetProjected(target, state.Position, ChargeDistance)
                && !WillLeaveMapSoonProjected(state))
            {
                state.CurrentTarget = target;
                return;
            }

            state.CurrentTarget = null;
            state.ChargeLocked = false;
            state.ChargeDirection = Vector3.zero;
            state.LastChargeTargetAcquireTick = state.CurrentTick + 1 + ChargeCooldownTicks;
            state.BehaviorId = SandWormBehaviorIds.Wander;
            state.LastBehaviorChangeTick = state.CurrentTick + 1;
        }

        private void ApplyPredictedMovement(ref PredictedPathState state, float moveSpeed, float maxTurnPerTick, float turnBlend)
        {
            Vector3 worldForward = GetWorldForwardFromYaw(state.YawDeg);

            if (state.Velocity.sqrMagnitude < 0.000001f)
            {
                state.Velocity = worldForward * moveSpeed;
            }
            else
            {
                float maxTurnRad = maxTurnPerTick * Mathf.Deg2Rad;
                Vector3 targetVelocity = worldForward * moveSpeed;
                state.Velocity = Vector3.RotateTowards(state.Velocity, targetVelocity, maxTurnRad, moveSpeed);
                state.Velocity = Vector3.Lerp(state.Velocity, targetVelocity, turnBlend);
            }

            state.Position += state.Velocity;
        }

        private float GetPredictedReturnToCenterTurn(
            Vector3 projectedPos,
            float projectedYawDeg,
            float thresholdFactor,
            float returnMargin,
            float returnForce)
        {
            Vector3 toCenter = mapCenter - projectedPos;
            toCenter.y = 0f;

            float distanceFromCenter = toCenter.magnitude;
            float threshold = mapRadius * thresholdFactor;
            if (distanceFromCenter < threshold)
            {
                return 0f;
            }

            float excess = distanceFromCenter - threshold;
            float maxExcess = mapRadius * (1f - thresholdFactor) + returnMargin;
            float strength = Mathf.Clamp01(excess / maxExcess) * returnForce;
            float signedAngle = GetSignedAngleToProjected(projectedPos, projectedYawDeg, mapCenter);
            float absSigned = signedAngle < 0f ? -signedAngle : signedAngle;
            if (absSigned < 1f)
            {
                return 0f;
            }

            return Mathf.Clamp(-signedAngle, -strength, strength);
        }

        private float GetPredictedBiasTowardPlayerTurn(
            Vector3 projectedPos,
            float projectedYawDeg,
            float biasForce,
            float maxBiasTurn)
        {
            if (Map == null)
            {
                return 0f;
            }

            Pawn closestPawn = null;
            float closestDistSq = float.MaxValue;
            IReadOnlyList<Pawn> pawns = Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned || pawn.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                float distSq = (pawn.DrawPos - projectedPos).sqrMagnitude;
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestPawn = pawn;
                }
            }

            if (closestPawn != null)
            {
                float angleToPawn = GetSignedAngleToProjected(projectedPos, projectedYawDeg, closestPawn.DrawPos);
                return Mathf.Clamp(angleToPawn * biasForce, -maxBiasTurn, maxBiasTurn);
            }

            List<Building> buildings = Map.listerBuildings.allBuildingsColonist;
            float closestBuildingDistSq = float.MaxValue;
            Vector3? closestBuildingPos = null;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building building = buildings[i];
                if (building == null || building.Destroyed || !building.Spawned)
                {
                    continue;
                }

                float distSq = (building.DrawPos - projectedPos).sqrMagnitude;
                if (distSq < closestBuildingDistSq)
                {
                    closestBuildingDistSq = distSq;
                    closestBuildingPos = building.DrawPos;
                }
            }

            if (!closestBuildingPos.HasValue)
            {
                return 0f;
            }

            float angleToBuilding = GetSignedAngleToProjected(projectedPos, projectedYawDeg, closestBuildingPos.Value);
            return Mathf.Clamp(angleToBuilding * biasForce, -maxBiasTurn, maxBiasTurn);
        }

        private Thing FindPreferredChargeTargetProjected(Vector3 projectedPos, float maxDistance)
        {
            if (Map == null)
            {
                return null;
            }

            Pawn bestPlayerPawn = null;
            float bestPlayerPawnDistanceSq = float.MaxValue;
            Pawn bestOtherPawn = null;
            float bestOtherPawnDistanceSq = float.MaxValue;
            IReadOnlyList<Pawn> pawns = Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!IsValidChargeTargetProjected(pawn, projectedPos, maxDistance))
                {
                    continue;
                }

                float distanceSq = (pawn.DrawPos - projectedPos).sqrMagnitude;
                if (pawn.Faction == Faction.OfPlayer)
                {
                    if (distanceSq >= bestPlayerPawnDistanceSq)
                    {
                        continue;
                    }

                    bestPlayerPawn = pawn;
                    bestPlayerPawnDistanceSq = distanceSq;
                }
                else
                {
                    if (distanceSq >= bestOtherPawnDistanceSq)
                    {
                        continue;
                    }

                    bestOtherPawn = pawn;
                    bestOtherPawnDistanceSq = distanceSq;
                }
            }

            if (bestPlayerPawn != null)
            {
                return bestPlayerPawn;
            }

            if (bestOtherPawn != null)
            {
                return bestOtherPawn;
            }

            Building bestTurret = null;
            float bestTurretDistanceSq = float.MaxValue;
            List<Building> buildings = Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building building = buildings[i];
                if (!IsValidChargeTargetProjected(building, projectedPos, maxDistance))
                {
                    continue;
                }

                if (building.def?.building == null || !building.def.building.IsTurret)
                {
                    continue;
                }

                float distanceSq = (building.DrawPos - projectedPos).sqrMagnitude;
                if (distanceSq >= bestTurretDistanceSq)
                {
                    continue;
                }

                bestTurret = building;
                bestTurretDistanceSq = distanceSq;
            }

            return bestTurret;
        }

        private bool IsValidChargeTargetProjected(Thing target, Vector3 projectedPos, float maxDistance)
        {
            if (target == null || target.Destroyed || !target.Spawned || target.Map != Map)
            {
                return false;
            }

            if ((target.DrawPos - projectedPos).sqrMagnitude > maxDistance * maxDistance)
            {
                return false;
            }

            if (target == this || target is SandWormHitProxyThing)
            {
                return false;
            }

            Pawn pawn = target as Pawn;
            if (pawn != null)
            {
                return !pawn.Dead && !pawn.Downed;
            }

            Building building = target as Building;
            return building != null
                && building.def?.building != null
                && building.def.building.IsTurret;
        }

        private float GetSignedAngleToProjected(Vector3 projectedPos, float projectedYawDeg, Vector3 worldTarget)
        {
            Vector3 toTarget = worldTarget - projectedPos;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            Vector3 forward = GetWorldForwardFromYaw(projectedYawDeg);
            forward.Normalize();
            toTarget.Normalize();
            return -Vector3.SignedAngle(forward, toTarget, Vector3.up);
        }

        private static Vector3 GetWorldForwardFromYaw(float yawDeg)
        {
            float yawRad = yawDeg * Mathf.Deg2Rad;
            return new Vector3(-Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
        }

        private static float GetMoveSpeedMultiplier()
        {
            return SandWormMod.Settings != null ? Mathf.Clamp(SandWormMod.Settings.moveSpeedMultiplier, SandWormSettings.MinMultiplier, SandWormSettings.MaxMultiplier) : 1f;
        }

        private static float GetHitPointMultiplier()
        {
            return SandWormMod.Settings != null ? Mathf.Clamp(SandWormMod.Settings.hitPointMultiplier, SandWormSettings.MinMultiplier, SandWormSettings.MaxMultiplier) : 1f;
        }

        private static float GetModelYOffset()
        {
            return SandWormMod.Settings != null ? Mathf.Clamp(SandWormMod.Settings.modelYOffset, -5f, 5f) : 0f;
        }

        internal static int ScaledMaxHitPoints()
        {
            return ScaledMaxHitPointsForBase(50000);
        }

        // ScaledMaxHitPointsForBase 负责按全局难度倍率计算指定沙虫基础血量的最终血量。
        internal static int ScaledMaxHitPointsForBase(int baseMaxHitPoints)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseMaxHitPoints * GetHitPointMultiplier()));
        }

        private static float GetIncomingDamageMultiplier()
        {
            return SandWormMod.Settings != null ? Mathf.Clamp(SandWormMod.Settings.playerDamageMultiplier, SandWormSettings.MinMultiplier, SandWormSettings.MaxMultiplier) : 1f;
        }

        private static bool ShouldShowProjectedPath()
        {
            return SandWormMod.Settings == null || SandWormMod.Settings.showProjectedPath;
        }

        private bool WillLeaveMapSoonProjected(PredictedPathState state)
        {
            if (Map == null || !state.ChargeLocked)
            {
                return false;
            }

            Vector3 direction = state.ChargeDirection;
            if (direction.sqrMagnitude < 0.001f)
            {
                return false;
            }

            direction.Normalize();
            Vector3 futurePos = state.Position + direction * 18f;
            IntVec3 futureCell = futurePos.ToIntVec3();
            if (!futureCell.InBounds(Map))
            {
                return true;
            }

            return futureCell.x < ChargeBoundaryMargin
                || futureCell.z < ChargeBoundaryMargin
                || futureCell.x >= Map.Size.x - ChargeBoundaryMargin
                || futureCell.z >= Map.Size.z - ChargeBoundaryMargin;
        }

        private struct PredictedPathState
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float YawDeg;
            public float NoiseTime;
            public string BehaviorId;
            public Thing CurrentTarget;
            public bool ChargeLocked;
            public Vector3 ChargeDirection;
            public int CurrentTick;
            public int LastBehaviorChangeTick;
            public int LastChargeTargetAcquireTick;
        }
    }
}
