using System.Collections.Generic;
using System.Linq;
using BANWlLib.BaDef;
using BANWlLib.DamageFontSystem;
using BANWlLib.Tool;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    public static class BattleStatUtility
    {
        private static DamageAffinityMatrixDef cachedAffinityMatrix;

        // 获取学生基础属性扩展，负责从 PawnKindDef 读取学生固有战斗属性。
        public static BattleBaseStatExtension GetBaseStatExtension(Pawn pawn)
        {
            return pawn?.kindDef?.GetModExtension<BattleBaseStatExtension>();
        }

        // 获取学生基础生命尺度平加，负责接入 HealthScale 计算。
        public static float GetBaseHealthFlat(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.healthFlat ?? 0f;
        }

        // 获取学生基础生命尺度百分比，负责接入 HealthScale 计算。
        public static float GetBaseHealthPercent(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.healthPercent ?? 0f;
        }

        // 获取学生基础攻击力平加，负责接入最终攻击力计算。
        public static float GetBaseAttackFlat(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.attackFlat ?? 0f;
        }

        // 获取学生基础攻击力百分比，负责接入最终攻击力计算。
        public static float GetBaseAttackPercent(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.attackPercent ?? 0f;
        }

        // 获取学生基础治疗力平加，负责接入最终治疗力计算。
        public static float GetBaseHealFlat(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.healFlat ?? 0f;
        }

        // 获取学生基础治疗力百分比，负责接入最终治疗力计算。
        public static float GetBaseHealPercent(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.healPercent ?? 0f;
        }

        // 获取学生基础受回复倍率平加，负责接入受疗倍率计算。
        public static float GetBaseHealReceivedMultiplierOffset(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.healReceivedMultiplierOffset ?? 0f;
        }

        // 获取学生基础 EX 技能倍率平加，负责接入 EX 技能最终倍率。
        public static float GetBaseExSkillMultiplierOffset(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.exSkillMultiplierOffset ?? 0f;
        }

        public static float GetStarHealthFlat(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            return extension?.healthFlat?.Evaluate(GetCurrentRankLevel(pawn)) ?? 0f;
        }

        public static float GetStarHealthPercent(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            return extension?.healthPercent?.Evaluate(GetCurrentRankLevel(pawn)) ?? 0f;
        }

        public static float GetStarAttackFlat(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            return extension?.attackFlat?.Evaluate(GetCurrentRankLevel(pawn)) ?? 0f;
        }

        public static float GetStarAttackPercent(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            return extension?.attackPercent?.Evaluate(GetCurrentRankLevel(pawn)) ?? 0f;
        }

        public static float GetStarHealFlat(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            return extension?.healFlat?.Evaluate(GetCurrentRankLevel(pawn)) ?? 0f;
        }

        public static float GetStarHealPercent(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            return extension?.healPercent?.Evaluate(GetCurrentRankLevel(pawn)) ?? 0f;
        }

        public static BattleStarGrowthExtension GetStarGrowthExtension(Pawn pawn)
        {
            return pawn?.kindDef?.GetModExtension<BattleStarGrowthExtension>();
        }

        // 获取当前阶级，负责让战斗成长跟随角色信息面板的星星进度。
        public static int GetCurrentRankLevel(Pawn pawn)
        {
            return StudentRankUtility.GetCurrentRankLevel(pawn);
        }

        // 获取当前成长阶级，负责兼容旧调试入口的调用命名。
        public static int GetCurrentStarLevel(Pawn pawn)
        {
            return GetCurrentRankLevel(pawn);
        }

        public static float GetAdditionalBattleStatOffset(Pawn pawn, StatDef statDef)
        {
            if (pawn?.health?.hediffSet?.hediffs == null || statDef == null)
            {
                return 0f;
            }

            float total = 0f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                HediffComp_BattleStack comp = hediffs[i].TryGetComp<HediffComp_BattleStack>();
                if (comp != null && comp.AffectsStat(statDef))
                {
                    total += comp.GetCurrentValue(statDef);
                }
            }

            return total;
        }

        public static float GetAttackFlatBonus(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return Mathf.Max(0f,
                GetBaseAttackFlat(pawn) +
                GetStarAttackFlat(pawn));
        }

        // 获取基础攻击倍率，负责把基础攻击力加成按百分比转成 100% 起步的倍率。
        public static float GetAttackPowerBaseMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float bonus = pawn.GetStatValue(BattleStatDefOf.BANW_RangedWeapon_Damage) +
                          GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_RangedWeapon_Damage);
            return Mathf.Max(0f, 1f + bonus);
        }

        public static float GetAttackMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float bonus = GetBaseAttackPercent(pawn) +
                          pawn.GetStatValue(BattleStatDefOf.BANW_FinalDamageMultiplier) +
                          GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_FinalDamageMultiplier) +
                          GetStarAttackPercent(pawn);
            return Mathf.Max(0f, 1f + bonus);
        }

        public static float GetFinalAttackPower(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, GetAttackFlatBonus(pawn) * GetAttackPowerBaseMultiplier(pawn) * GetAttackMultiplier(pawn));
        }

        // 获取最终攻击倍率，负责统一组合基础攻击倍率和最终攻击力加成。
        public static float GetTotalAttackMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            return Mathf.Max(0f, GetAttackPowerBaseMultiplier(pawn) * GetAttackMultiplier(pawn));
        }

        public static float ScaleDamageBase(Pawn pawn, float baseAmount)
        {
            if (pawn == null)
            {
                return Mathf.Max(0f, baseAmount);
            }

            return Mathf.Max(0f, Mathf.Max(0f, baseAmount) * GetTotalAttackMultiplier(pawn));
        }

        // 缩放普通武器伤害，负责让武器弹丸按武器原始伤害、基础攻击倍率和最终攻击倍率结算。
        public static float ScaleWeaponDamageBase(Pawn pawn, float weaponBaseDamage)
        {
            if (pawn == null)
            {
                return Mathf.Max(0f, weaponBaseDamage);
            }

            return Mathf.Max(0f, Mathf.Max(0f, weaponBaseDamage) * GetTotalAttackMultiplier(pawn));
        }

        public static float GetHealFlatBonus(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return Mathf.Max(0f,
                GetBaseHealFlat(pawn) +
                pawn.GetStatValue(BattleStatDefOf.BANW_HealPowerBase) +
                GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_HealPowerBase) +
                GetStarHealFlat(pawn));
        }

        public static float GetHealMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float bonus = GetBaseHealPercent(pawn) +
                          pawn.GetStatValue(BattleStatDefOf.BANW_HealPowerMultiplier) +
                          GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_HealPowerMultiplier) +
                          GetStarHealPercent(pawn);
            return Mathf.Max(0f, 1f + bonus);
        }

        public static float GetFinalHealPower(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, GetHealFlatBonus(pawn) * GetHealMultiplier(pawn));
        }

        public static float ScaleHealBase(Pawn pawn, float baseAmount)
        {
            if (pawn == null)
            {
                return Mathf.Max(0f, baseAmount);
            }

            return Mathf.Max(0f, (Mathf.Max(0f, baseAmount) + GetHealFlatBonus(pawn)) * GetHealMultiplier(pawn));
        }

        public static float GetHealReceivedMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float multiplier = pawn.GetStatValue(BattleStatDefOf.BANW_HealReceivedMultiplier) +
                               GetBaseHealReceivedMultiplierOffset(pawn) +
                               GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_HealReceivedMultiplier);
            return Mathf.Max(0f, multiplier);
        }

        // 获取角色 EX 技能倍率，负责把属性和叠层状态加成合并为最终倍率。
        public static float GetExSkillMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float multiplier = pawn.GetStatValue(BattleStatDefOf.BANW_ExSkillMultiplier) +
                               GetBaseExSkillMultiplierOffset(pawn) +
                               GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_ExSkillMultiplier);
            return Mathf.Max(0f, multiplier);
        }

        // 获取本次动作的 EX 技能倍率，负责在普通动作和 EX 动作之间选择正确倍率。
        public static float GetExSkillMultiplier(Thing instigator, BattleCasterSnapshot snapshot, bool isExSkill)
        {
            if (!isExSkill)
            {
                return 1f;
            }

            if (snapshot != null)
            {
                return Mathf.Max(0f, snapshot.exSkillMultiplier);
            }

            return GetExSkillMultiplier(instigator as Pawn);
        }

        public static BattleCasterSnapshot CreateSnapshot(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            float attackFlatBonus = GetAttackFlatBonus(pawn);
            float attackPowerBase = GetAttackPowerBaseMultiplier(pawn);
            float attackMultiplier = GetAttackMultiplier(pawn);
            float healFlatBonus = GetHealFlatBonus(pawn);
            float healMultiplier = GetHealMultiplier(pawn);
            return new BattleCasterSnapshot
            {
                attackFlatBonus = attackFlatBonus,
                attackPowerBase = attackPowerBase,
                attackMultiplier = attackMultiplier,
                attackPower = attackFlatBonus * attackPowerBase * attackMultiplier,
                healFlatBonus = healFlatBonus,
                healMultiplier = healMultiplier,
                healPower = healFlatBonus * healMultiplier,
                criticalChance = pawn.GetStatValue(BattleStatDefOf.BANW_CriticalChance) + GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_CriticalChance),
                criticalDamage = pawn.GetStatValue(BattleStatDefOf.BANW_CriticalDamage) + GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_CriticalDamage),
                exSkillMultiplier = GetExSkillMultiplier(pawn),
                damageType = TryGetDamageType(pawn)
            };
        }

        public static BattleDamageResult BuildDamageResult(BattleDamageRequest request)
        {
            BattleDamageResult result = new BattleDamageResult();
            if (request == null)
            {
                return result;
            }

            Pawn casterPawn = request.instigator as Pawn;
            float amount = Mathf.Max(0f, request.baseAmount);
            if (request.snapshot != null)
            {
                amount = Mathf.Max(0f, request.baseAmount) * request.snapshot.attackPowerBase * request.snapshot.attackMultiplier;
                if (request.attackPowerRatio > 0f)
                {
                    amount += request.snapshot.attackPower * request.attackPowerRatio;
                }
            }
            else if (casterPawn != null)
            {
                amount = ScaleDamageBase(casterPawn, request.baseAmount);
                if (request.attackPowerRatio > 0f)
                {
                    amount += GetFinalAttackPower(casterPawn) * request.attackPowerRatio;
                }
            }

            float critMultiplier = 1f;
            result.isCrit = TryRollCrit(casterPawn, request.snapshot, request.canCrit, out critMultiplier);
            amount *= critMultiplier;

            if (request.applyAffinity)
            {
                result.affinityMultiplier = GetAffinityMultiplier(request.instigator, request.target, request.snapshot);
                amount *= result.affinityMultiplier;
            }

            result.exSkillMultiplier = GetExSkillMultiplier(request.instigator, request.snapshot, request.isExSkill);
            amount *= result.exSkillMultiplier;

            result.finalAmount = Mathf.Max(0f, amount);
            return result;
        }

        public static BattleHealResult BuildHealResult(BattleHealRequest request)
        {
            BattleHealResult result = new BattleHealResult();
            if (request == null || request.target == null)
            {
                return result;
            }

            Pawn casterPawn = request.instigator as Pawn;
            float amount = Mathf.Max(0f, request.baseAmount);
            if (request.snapshot != null)
            {
                amount = (Mathf.Max(0f, request.baseAmount) + request.snapshot.healFlatBonus) * request.snapshot.healMultiplier;
                if (request.healPowerRatio > 0f)
                {
                    amount += request.snapshot.healPower * request.healPowerRatio;
                }
            }
            else if (casterPawn != null)
            {
                amount = ScaleHealBase(casterPawn, request.baseAmount);
                if (request.healPowerRatio > 0f)
                {
                    amount += GetFinalHealPower(casterPawn) * request.healPowerRatio;
                }
            }

            float critMultiplier = 1f;
            result.isCrit = TryRollCrit(casterPawn, request.snapshot, request.canCrit, out critMultiplier);
            amount *= critMultiplier;
            amount *= GetHealReceivedMultiplier(request.target);
            result.exSkillMultiplier = GetExSkillMultiplier(request.instigator, request.snapshot, request.isExSkill);
            amount *= result.exSkillMultiplier;
            result.finalAmount = Mathf.Max(0f, amount);
            return result;
        }

        public static void ApplyDamage(BattleDamageRequest request)
        {
            if (request == null || request.target == null || request.damageDef == null)
            {
                return;
            }

            BattleDamageResult result = BuildDamageResult(request);
            bool instigatorGuilty = !(request.instigator is Pawn launcherPawn) || !launcherPawn.Drafted;
            BattleDamageDisplayState.RegisterManualDamage(request.target, request.instigator, result.isCrit);
            BattleDamageDisplayState.RegisterCriticalFloatText(request.target, result.isCrit);
            DamageInfo damageInfo = new DamageInfo(
                request.damageDef,
                result.finalAmount,
                request.penetration,
                -1f,
                request.instigator,
                null,
                null,
                DamageInfo.SourceCategory.ThingOrUnknown,
                request.target,
                instigatorGuilty);
            request.target.TakeDamage(damageInfo);
        }

        public static BattleHealResult ApplyHealing(BattleHealRequest request)
        {
            BattleHealResult result = BuildHealResult(request);
            if (request == null || request.target == null || result.finalAmount <= 0f)
            {
                return result;
            }

            float remaining = result.finalAmount;
            List<Hediff_Injury> injuries = GetHealableInjuries(request.target, request.allowPermanentInjuryHealing);
            for (int i = 0; i < injuries.Count && remaining > 0.01f; i++)
            {
                Hediff_Injury injury = injuries[i];
                float before = injury.Severity;
                injury.Heal(remaining);
                float healed = Mathf.Max(0f, before - injury.Severity);
                result.actualHealedAmount += healed;
                remaining -= healed;
            }

            if (result.actualHealedAmount > 0.01f)
            {
                CriticalObjPool.showHealShow(result.actualHealedAmount, request.target, result.isCrit);
            }

            return result;
        }

        public static void ApplyAction(Thing instigator, Thing target, BattleActionConfig action, BattleCasterSnapshot snapshot = null)
        {
            if (action == null || target == null)
            {
                return;
            }

            if (action.effecterDef != null)
            {
                Effecter effecter = action.effecterDef.Spawn();
                effecter.Trigger(new TargetInfo(target), TargetInfo.Invalid);
                effecter.Cleanup();
            }

            if (action.isHealing && target is Pawn pawnTarget)
            {
                ApplyHealing(new BattleHealRequest
                {
                    instigator = instigator,
                    target = pawnTarget,
                    baseAmount = action.baseAmount,
                    healPowerRatio = action.healPowerRatio,
                    canCrit = action.canCrit,
                    allowPermanentInjuryHealing = action.allowPermanentInjuryHealing,
                    isExSkill = action.isExSkill,
                    snapshot = snapshot
                });
            }
            else if (!action.isHealing && action.damageDef != null)
            {
                ApplyDamage(new BattleDamageRequest
                {
                    instigator = instigator,
                    target = target,
                    damageDef = action.damageDef,
                    baseAmount = action.baseAmount,
                    attackPowerRatio = action.attackPowerRatio,
                    penetration = action.penetration,
                    canCrit = action.canCrit,
                    applyAffinity = action.applyAffinity,
                    isExSkill = action.isExSkill,
                    snapshot = snapshot
                });
            }

            if (action.triggerHediff != null && target is Pawn hediffPawn)
            {
                if (action.triggerHediff.CompProps<HediffCompProperties_BattleStack>() != null)
                {
                    BattleStackHediffUtility.ApplyStackedHediff(hediffPawn, action.triggerHediff);
                }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(action.triggerHediff, hediffPawn);
                    hediffPawn.health.AddHediff(hediff);
                }
            }
        }

        public static bool ShouldAffectTarget(Pawn caster, Thing target, BattleActionConfig action)
        {
            if (action == null || target == null)
            {
                return false;
            }

            if (target is Building)
            {
                return action.canHitBuilding;
            }

            Pawn targetPawn = target as Pawn;
            if (targetPawn == null || targetPawn.Dead)
            {
                return false;
            }

            if (caster == null || caster.Faction == null || targetPawn.Faction == null)
            {
                return action.affectHostile;
            }

            if (targetPawn.HostileTo(caster))
            {
                return action.affectHostile;
            }

            return action.affectFriendly;
        }

        public static float GetAffinityMultiplier(Thing instigator, Thing target, BattleCasterSnapshot snapshot = null)
        {
            DamageAffinityMatrixDef matrix = GetAffinityMatrix();
            if (matrix == null)
            {
                return 1f;
            }

            damageType? attackType = snapshot?.damageType;
            if (!attackType.HasValue)
            {
                attackType = TryGetDamageType(instigator as Pawn);
            }

            damageType? defenseType = TryGetDefenseType(target as Pawn);
            if (!attackType.HasValue || !defenseType.HasValue)
            {
                return matrix.defaultMultiplier;
            }

            for (int i = 0; i < matrix.rows.Count; i++)
            {
                DamageAffinityRow row = matrix.rows[i];
                if (row.attackType != attackType.Value || row.entries == null)
                {
                    continue;
                }

                for (int j = 0; j < row.entries.Count; j++)
                {
                    DamageAffinityEntry entry = row.entries[j];
                    if (entry.defenseType == defenseType.Value)
                    {
                        float matrixMultiplier = Mathf.Max(0f, entry.multiplier);
                        if (matrixMultiplier <= 1f)
                        {
                            return matrixMultiplier;
                        }

                        return matrixMultiplier * (1f + GetAffinityBonus(instigator as Pawn, attackType.Value));
                    }
                }
            }

            return matrix.defaultMultiplier;
        }

        public static DamageAffinityMatrixDef GetAffinityMatrix()
        {
            if (cachedAffinityMatrix == null)
            {
                cachedAffinityMatrix = DefDatabase<DamageAffinityMatrixDef>.AllDefsListForReading.FirstOrDefault();
            }

            return cachedAffinityMatrix;
        }

        public static damageType? TryGetDamageType(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            // 读取 PawnKindDef 扩展中的攻击类型，属性克制不再从 TraitDef 或学生 UI 数据读取。
            return ParseConfiguredDamageType(pawn, GetBaseStatExtension(pawn)?.damageType, "damageType");
        }

        public static damageType? TryGetDefenseType(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            // 读取 PawnKindDef 扩展中的护甲类型，属性克制不再从 TraitDef 或学生 UI 数据读取。
            return ParseConfiguredDamageType(pawn, GetBaseStatExtension(pawn)?.defenseType, "defenseType");
        }

        // 解析配置中的属性类型，负责让 PawnKindDef 扩展成为唯一战斗属性类型来源。
        private static damageType? ParseConfiguredDamageType(Pawn pawn, string value, string fieldName)
        {
            if (value.NullOrEmpty())
            {
                return null;
            }

            damageType parsedValue;
            if (System.Enum.TryParse(value, out parsedValue))
            {
                return parsedValue;
            }

            Log.Error("[BANW] PawnKindDef " + pawn.kindDef?.defName + " 的 " + fieldName + " 配置无效：" + value);
            return null;
        }

        private static bool TryRollCrit(Pawn casterPawn, BattleCasterSnapshot snapshot, bool canCrit, out float critMultiplier)
        {
            critMultiplier = 1f;
            if (!canCrit)
            {
                return false;
            }

            float critChance = 0f;
            float critDamage = 2f;
            if (snapshot != null)
            {
                critChance = snapshot.criticalChance;
                critDamage = snapshot.criticalDamage;
            }
            else if (casterPawn != null)
            {
                critChance = casterPawn.GetStatValue(BattleStatDefOf.BANW_CriticalChance) + GetAdditionalBattleStatOffset(casterPawn, BattleStatDefOf.BANW_CriticalChance);
                critDamage = casterPawn.GetStatValue(BattleStatDefOf.BANW_CriticalDamage) + GetAdditionalBattleStatOffset(casterPawn, BattleStatDefOf.BANW_CriticalDamage);
            }

            bool isCrit = Rand.Value < Mathf.Clamp01(critChance);
            if (isCrit)
            {
                critMultiplier = Mathf.Max(1f, critDamage);
            }

            return isCrit;
        }

        // 获取属性克制额外加成，负责只在优势克制时叠加对应攻击类型的专属增幅。
        public static float GetAffinityBonus(Pawn pawn, damageType attackType)
        {
            StatDef statDef = GetAffinityBonusStatDef(attackType);
            if (pawn == null || statDef == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, pawn.GetStatValue(statDef) + GetAdditionalBattleStatOffset(pawn, statDef));
        }

        // 获取克制加成属性定义，负责把攻击类型映射到对应的 StatDef。
        public static StatDef GetAffinityBonusStatDef(damageType attackType)
        {
            switch (attackType)
            {
                case damageType.Explosion:
                    return BattleStatDefOf.BANW_AffinityBonus_Explosion;
                case damageType.Mysterious:
                    return BattleStatDefOf.BANW_AffinityBonus_Mysterious;
                case damageType.Vibration:
                    return BattleStatDefOf.BANW_AffinityBonus_Vibration;
                case damageType.Through:
                    return BattleStatDefOf.BANW_AffinityBonus_Through;
                case damageType.Composite:
                    return BattleStatDefOf.BANW_AffinityBonus_Composite;
                default:
                    return null;
            }
        }

        private static List<Hediff_Injury> GetHealableInjuries(Pawn pawn, bool allowPermanent)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(injury =>
                {
                    if (!allowPermanent)
                    {
                        return !injury.IsPermanent();
                    }

                    if (!injury.IsPermanent())
                    {
                        return true;
                    }

                    return injury.TryGetComp<HediffComp_GetsPermanent>() != null && (injury.Part == null || !pawn.health.hediffSet.PartIsMissing(injury.Part));
                })
                .OrderByDescending(injury => injury.Severity)
                .ToList();
        }
    }
}
