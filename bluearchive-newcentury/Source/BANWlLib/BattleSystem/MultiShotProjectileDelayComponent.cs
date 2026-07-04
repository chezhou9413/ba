using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 多发投射物延迟队列，负责在地图 tick 中按配置时间逐发生成原版子弹。
    public class MultiShotProjectileDelayComponent : MapComponent
    {
        private List<PendingMultiShotProjectile> pendingProjectiles = new List<PendingMultiShotProjectile>();

        // 创建地图队列组件，负责绑定当前地图的延迟多发投射物任务。
        public MultiShotProjectileDelayComponent(Map map) : base(map)
        {
        }

        // 地图每 tick 更新，负责触发到期的待发射子弹。
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            FireDueProjectiles(Find.TickManager.TicksGame);
        }

        // 保存和读取发射队列，负责让延迟发射在存读档后继续执行。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pendingProjectiles, "pendingMultiShotProjectiles", LookMode.Deep);
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
                    fireAtTick = currentTick + (shot?.delayTicks ?? 0),
                    caster = caster,
                    targetPawn = target.Pawn,
                    targetThing = target.Thing,
                    targetCell = target.Cell,
                    projectileDef = projectileDef,
                    canHitOwnPawn = props.canHitOwnPawn,
                    canHitOwnBuilding = props.canHitOwnBuilding,
                    preventFriendlyFire = preventFriendlyFire
                });
            }

            component.FireDueProjectiles(currentTick);
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
                pendingProjectile.Fire();
            }
        }
    }

    // 待发射多发投射物，负责保存单发延迟子弹的施法者、目标和发射参数。
    public class PendingMultiShotProjectile : IExposable
    {
        public int fireAtTick;
        public Pawn caster;
        public Pawn targetPawn;
        public Thing targetThing;
        public IntVec3 targetCell;
        public ThingDef projectileDef;
        public bool canHitOwnPawn;
        public bool canHitOwnBuilding;
        public bool preventFriendlyFire;

        // 发射子弹，负责按目标当前状态生成原版 Projectile 并交给现有 BA 投射物链路处理。
        public void Fire()
        {
            if (caster == null || caster.Map == null || !caster.Spawned)
            {
                return;
            }

            if (projectileDef == null)
            {
                Log.Error("[BANW] 待发射多发投射物缺少 projectileDef。");
                return;
            }

            LocalTargetInfo fireTarget = ResolveFireTarget(caster.Map);
            if (!fireTarget.IsValid)
            {
                return;
            }

            Projectile projectile = GenSpawn.Spawn(projectileDef, caster.Position, caster.Map) as Projectile;
            if (projectile == null)
            {
                Log.Error("[BANW] 多发投射物 " + projectileDef.defName + " 不是 Projectile。");
                return;
            }

            projectile.Launch(caster, caster.DrawPos, fireTarget, fireTarget, ProjectileHitFlags.IntendedTarget, preventFriendlyFire);
            RegisterBattleContext(projectile);
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
                attackPowerRatio = extension?.attackPowerRatio ?? 0f,
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
            Scribe_Values.Look(ref fireAtTick, "fireAtTick", 0);
            Scribe_References.Look(ref caster, "caster");
            Scribe_References.Look(ref targetPawn, "targetPawn");
            Scribe_References.Look(ref targetThing, "targetThing");
            Scribe_Values.Look(ref targetCell, "targetCell", IntVec3.Invalid);
            Scribe_Defs.Look(ref projectileDef, "projectileDef");
            Scribe_Values.Look(ref canHitOwnPawn, "canHitOwnPawn", false);
            Scribe_Values.Look(ref canHitOwnBuilding, "canHitOwnBuilding", false);
            Scribe_Values.Look(ref preventFriendlyFire, "preventFriendlyFire", false);
        }
    }
}
