using HarmonyLib;
using BANWlLib.Projectiles;
using RimWorld;
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
                normalAttackMultiplier = data.normalAttackMultiplier,
                baseMasteryMultiplier = data.baseMasteryMultiplier,
                penetration = __instance.ArmorPenetration,
                isNormalAttack = data.isNormalAttack,
                canCrit = data.canCrit,
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
                normalAttackMultiplier = data.normalAttackMultiplier,
                baseMasteryMultiplier = data.baseMasteryMultiplier,
                penetration = __instance.ArmorPenetration,
                isNormalAttack = data.isNormalAttack,
                canCrit = data.canCrit,
                alwaysShowCriticalText = data.alwaysShowCriticalText,
                applyAffinity = data.applyAffinity,
                isExSkill = data.isExSkill
            }, result);
            BattleDamageDisplayState.RegisterManualDamage(target, launcher, result.isCrit);
            BattleDamageDisplayState.RegisterCriticalFloatText(target, result.isCrit || data.alwaysShowCriticalText);
            if (data.alwaysShowCriticalText)
            {
                BattleDamageDisplayState.RegisterForcedCriticalFloatAmount(target, result.finalAmount);
            }
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
            if (__instance == null || __instance is Projectile_PiercingArea || !(launcher is Pawn))
            {
                return;
            }

            BattleProjectileExtension extension = __instance.def.GetModExtension<BattleProjectileExtension>();
            ProjectileBattleData data = new ProjectileBattleData
            {
                weaponBaseAttack = __instance.def?.projectile?.GetDamageAmount(null) ?? 0f,
                attackPowerRatio = extension?.attackPowerRatio ?? 0f,
                normalAttackMultiplier = extension?.normalAttackMultiplier ?? 1f,
                baseMasteryMultiplier = extension?.baseMasteryMultiplier ?? 1f,
                shieldPowerRatio = extension?.shieldPowerRatio ?? 0f,
                shieldHediffDef = extension?.shieldHediffDef,
                isNormalAttack = extension?.isNormalAttack ?? (extension == null || extension.attackPowerRatio <= 0f),
                isShield = extension?.isShield ?? false,
                isExSkill = extension?.isExSkill ?? false,
                canCrit = extension?.canCrit ?? true,
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
        // 在原版 Impact 执行前注册施法者快照，负责在 DamageDef.additionalHediffs 附加 Hediff 时让 HealProjectileContext 有数据可读。
        public static void Prefix(Projectile __instance, Thing hitThing)
        {
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

    // 物体销毁补丁，负责在投射物销毁时清理普通投射物战斗上下文。
    [HarmonyPatch(typeof(Thing), "Destroy", typeof(DestroyMode))]
    public static class BattleProjectileDestroyPatch
    {
        // 投射物销毁时清理上下文，负责避免后续复用 thingID 时读到旧数据。
        public static void Prefix(Thing __instance, DestroyMode mode)
        {
            if (__instance is Projectile projectile)
            {
                ProjectileBattleContext.Clear(projectile);
            }
        }
    }
}
