using BANWlLib.BattleSystem;
using BANWlLib.DamageFontSystem;
using BANWlLib.DamageFontSystem.Comp;
using BANWlLib.DamageFontSystem.Setting;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

public class DamageFontSystemPatche
{
    public static Dictionary<int, bool> CritState = new Dictionary<int, bool>();

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_Pawn_PreApplyDamage
    {
        // 受伤前处理，负责给原版伤害补上暴击与属性克制，同时跳过已走统一战斗层的手动结算。
        public static void Prefix(Pawn __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (__instance == null || dinfo.Instigator == null)
            {
                return;
            }

            if (BattleDamageDisplayState.TryConsumeManualCritState(__instance, out bool manualCrit))
            {
                if (CritState.ContainsKey(__instance.thingIDNumber))
                {
                    CritState.Remove(__instance.thingIDNumber);
                }
                return;
            }

            if (TryApplySkillProjectileDamage(__instance, ref dinfo))
            {
                return;
            }

            if (TryApplyNormalRangedDamage(__instance, ref dinfo))
            {
                return;
            }

            DisableCriticalComp comp = Current.Game.GetComponent<DisableCriticalComp>();
            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker == null)
            {
                return;
            }

            string damageType = dinfo.Def.defName;
            bool disableCrit = comp != null && comp.DisableCritical.Any(p => p.defName == damageType);
            bool isForcedCrit = comp != null && comp.EnsureCritical.Any(p => p.defName == damageType);
            float finalAmount = dinfo.Amount;
            bool isCrit = false;
            if (!disableCrit)
            {
                float critChance = Mathf.Clamp01(attacker.GetStatValue(CriticalRef.BANW_CriticalChance) - BattleStatUtility.GetCriticalChanceResistance(__instance));
                float critMultiplier = Mathf.Max(1f, attacker.GetStatValue(CriticalRef.BANW_CriticalDamage) - BattleStatUtility.GetCriticalDamageResistance(__instance));
                isCrit = isForcedCrit || Rand.Value < critChance;
                if (isCrit)
                {
                    finalAmount *= critMultiplier;
                    CritState[__instance.thingIDNumber] = true;
                }
            }

            float affinityMultiplier = BattleStatUtility.GetAffinityMultiplier(attacker, __instance);
            if (Mathf.Abs(affinityMultiplier - 1f) > 0.0001f)
            {
                finalAmount *= affinityMultiplier;
            }

            if (isCrit)
            {
                dinfo.SetAmount(finalAmount);
            }
            else if (CritState.ContainsKey(__instance.thingIDNumber))
            {
                CritState.Remove(__instance.thingIDNumber);
                dinfo.SetAmount(finalAmount);
            }
            else if (Mathf.Abs(finalAmount - dinfo.Amount) > 0.0001f)
            {
                dinfo.SetAmount(finalAmount);
            }
        }

    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
    public static class Patch_Pawn_PostApplyDamage
    {
        public static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            if (!DamageFontMod.settings.enableDamageFloat)
            {
                BattleFormulaDebugUtility.LogDamageActual(__instance, dinfo, totalDamageDealt);
                return;
            }

            if (BattleDamageDisplayState.TryConsumeForcedCriticalFloatAmount(__instance, out float forcedAmount))
            {
                CriticalObjPool.showFixedDamageShow(forcedAmount, __instance);
                BattleFormulaDebugUtility.LogDamageActual(__instance, dinfo, totalDamageDealt);
                return;
            }

            bool isCrit = false;
            if (BattleDamageDisplayState.TryConsumeCriticalFloatText(__instance, out bool manualFloatTextCrit))
            {
                isCrit = manualFloatTextCrit;
            }
            else if (CritState.TryGetValue(__instance.thingIDNumber, out bool state))
            {
                isCrit = state;
                CritState.Remove(__instance.thingIDNumber);
            }

            if (!isCrit || totalDamageDealt <= 0.01f)
            {
                BattleFormulaDebugUtility.LogDamageActual(__instance, dinfo, totalDamageDealt);
                return;
            }

