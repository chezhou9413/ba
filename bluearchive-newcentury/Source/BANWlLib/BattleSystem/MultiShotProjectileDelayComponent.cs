using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 多发投射物延迟队列，负责在地图 tick 中按配置时间逐发生成原版子弹。
    public class MultiShotProjectileDelayComponent : MapComponent
    {
        private List<PendingMultiShotProjectile> pendingProjectiles = new List<PendingMultiShotProjectile>();
        private int nextSequenceId = 1;

        // 创建地图队列组件，负责绑定当前地图的延迟多发投射物任务。
        public MultiShotProjectileDelayComponent(Map map) : base(map)
        {
        }

        // 地图每 tick 更新，负责触发到期的待发射子弹。
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            int currentTick = Find.TickManager.TicksGame;
            MaintainCasterLocks(currentTick);
            FireDueProjectiles(currentTick);
        }

        // 保存和读取发射队列，负责让延迟发射在存读档后继续执行。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pendingProjectiles, "pendingMultiShotProjectiles", LookMode.Deep);
            Scribe_Values.Look(ref nextSequenceId, "nextMultiShotSequenceId", 1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pendingProjectiles == null)
            {
                pendingProjectiles = new List<PendingMultiShotProjectile>();
            }
        }

        // 入队多发投射物，负责把技能配置转换为地图上的待发射任务。
        public static void Queue(Pawn caster, LocalTargetInfo target, CompProperties_AbilityMultiShotProjectile props, bool preventFriendlyFire)
        {
            if (caster?.Map == null || props == null)
            {
                Log.Error("[BANW] 多发投射物入队缺少施法者、地图或配置。");
                return;
            }

            MultiShotProjectileDelayComponent component = caster.Map.GetComponent<MultiShotProjectileDelayComponent>();
            if (component == null)
            {
                Log.Error("[BANW] 地图缺少 MultiShotProjectileDelayComponent，无法延迟发射多发子弹。");
                return;
            }

            int currentTick = Find.TickManager.TicksGame;
            int sequenceId = component.nextSequenceId++;
            IntVec3 casterStartCell = caster.Position;
            for (int i = 0; i < props.shots.Count; i++)
            {
                MultiShotProjectileShotConfig shot = props.shots[i];
                ThingDef projectileDef = shot?.projectileDef ?? props.projectileDef;
                if (projectileDef == null)
                {
                    Log.Error("[BANW] 多发投射物第 " + i + " 发缺少 projectileDef。");
                    continue;
                }

                component.pendingProjectiles.Add(new PendingMultiShotProjectile
                {
                    sequenceId = sequenceId,
                    fireAtTick = currentTick + (shot?.delayTicks ?? 0),
                    caster = caster,
                    casterStartCell = casterStartCell,
                    targetPawn = target.Pawn,
                    targetThing = target.Thing,
                    targetCell = target.Cell,
                    projectileDef = projectileDef,
                    attackPowerRatio = shot?.attackPowerRatio ?? -1f,
                    canHitOwnPawn = props.canHitOwnPawn,
                    canHitOwnBuilding = props.canHitOwnBuilding,
                    cancelWhenCasterMoved = props.cancelWhenCasterMoved,
                    requireCasterCanShoot = props.requireCasterCanShoot,
                    requireLineOfSightEachShot = props.requireLineOfSightEachShot,
                    lockCasterDuringSequence = props.lockCasterDuringSequence,
                    drawPrimaryWeaponAimDuringSequence = props.drawPrimaryWeaponAimDuringSequence,
                    preventFriendlyFire = preventFriendlyFire
                });
            }

            component.MaintainCasterLocks(currentTick);
            component.FireDueProjectiles(currentTick);
        }

        // 维持施法者锁定，负责让延迟连射剩余子弹存在时小人不能移动并保持瞄准。
        private void MaintainCasterLocks(int currentTick)
        {
            if (pendingProjectiles.NullOrEmpty())
            {
                return;
            }

            HashSet<int> maintainedSequences = new HashSet<int>();
            for (int i = 0; i < pendingProjectiles.Count; i++)
            {
                PendingMultiShotProjectile pendingProjectile = pendingProjectiles[i];
                if (pendingProjectile == null || pendingProjectile.fireAtTick < currentTick || maintainedSequences.Contains(pendingProjectile.sequenceId))
                {
                    continue;
                }

                pendingProjectile.MaintainCasterLock();
                maintainedSequences.Add(pendingProjectile.sequenceId);
            }
        }

        // 查询施法者是否还有待发射子弹，负责让原版 JobDriver 等待同一轮延迟连射结束。
        public bool HasPendingForCaster(Pawn caster)
        {
            if (caster == null || pendingProjectiles.NullOrEmpty())
            {
                return false;
            }

            for (int i = 0; i < pendingProjectiles.Count; i++)
            {
                if (pendingProjectiles[i]?.caster == caster)
                {
                    return true;
                }
            }

            return false;
        }

        // 触发到期投射物，负责倒序移除已处理任务。
        private void FireDueProjectiles(int currentTick)
        {
            if (pendingProjectiles.NullOrEmpty())
            {
                return;
            }

            for (int i = pendingProjectiles.Count - 1; i >= 0; i--)
            {
                PendingMultiShotProjectile pendingProjectile = pendingProjectiles[i];
                if (pendingProjectile == null || pendingProjectile.fireAtTick > currentTick)
                {
                    continue;
                }

                pendingProjectiles.RemoveAt(i);
                PendingMultiShotProjectileResult result = pendingProjectile.Fire();
                if (result == PendingMultiShotProjectileResult.CancelSequence)
                {
                    RemoveSequence(pendingProjectile.sequenceId);
                }
            }
        }

        // 移除同一次施法的剩余投射物，负责在站桩连射被打断时清理后续子弹。
        private void RemoveSequence(int sequenceId)
        {
            for (int i = pendingProjectiles.Count - 1; i >= 0; i--)
            {
                if (pendingProjectiles[i]?.sequenceId == sequenceId)
                {
                    pendingProjectiles.RemoveAt(i);
                }
            }
        }
    }

    // 延迟子弹处理结果，负责告诉地图队列是否继续保留同一轮后续子弹。
    public enum PendingMultiShotProjectileResult
    {
        Fired,
        Skipped,
        CancelSequence
    }

    // 待发射多发投射物，负责保存单发延迟子弹的施法者、目标和发射参数。
    public class PendingMultiShotProjectile : IExposable
    {
        public int sequenceId;
        public int fireAtTick;
        public Pawn caster;
        public IntVec3 casterStartCell;
        public Pawn targetPawn;
        public Thing targetThing;
        public IntVec3 targetCell;
        public ThingDef projectileDef;
        public float attackPowerRatio = -1f;
        public bool canHitOwnPawn;
        public bool canHitOwnBuilding;
        public bool cancelWhenCasterMoved;
        public bool requireCasterCanShoot;
        public bool requireLineOfSightEachShot;
        public bool lockCasterDuringSequence;
        public bool drawPrimaryWeaponAimDuringSequence;
        public bool preventFriendlyFire;

        // 维持施法者锁定，负责在延迟发射等待期间停止移动并刷新主武器瞄准姿态。
        public void MaintainCasterLock()
        {
            if (caster == null || caster.Map == null || !caster.Spawned)
            {
                return;
            }

            if (!lockCasterDuringSequence && !drawPrimaryWeaponAimDuringSequence)
            {
                return;
            }

            LocalTargetInfo aimTarget = ResolveFireTarget(caster.Map);
            if (!aimTarget.IsValid)
            {
                return;
            }

            if (lockCasterDuringSequence)
            {
                caster.pather?.StopDead();
            }

            caster.rotationTracker?.FaceTarget(aimTarget);
            if (drawPrimaryWeaponAimDuringSequence)
            {
                caster.stances?.SetStance(new MultiShotAimStance(2, aimTarget));
            }
        }

        // 发射子弹，负责按目标当前状态生成原版 Projectile 并交给现有 BA 投射物链路处理。
        public PendingMultiShotProjectileResult Fire()
        {
            if (caster == null || caster.Map == null || !caster.Spawned)
            {
                return PendingMultiShotProjectileResult.CancelSequence;
            }

            if (projectileDef == null)
            {
                Log.Error("[BANW] 待发射多发投射物缺少 projectileDef。");
                return PendingMultiShotProjectileResult.Skipped;
            }

            if (!CanCasterContinue())
            {
                return PendingMultiShotProjectileResult.CancelSequence;
            }

            LocalTargetInfo fireTarget = ResolveFireTarget(caster.Map);
            if (!fireTarget.IsValid)
            {
                return PendingMultiShotProjectileResult.CancelSequence;
            }

            if (requireLineOfSightEachShot && !GenSight.LineOfSight(caster.Position, fireTarget.Cell, caster.Map, true))
            {
                return PendingMultiShotProjectileResult.CancelSequence;
            }

            Projectile projectile = GenSpawn.Spawn(projectileDef, caster.Position, caster.Map) as Projectile;
            if (projectile == null)
            {
                Log.Error("[BANW] 多发投射物 " + projectileDef.defName + " 不是 Projectile。");
                return PendingMultiShotProjectileResult.Skipped;
            }

            projectile.Launch(caster, caster.DrawPos, fireTarget, fireTarget, ProjectileHitFlags.IntendedTarget, preventFriendlyFire);
            RegisterBattleContext(projectile);
            return PendingMultiShotProjectileResult.Fired;
        }

        // 检查施法者状态，负责决定站桩连射是否还能继续。
        private bool CanCasterContinue()
        {
            if (cancelWhenCasterMoved && casterStartCell.IsValid && caster.Position != casterStartCell)
            {
                return false;
            }

            if (!requireCasterCanShoot)
            {
                return true;
            }

            if (caster.Destroyed || caster.Dead || caster.Downed)
            {
                return false;
            }

            if (caster.stances?.stunner?.Stunned == true)
            {
                return false;
            }

            if (caster.health?.capacities == null)
            {
                return false;
            }

            return caster.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)
                && caster.health.capacities.CapableOf(PawnCapacityDefOf.Sight)
                && caster.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness);
        }

        // 解析本发目标，负责让 Pawn 目标按当前所在位置发射，非 Pawn 目标按施法时格子发射。
        private LocalTargetInfo ResolveFireTarget(Map casterMap)
        {
            if (targetPawn != null)
            {
                if (targetPawn.Destroyed || targetPawn.Dead || targetPawn.MapHeld != casterMap)
                {
                    return LocalTargetInfo.Invalid;
                }

                return targetPawn;
            }

            if (targetThing != null && !targetThing.Destroyed && targetThing.MapHeld == casterMap)
            {
                return targetThing;
            }

            if (targetCell.IsValid && targetCell.InBounds(casterMap))
            {
                return targetCell;
            }

            return LocalTargetInfo.Invalid;
        }

        // 注册本技能的投射物战斗参数，负责让友军和己方建筑命中开关能按技能配置覆盖。
        private void RegisterBattleContext(Projectile projectile)
        {
            BattleProjectileExtension extension = projectileDef.GetModExtension<BattleProjectileExtension>();
            ProjectileBattleData data = new ProjectileBattleData
            {
                weaponBaseAttack = projectileDef.projectile?.GetDamageAmount(null) ?? 0f,
                attackPowerRatio = attackPowerRatio >= 0f ? attackPowerRatio : extension?.attackPowerRatio ?? 0f,
                baseMasteryMultiplier = extension?.baseMasteryMultiplier ?? 1f,
                shieldPowerRatio = extension?.shieldPowerRatio ?? 0f,
                shieldHediffDef = extension?.shieldHediffDef,
                isNormalAttack = extension?.isNormalAttack ?? false,
                useNormalAttackStat = false,
                isShield = extension?.isShield ?? false,
                isExSkill = extension?.isExSkill ?? false,
                canCrit = extension?.canCrit ?? true,
                alwaysCrit = extension?.alwaysCrit ?? false,
                alwaysShowCriticalText = extension?.alwaysShowCriticalText ?? false,
                applyAffinity = extension?.applyAffinity ?? true,
                canHitOwnPawn = canHitOwnPawn || extension?.canHitOwnPawn == true,
                canHitOwnBuilding = canHitOwnBuilding || extension?.canHitOwnBuilding == true,
                hasCustomExtension = extension != null || canHitOwnPawn || canHitOwnBuilding
            };

            ProjectileBattleContext.Register(projectile, data);
            if (!data.isNormalAttack && projectile.DamageDef != null)
            {
                ProjectileBattleContext.RegisterSkillDamage(caster, projectile.DamageDef, data);
            }
        }

        // 保存和读取待发射任务，负责支持延迟发射期间存读档。
        public void ExposeData()
        {
            Scribe_Values.Look(ref sequenceId, "sequenceId", 0);
            Scribe_Values.Look(ref fireAtTick, "fireAtTick", 0);
            Scribe_References.Look(ref caster, "caster");
            Scribe_Values.Look(ref casterStartCell, "casterStartCell", IntVec3.Invalid);
            Scribe_References.Look(ref targetPawn, "targetPawn");
            Scribe_References.Look(ref targetThing, "targetThing");
            Scribe_Values.Look(ref targetCell, "targetCell", IntVec3.Invalid);
            Scribe_Defs.Look(ref projectileDef, "projectileDef");
            Scribe_Values.Look(ref attackPowerRatio, "attackPowerRatio", -1f);
            Scribe_Values.Look(ref canHitOwnPawn, "canHitOwnPawn", false);
            Scribe_Values.Look(ref canHitOwnBuilding, "canHitOwnBuilding", false);
            Scribe_Values.Look(ref cancelWhenCasterMoved, "cancelWhenCasterMoved", false);
            Scribe_Values.Look(ref requireCasterCanShoot, "requireCasterCanShoot", false);
            Scribe_Values.Look(ref requireLineOfSightEachShot, "requireLineOfSightEachShot", false);
            Scribe_Values.Look(ref lockCasterDuringSequence, "lockCasterDuringSequence", false);
            Scribe_Values.Look(ref drawPrimaryWeaponAimDuringSequence, "drawPrimaryWeaponAimDuringSequence", false);
            Scribe_Values.Look(ref preventFriendlyFire, "preventFriendlyFire", false);
        }
    }
}
