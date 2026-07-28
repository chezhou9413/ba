using HarmonyLib;
using BANWlLib.Projectiles;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    [HarmonyPatch(typeof(Projectile), "get_DamageAmount")]
    public static class BattleProjectileDamagePatch
    {
        public static void Postfix(Projectile __instance, ref int __result)
        {
            if (__instance is Projectile_PiercingArea ||
                !(__instance?.Launcher is Pawn launcher) ||
                !ProjectileBattleContext.TryGet(__instance, out ProjectileBattleData data) ||
                !ProjectileBattleContext.TryGetImpactTarget(__instance, out Thing target))
            {
                return;
            }

            if (data.hasCustomExtension && BattleProjectileTargetFilter.IsBlockedOwnTarget(launcher, target, data))
            {
                __result = 0;
                return;
            }

            BattleDamageResult result = BattleStatUtility.BuildDamageResult(new BattleDamageRequest
            {
                instigator = launcher,
                target = target,
                damageDef = __instance.DamageDef,
                weaponBaseAttack = data.weaponBaseAttack,
                attackPowerRatio = data.attackPowerRatio,
                baseMasteryMultiplier = data.baseMasteryMultiplier,
                penetration = __instance.ArmorPenetration,
                isNormalAttack = data.isNormalAttack,
                useNormalAttackStat = data.useNormalAttackStat,
                canCrit = data.canCrit,
                alwaysCrit = data.alwaysCrit,
                alwaysShowCriticalText = data.alwaysShowCriticalText,
                applyAffinity = data.applyAffinity,
                isExSkill = data.isExSkill
            });

            BattleFormulaDebugUtility.LogDamagePreview(new BattleDamageRequest
            {
                instigator = launcher,
                target = target,
                damageDef = __instance.DamageDef,
                weaponBaseAttack = data.weaponBaseAttack,
                attackPowerRatio = data.attackPowerRatio,
                baseMasteryMultiplier = data.baseMasteryMultiplier,
                penetration = __instance.ArmorPenetration,
                isNormalAttack = data.isNormalAttack,
                useNormalAttackStat = data.useNormalAttackStat,
                canCrit = data.canCrit,
                alwaysCrit = data.alwaysCrit,
                alwaysShowCriticalText = data.alwaysShowCriticalText,
                applyAffinity = data.applyAffinity,
                isExSkill = data.isExSkill
            }, result);
            BattleDamageDisplayState.RegisterManualDamage(target, launcher, result.isCrit);
            BattleDamageDisplayState.RegisterCriticalFloatText(target, result.isCrit || data.alwaysShowCriticalText);
            __result = Mathf.Max(0, Mathf.RoundToInt(result.finalAmount));
        }
    }

    // 投射物发射补丁，负责把普通攻击和技能弹的战斗参数注册到命中上下文。
    [HarmonyPatch(typeof(Projectile), "Launch", typeof(Thing), typeof(UnityEngine.Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef))]
    public static class BattleProjectileLaunchPatch
    {
        // 发射后注册战斗上下文，负责让原版 Projectile 命中时也能使用统一攻击公式。
        public static void Postfix(Projectile __instance, Thing launcher)
        {
            if (__instance == null || __instance is Projectile_PiercingArea || !(launcher is Pawn launcherPawn))
            {
                return;
            }

            BattleProjectileExtension extension = __instance.def.GetModExtension<BattleProjectileExtension>();
            bool isPlainWeaponAttack = extension == null && IsCurrentWeaponProjectile(launcherPawn, __instance.def);
            if (extension == null && !isPlainWeaponAttack)
            {
                return;
            }

            ProjectileBattleData data = new ProjectileBattleData
            {
                weaponBaseAttack = __instance.def?.projectile?.GetDamageAmount(null) ?? 0f,
                attackPowerRatio = extension?.attackPowerRatio ?? 0f,
                baseMasteryMultiplier = extension?.baseMasteryMultiplier ?? 1f,
                shieldPowerRatio = extension?.shieldPowerRatio ?? 0f,
                shieldHediffDef = extension?.shieldHediffDef,
                isNormalAttack = extension?.isNormalAttack ?? isPlainWeaponAttack,
                useNormalAttackStat = isPlainWeaponAttack,
                isShield = extension?.isShield ?? false,
                isExSkill = extension?.isExSkill ?? false,
                canCrit = extension?.canCrit ?? true,
                alwaysCrit = extension?.alwaysCrit ?? false,
                alwaysShowCriticalText = extension?.alwaysShowCriticalText ?? false,
                applyAffinity = extension?.applyAffinity ?? true,
                canHitOwnBuilding = extension?.canHitOwnBuilding ?? false,
                canHitOwnPawn = extension?.canHitOwnPawn ?? false,
                hasCustomExtension = extension != null
            };
            ProjectileBattleContext.Register(__instance, data);
            if (extension != null && !data.isNormalAttack && __instance.DamageDef != null)
            {
                ProjectileBattleContext.RegisterSkillDamage(launcher, __instance.DamageDef, data);
            }

        }

        // 判断投射物是否来自当前主武器，负责把普通攻击倍率限制在原版武器平A。
        private static bool IsCurrentWeaponProjectile(Pawn launcher, ThingDef projectileDef)
        {
            ThingWithComps weapon = launcher?.equipment?.Primary;
            if (weapon?.def?.Verbs == null || !weapon.def.IsRangedWeapon || projectileDef == null)
            {
                return false;
            }

            for (int i = 0; i < weapon.def.Verbs.Count; i++)
            {
                if (weapon.def.Verbs[i]?.defaultProjectile == projectileDef)
                {
                    return true;
                }
            }

            return false;
        }
    }

    //投射物目标过滤，负责让配置过战斗扩展的伤害弹不误伤己方单位。
    public static class BattleProjectileTargetFilter
    {
        //判断目标是否被己方过滤规则阻止，负责区分己方建筑和己方 Pawn。
        public static bool IsBlockedOwnTarget(Pawn launcher, Thing target, ProjectileBattleData data)
        {
            if (launcher?.Faction == null || target?.Faction == null || launcher.Faction != target.Faction)
            {
                return false;
            }

            if (target is Building)
            {
                return !data.canHitOwnBuilding;
            }

            if (target is Pawn)
            {
                return !data.canHitOwnPawn;
            }

            return false;
        }
    }

    // 子弹命中前注册施法者快照，负责让子弹附加的治疗 Hediff 能使用施法者的治疗力加成和 EX 倍率。
    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class ProjectileImpactHealContextPatch
    {
        // 在原版 Impact 执行前处理命中特效与施法者快照，负责保留投射物地图并让治疗 Hediff 读取施法者数据。
        public static void Prefix(Projectile __instance, Thing hitThing)
        {
            if (!(__instance is Projectile_PiercingArea))
            {
                DirectionalImpactEffectUtility.TryTrigger(
                    __instance,
                    hitThing,
                    DirectionalImpactEffectUtility.GetTravelDirection(__instance));
            }

            if (__instance != null && hitThing != null)
            {
                ProjectileBattleContext.RegisterImpactTarget(__instance, hitThing);
            }

            if (hitThing is Pawn targetPawn && __instance?.Launcher is Pawn casterPawn)
            {
                BattleCasterSnapshot snapshot = BattleStatUtility.CreateSnapshot(casterPawn);
                if (snapshot != null)
                {
                    HealProjectileContext.Register(targetPawn, snapshot);
                }
            }
        }
    }

    // 普通子弹多段命中补丁，负责在 Bullet 原本伤害后按配置追加多段统一战斗伤害。
    [HarmonyPatch(typeof(Bullet), "Impact", typeof(Thing), typeof(bool))]
    public static class ProjectileMultiHitImpactPatch
    {
        // 命中前保存多段伤害上下文，负责避免 Bullet 原始 Impact 销毁弹体后丢失施法者和配置。
        public static void Prefix(Bullet __instance, Thing hitThing, ref ProjectileMultiHitImpactState __state)
        {
            __state = null;
            if (__instance == null || hitThing == null || !(__instance.Launcher is Pawn launcher))
            {
                return;
            }

            ProjectileBattleContext.RegisterImpactTarget(__instance, hitThing);
            if (hitThing is Pawn targetPawn)
            {
                BattleCasterSnapshot snapshot = BattleStatUtility.CreateSnapshot(launcher);
                if (snapshot != null)
                {
                    HealProjectileContext.Register(targetPawn, snapshot);
                }
            }

            ProjectileMultiHitExtension extension = __instance.def.GetModExtension<ProjectileMultiHitExtension>();
            List<ProjectileExtraDamageConfig> extraDamages = extension?.extraDamages;
            if (extraDamages == null || extraDamages.Count == 0)
            {
                return;
            }

            __state = new ProjectileMultiHitImpactState
            {
                projectileDef = __instance.def,
                launcher = launcher,
                target = hitThing,
                extraDamages = extraDamages,
                damageIntervalTicks = extension.damageIntervalTicks,
                canHitOwnPawn = extension.canHitOwnPawn,
                canHitOwnBuilding = extension.canHitOwnBuilding,
                weaponBaseAttack = Mathf.Max(0f, __instance.def?.projectile?.GetDamageAmount(null) ?? 0f),
                armorPenetration = __instance.ArmorPenetration
            };
        }

        // 命中后入队多段伤害，负责让原始子弹伤害先完成，再由地图组件逐段执行追加结算。
        public static void Postfix(Bullet __instance, ProjectileMultiHitImpactState __state)
        {
            ProjectileMultiHitDelayComponent.Queue(__state);
            ProjectileBattleContext.Clear(__instance);
        }

        // 应用一段追加伤害，负责把 XML 段配置转为统一伤害请求。
        public static void ApplyExtraDamage(ProjectileMultiHitImpactState state, ProjectileExtraDamageConfig config)
        {
            if (config == null)
            {
                return;
            }

            DamageDef damageDef = config.ResolveDamageDef();
            if (damageDef == null)
            {
                Log.Error("[BANW] 投射物 " + state.projectileDef.defName + " 的多段追加伤害缺少 damageDef。");
                return;
            }

            if (IsBlockedOwnTarget(state, config))
            {
                return;
            }

            float penetration = config.penetration >= 0f ? config.penetration : state.armorPenetration;
            BattleStatUtility.ApplyDamage(new BattleDamageRequest
            {
                instigator = state.launcher,
                target = state.target,
                damageDef = damageDef,
                weaponBaseAttack = state.weaponBaseAttack,
                attackPowerRatio = config.attackPowerRatio,
                baseMasteryMultiplier = config.baseMasteryMultiplier,
                penetration = penetration,
                isNormalAttack = config.isNormalAttack,
                canCrit = config.canCrit,
                alwaysCrit = config.alwaysCrit,
                alwaysShowCriticalText = config.alwaysShowCriticalText,
                applyAffinity = config.applyAffinity,
                isExSkill = config.isExSkill
            });
        }

        // 判断追加伤害是否被己方过滤，负责避免多段伤害误伤己方 Pawn 或建筑。
        private static bool IsBlockedOwnTarget(ProjectileMultiHitImpactState state, ProjectileExtraDamageConfig config)
        {
            Pawn launcher = state?.launcher;
            Thing target = state?.target;
            if (launcher?.Faction == null || target?.Faction == null || launcher.Faction != target.Faction)
            {
                return false;
            }

            if (target is Building)
            {
                return !config.canHitOwnBuilding && !state.canHitOwnBuilding;
            }

            if (target is Pawn)
            {
                return !config.canHitOwnPawn && !state.canHitOwnPawn;
            }

            return false;
        }

        // 普通子弹多段命中上下文，负责跨过 Bullet 原始 Impact 保存追加段所需数据。
        public class ProjectileMultiHitImpactState
        {
            public ThingDef projectileDef;
            public Pawn launcher;
            public Thing target;
            public List<ProjectileExtraDamageConfig> extraDamages;
            public int damageIntervalTicks;
            public bool canHitOwnPawn;
            public bool canHitOwnBuilding;
            public float weaponBaseAttack;
            public float armorPenetration;

            // 判断是否需要延迟队列，负责支持外层统一间隔和单段独立延迟两种配置。
            public bool ShouldUseDelayQueue()
            {
                if (damageIntervalTicks > 0)
                {
                    return true;
                }

                if (extraDamages == null)
                {
                    return false;
                }

                for (int i = 0; i < extraDamages.Count; i++)
                {
                    if (extraDamages[i]?.delayTicks > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    // 物体销毁补丁，负责在投射物销毁时清理普通投射物战斗上下文。
    [HarmonyPatch(typeof(Thing), "Destroy", typeof(DestroyMode))]
    public static class BattleProjectileDestroyPatch
    {
        // 投射物销毁时清理上下文，负责避免后续复用 thingID 时读到旧数据。
        public static void Prefix(Thing __instance, DestroyMode mode)
        {
            if (__instance is Projectile projectile)
            {
                if (ProjectileBattleContext.HasImpactTarget(projectile))
                {
                    return;
                }

                ProjectileBattleContext.Clear(projectile);
            }
        }
    }
}