            CriticalObjPool.showCriticalShow(totalDamageDealt, __instance);
            BattleFormulaDebugUtility.LogDamageActual(__instance, dinfo, totalDamageDealt);
            }
        }

        // 应用技能投射物伤害，负责让爆炸和延迟命中的技能弹也进入新攻击力公式。
        private static bool TryApplySkillProjectileDamage(Pawn target, ref DamageInfo dinfo)
        {
            if (!(dinfo.Instigator is Pawn attacker) || !ProjectileBattleContext.TryGetSkillDamage(attacker, dinfo.Def, out ProjectileBattleData data))
            {
                return false;
            }

            if (data.hasCustomExtension && BattleProjectileTargetFilter.IsBlockedOwnTarget(attacker, target, data))
            {
                dinfo.SetAmount(0f);
                return true;
            }

            BattleDamageResult result = BattleStatUtility.BuildDamageResult(new BattleDamageRequest
            {
                instigator = attacker,
                target = target,
                damageDef = dinfo.Def,
                weaponBaseAttack = data.weaponBaseAttack,
                attackPowerRatio = data.attackPowerRatio,
                normalAttackMultiplier = data.normalAttackMultiplier,
                baseMasteryMultiplier = data.baseMasteryMultiplier,
                penetration = dinfo.ArmorPenetrationInt,
                isNormalAttack = data.isNormalAttack,
                canCrit = data.canCrit,
                alwaysShowCriticalText = data.alwaysShowCriticalText,
                applyAffinity = data.applyAffinity,
                isExSkill = data.isExSkill
            });

            BattleDamageDisplayState.RegisterCriticalFloatText(target, result.isCrit || data.alwaysShowCriticalText);
            dinfo.SetAmount(result.finalAmount);
            return true;
        }

        // 应用普通远程武器伤害，负责让原版子弹命中也使用角色最终攻击力公式。
        private static bool TryApplyNormalRangedDamage(Pawn target, ref DamageInfo dinfo)
        {
            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker == null || target == null || dinfo.Def == null)
            {
                return false;
            }

            ThingWithComps weapon = attacker.equipment?.Primary;
            ThingDef projectileDef = FindMatchingProjectileDef(weapon, dinfo.Def);
            if (projectileDef?.projectile == null)
            {
                return false;
            }

            float baseDamage = Mathf.Max(0f, projectileDef.projectile.GetDamageAmount(weapon));
            if (baseDamage <= 0f)
            {
                return false;
            }

            BattleDamageRequest request = new BattleDamageRequest
            {
                instigator = attacker,
                target = target,
                damageDef = dinfo.Def,
                weaponBaseAttack = baseDamage,
                normalAttackMultiplier = 1f,
                baseMasteryMultiplier = 1f,
                penetration = dinfo.ArmorPenetrationInt,
                isNormalAttack = true,
                canCrit = true,
                alwaysShowCriticalText = false,
                applyAffinity = true,
                isExSkill = false
            };

            BattleDamageResult result = BattleStatUtility.BuildDamageResult(request);
            BattleFormulaDebugUtility.LogDamagePreview(request, result);
            BattleDamageDisplayState.RegisterCriticalFloatText(target, result.isCrit);
            dinfo.SetAmount(result.finalAmount);
            return true;
        }

        // 查找本次 DamageDef 对应的主武器子弹，负责避免近战和非当前武器伤害误入普通远程公式。
        private static ThingDef FindMatchingProjectileDef(ThingWithComps weapon, DamageDef damageDef)
        {
            if (weapon?.def?.Verbs == null || !weapon.def.IsRangedWeapon || damageDef == null)
            {
                return null;
            }

            for (int i = 0; i < weapon.def.Verbs.Count; i++)
            {
                ThingDef projectileDef = weapon.def.Verbs[i]?.defaultProjectile;
                if (projectileDef?.projectile?.damageDef == damageDef)
                {
                    return projectileDef;
                }
            }

            return null;
        }
    }
