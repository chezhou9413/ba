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

        // 获取学生初始生命值，负责从 PawnKindDef 读取生命值公式的基础乘算项。
        public static float GetInitialHealth(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.initialHealth ?? 0f;
        }

        // 获取学生基础攻击力百分比，负责接入最终攻击力计算。
        public static float GetBaseAttackPercent(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.attackPercent ?? 0f;
        }

        // 获取学生基础攻击力平加，负责接入最终攻击力计算。
        public static float GetBaseAttackFlat(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.attackFlat ?? 0f;
        }

        // 获取学生初始治愈力，负责从 PawnKindDef 读取治愈力公式的基础乘算项。
        public static float GetInitialHeal(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.initialHeal ?? 0f;
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

        // 获取学生基础精通倍率平加，负责接入普通攻击口径伤害的基础精通属性。
        public static float GetBaseMasteryMultiplierOffset(Pawn pawn)
        {
            return GetBaseStatExtension(pawn)?.baseMasteryMultiplierOffset ?? 0f;
        }

        // 获取当前阶级，负责兼容外部调用。
        public static int GetCurrentRankLevel(Pawn pawn)
        {
            return StudentRankUtility.GetCurrentRankLevel(pawn);
        }

        // 获取当前成长阶级，负责兼容旧调试入口的调用命名。
        public static int GetCurrentStarLevel(Pawn pawn)
        {
            return GetCurrentRankLevel(pawn);
        }

        // 获取阶级成长扩展，负责从 PawnKindDef 读取当前角色的成长配置。
        public static BattleStarGrowthExtension GetStarGrowthExtension(Pawn pawn)
        {
            return pawn?.kindDef?.GetModExtension<BattleStarGrowthExtension>();
        }

        // 获取当前阶级治愈力百分比成长，负责让阶级成长进入治愈力加成。
        public static float GetRankHealPercent(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            if (extension == null)
            {
                return 0f;
            }

            return extension.healPercent.Evaluate(GetCurrentRankLevel(pawn));
        }

        // 获取当前阶级基础攻击力成长，负责让阶级成长进入最终攻击力。
        public static float GetRankAttackFlat(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            if (extension == null)
            {
                return 0f;
            }

            return extension.attackFlat.Evaluate(GetCurrentRankLevel(pawn));
        }

        // 获取当前阶级攻击力百分比成长，负责让阶级成长进入攻击力加成。
        public static float GetRankAttackPercent(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            if (extension == null)
            {
                return 0f;
            }

            return extension.attackPercent.Evaluate(GetCurrentRankLevel(pawn));
        }

        // 获取当前阶级生命值百分比成长，负责让星级成长进入升星生命值倍率。
        public static float GetRankHealthPercent(Pawn pawn)
        {
            BattleStarGrowthExtension extension = GetStarGrowthExtension(pawn);
            if (extension == null)
            {
                return 0f;
            }

            return extension.healthPercent.Evaluate(GetCurrentRankLevel(pawn));
        }

        // 获取 PawnKind 基础战斗属性对指定 Stat 的额外加成，负责让 PawnKind 固有属性注入原版 StatWorker 显示。
        public static float GetBaseStatOffset(Pawn pawn, StatDef statDef)
        {
            if (pawn == null || statDef == null)
            {
                return 0f;
            }

            BattleBaseStatExtension ext = GetBaseStatExtension(pawn);
            if (ext == null)
            {
                return 0f;
            }

            if (statDef == BattleStatDefOf.BANW_FinalDamageMultiplier)
            {
                return ext.attackPercent;
            }

            if (statDef == BattleStatDefOf.BANW_HealReceivedMultiplier)
            {
                return ext.healReceivedMultiplierOffset;
            }

            if (statDef == BattleStatDefOf.BANW_ExSkillMultiplier)
            {
                return ext.exSkillMultiplierOffset;
            }

            if (statDef == BattleStatDefOf.BANW_BaseMasteryMultiplier)
            {
                return ext.baseMasteryMultiplierOffset;
            }

            return 0f;
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

        // 获取攻击力加成倍率，负责把装备、状态和阶级成长合并为最终伤害乘区。
        public static float GetAttackMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float bonus = pawn.GetStatValue(BattleStatDefOf.BANW_FinalDamageMultiplier) +
                          GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_FinalDamageMultiplier);
            return Mathf.Max(0f, 1f + bonus);
        }

        // 获取基础精通倍率，负责把角色、装备和状态上的基础精通属性接入普通攻击口径伤害。
        public static float GetBaseMasteryMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float multiplier = pawn.GetStatValue(BattleStatDefOf.BANW_BaseMasteryMultiplier);
            return Mathf.Max(0f, multiplier);
        }

        // 获取升级攻击力倍率，负责把等级状态里的攻击成长转为角色自身攻击力乘区。
        public static float GetAttackLevelMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float bonus = pawn.GetStatValue(BattleStatDefOf.BANW_AttackLevelMultiplier) +
                          GetAdditionalBattleStatOffset(pawn, BattleStatDefOf.BANW_AttackLevelMultiplier);
            return Mathf.Max(0f, 1f + bonus);
        }

        // 获取升星攻击力倍率，负责让 PawnKind 和阶级成长进入武器初始攻击力乘区。
        public static float GetAttackStarMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float bonus = GetBaseAttackPercent(pawn) + GetRankAttackPercent(pawn);
            return Mathf.Max(0f, 1f + bonus);
        }

        // 获取固定攻击力，负责把学生基础值、状态平加和阶级成长合并为角色自身攻击力加算项。
        public static float GetAttackFlatBonus(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            float attackFlat = GetBaseAttackFlat(pawn) +
                               GetRankAttackFlat(pawn);
            return Mathf.Max(0f, attackFlat);
        }

        // 获取武器初始攻击力，负责读取当前主武器默认子弹的原始伤害。
        public static float GetWeaponBaseAttack(Pawn pawn)
        {
            ThingWithComps primary = pawn?.equipment?.Primary;
            ThingDef projectileDef = GetPrimaryProjectileDef(pawn);
            if (projectileDef?.projectile == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, projectileDef.projectile.GetDamageAmount(primary));
        }

        // 获取角色自身攻击力，负责按武器初始攻击力、升级倍率、升星倍率和固定攻击力结算。
        public static float GetFinalAttackPower(Pawn pawn)
        {
            return GetFinalAttackPower(pawn, GetWeaponBaseAttack(pawn));
        }

        // 获取角色自身攻击力，负责按指定武器初始攻击力结算普通攻击和技能伤害基数。
        public static float GetFinalAttackPower(Pawn pawn, float weaponBaseAttack)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, Mathf.Max(0f, weaponBaseAttack) * GetAttackLevelMultiplier(pawn) * GetAttackStarMultiplier(pawn) + GetAttackFlatBonus(pawn));
        }

        // 获取主武器默认投射物，负责为技能和普通攻击提供统一武器攻击力来源。
        private static ThingDef GetPrimaryProjectileDef(Pawn pawn)
        {
            ThingWithComps primary = pawn?.equipment?.Primary;
            if (primary?.def?.Verbs == null)
            {
                return null;
            }

            for (int i = 0; i < primary.def.Verbs.Count; i++)
            {
                ThingDef projectileDef = primary.def.Verbs[i]?.defaultProjectile;
                if (projectileDef?.projectile != null)
                {
                    return projectileDef;
                }
            }

            return null;
        }

        // 缩放普通武器伤害，负责让武器弹丸按角色自身攻击力和攻击力加成显示。
        public static float ScaleWeaponDamageBase(Pawn pawn, float weaponBaseDamage)
        {
            if (pawn == null)
            {
                return Mathf.Max(0f, weaponBaseDamage);
            }

            return Mathf.Max(0f, GetFinalAttackPower(pawn, weaponBaseDamage) * GetAttackMultiplier(pawn));
        }

        // 获取升级治愈力倍率，负责把等级状态和叠层状态转为治愈力升级乘区。
        public static float GetHealLevelMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float bonus = pawn.GetStatValue(BattleStatDefOf.BANW_HealLevelMultiplier);
            return Mathf.Max(0f, 1f + bonus);
        }

        // 获取升星治愈力倍率，负责让阶级成长进入治愈力升星乘区。
        public static float GetHealStarMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            return Mathf.Max(0f, 1f + GetRankHealPercent(pawn));
        }

        // 获取固定治愈力加算，负责把装备、状态和阶级固定成长合并到乘算后的固定项。
        public static float GetHealFlatBonus(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            float flatBonus = pawn.GetStatValue(BattleStatDefOf.BANW_HealFlatBonus);
            return Mathf.Max(0f, flatBonus);
        }

        // 获取治愈力加成，负责把 PawnKind、装备、状态和叠层加成合并为最终乘区。
        public static float GetHealBonusMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float bonus = pawn.GetStatValue(BattleStatDefOf.BANW_HealBonusMultiplier);
            return Mathf.Max(0f, 1f + bonus);
        }

        // 获取最终治愈力，负责按基础固定治愈力、升级倍率、升星倍率、固定加算和最终加成结算。
        public static float GetFinalHealPower(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            float baseHeal = GetInitialHeal(pawn) + pawn.GetStatValue(BattleStatDefOf.BANW_InitialHeal);
            float levelMultiplier = GetHealLevelMultiplier(pawn);
            float starMultiplier = GetHealStarMultiplier(pawn);
            float flatBonus = GetHealFlatBonus(pawn);
            float bonusMultiplier = GetHealBonusMultiplier(pawn);
            return Mathf.Max(0f, (baseHeal * levelMultiplier * starMultiplier + flatBonus) * bonusMultiplier);
        }

        // 获取受回复率，负责在治疗量完成后按目标状态和装备计算最终受疗倍率。
        public static float GetHealReceivedMultiplier(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }

            float multiplier = pawn.GetStatValue(BattleStatDefOf.BANW_HealReceivedMultiplier);
            return Mathf.Max(0f, multiplier);
        }

        // 获取暴击抵抗率，负责从目标属性和叠层状态读取暴击率减算值。
        public static float GetCriticalChanceResistance(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, pawn.GetStatValue(BattleStatDefOf.BANW_CriticalChanceResistance));
        }

        // 获取暴击伤害抵抗率，负责从目标属性和叠层状态读取暴击伤害减算值。
        public static float GetCriticalDamageResistance(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, pawn.GetStatValue(BattleStatDefOf.BANW_CriticalDamageResistance));
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

            float attackMultiplier = GetAttackMultiplier(pawn);
            float healMultiplier = GetHealBonusMultiplier(pawn);
            float weaponBaseAttack = GetWeaponBaseAttack(pawn);
            return new BattleCasterSnapshot
            {
                casterLabel = pawn.LabelShortCap,
                attackLevelMultiplier = GetAttackLevelMultiplier(pawn),
                attackMultiplier = attackMultiplier,
                baseMasteryMultiplier = GetBaseMasteryMultiplier(pawn),
                weaponBaseAttack = weaponBaseAttack,
                attackPower = GetFinalAttackPower(pawn, weaponBaseAttack),
                healMultiplier = healMultiplier,
                healPower = GetFinalHealPower(pawn),
                criticalChance = pawn.GetStatValue(BattleStatDefOf.BANW_CriticalChance),
                criticalDamage = pawn.GetStatValue(BattleStatDefOf.BANW_CriticalDamage),
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
            float weaponBaseAttack = ResolveWeaponBaseAttack(request, casterPawn);
            float attackPower = request.snapshot != null ? request.snapshot.attackPower : GetFinalAttackPower(casterPawn, weaponBaseAttack);
            float attackMultiplier = request.snapshot != null ? request.snapshot.attackMultiplier : GetAttackMultiplier(casterPawn);
            float actionMultiplier = request.isNormalAttack ? Mathf.Max(0f, request.normalAttackMultiplier) : Mathf.Max(0f, request.attackPowerRatio);
            float baseMasteryMultiplier = request.snapshot != null ? request.snapshot.baseMasteryMultiplier : GetBaseMasteryMultiplier(casterPawn);
            float masteryMultiplier = request.isNormalAttack ? Mathf.Max(0f, request.baseMasteryMultiplier) * baseMasteryMultiplier : 1f;
            float amount = attackPower * actionMultiplier * attackMultiplier * masteryMultiplier;

            float critMultiplier = 1f;
            bool canCrit = request.canCrit && !DamageFontRuleUtility.IsCriticalDisabled(request.damageDef);
            bool alwaysCrit = request.alwaysCrit || DamageFontRuleUtility.IsCriticalEnsured(request.damageDef);
            result.isCrit = TryRollCrit(casterPawn, request.target as Pawn, request.snapshot, canCrit, alwaysCrit, out critMultiplier);
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

        // 解析本次攻击的武器初始攻击力，负责优先使用投射物伤害，其次使用施法者主武器子弹。
        private static float ResolveWeaponBaseAttack(BattleDamageRequest request, Pawn casterPawn)
        {
            if (request == null)
            {
                return 0f;
            }

            if (request.weaponBaseAttack > 0f)
            {
                return request.weaponBaseAttack;
            }

            return GetWeaponBaseAttack(casterPawn);
        }

        public static BattleHealResult BuildHealResult(BattleHealRequest request)
        {
            BattleHealResult result = new BattleHealResult();
            if (request == null || request.target == null)
            {
                return result;
            }

            Pawn casterPawn = request.instigator as Pawn;
            float amount = 0f;
            if (request.snapshot != null)
            {
                if (request.healPowerRatio > 0f)
                {
                    amount = request.snapshot.healPower * request.healPowerRatio;
                }
            }
            else if (casterPawn != null)
            {
                if (request.healPowerRatio > 0f)
                {
                    amount = GetFinalHealPower(casterPawn) * request.healPowerRatio;
                }
            }

            result.isCrit = false;
            amount *= GetHealReceivedMultiplier(request.target);
            result.exSkillMultiplier = 1f;
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
            BattleFormulaDebugUtility.LogDamagePreview(request, result);
            bool instigatorGuilty = !(request.instigator is Pawn launcherPawn) || !launcherPawn.Drafted;
            BattleDamageDisplayState.RegisterManualDamage(request.target, request.instigator, result.isCrit);
            BattleDamageDisplayState.RegisterCriticalFloatText(request.target, result.isCrit || request.alwaysShowCriticalText);
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
            else if (request.alwaysShowHealText && result.finalAmount > 0.01f)
            {
                CriticalObjPool.showHealShow(result.finalAmount, request.target, result.isCrit);
            }

            BattleFormulaDebugUtility.LogHealing(request, result);
            return result;
        }

        // 结算护盾结果，负责按施法者最终治愈力和护盾倍率得到护盾值。
        public static BattleHealResult BuildShieldResult(BattleShieldRequest request)
        {
            BattleHealResult result = new BattleHealResult();
            if (request == null || request.target == null)
            {
                return result;
            }

            float healPower = request.snapshot != null ? request.snapshot.healPower : GetFinalHealPower(request.instigator as Pawn);
            result.finalAmount = Mathf.Max(0f, healPower * Mathf.Max(0f, request.shieldPowerRatio));
            result.isCrit = false;
            result.exSkillMultiplier = 1f;
            return result;
        }

        // 应用护盾，负责创建或叠加目标身上的战斗护盾 Hediff。
        public static BattleHealResult ApplyShield(BattleShieldRequest request)
        {
            BattleHealResult result = BuildShieldResult(request);
            if (request == null || request.target == null || result.finalAmount <= 0f)
            {
                return result;
            }

            if (request.shieldHediffDef == null)
            {
                Log.Error("[BANW] 护盾动作缺少 shieldHediffDef，无法应用护盾。");
                return result;
            }

            Hediff shieldHediff = request.target.health.hediffSet.GetFirstHediffOfDef(request.shieldHediffDef);
            if (shieldHediff == null)
            {
                shieldHediff = HediffMaker.MakeHediff(request.shieldHediffDef, request.target);
                request.target.health.AddHediff(shieldHediff);
            }

            HediffComp_BattleShield shieldComp = shieldHediff.TryGetComp<HediffComp_BattleShield>();
            if (shieldComp == null)
            {
                Log.Error("[BANW] 护盾 Hediff " + request.shieldHediffDef.defName + " 缺少 HediffComp_BattleShield。");
                return result;
            }

            shieldComp.AddShield(result.finalAmount);
            HediffComp_Disappears disappears = shieldHediff.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
            {
                disappears.ResetElapsedTicks();
            }

            CriticalObjPool.showHealShow(result.finalAmount, request.target, false);
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

            if (action.damageDef != null)
            {
                ApplyDamage(new BattleDamageRequest
                {
                    instigator = instigator,
                    target = target,
                    damageDef = action.damageDef,
                    weaponBaseAttack = action.weaponBaseAttack,
                    attackPowerRatio = action.attackPowerRatio,
                    normalAttackMultiplier = action.normalAttackMultiplier,
                    baseMasteryMultiplier = action.baseMasteryMultiplier,
                    penetration = action.penetration,
                    isNormalAttack = action.isNormalAttack,
                    canCrit = action.canCrit,
                    alwaysCrit = action.alwaysCrit,
                    alwaysShowCriticalText = action.alwaysShowCriticalText,
                    applyAffinity = action.applyAffinity,
                    isExSkill = action.isExSkill,
                    snapshot = snapshot
                });
            }
            else if (action.isShield && target is Pawn shieldTarget)
            {
                ApplyShield(new BattleShieldRequest
                {
                    instigator = instigator,
                    target = shieldTarget,
                    shieldPowerRatio = action.shieldPowerRatio,
                    shieldHediffDef = action.shieldHediffDef,
                    snapshot = snapshot
                });
            }
            else if (action.isHealing && target is Pawn pawnTarget)
            {
                ApplyHealing(new BattleHealRequest
                {
                    instigator = instigator,
                    target = pawnTarget,
                    healPowerRatio = action.healPowerRatio,
                    canCrit = action.canCrit,
                    alwaysShowHealText = action.alwaysShowHealText,
                    allowPermanentInjuryHealing = action.allowPermanentInjuryHealing,
                    isExSkill = action.isExSkill,
                    snapshot = snapshot
                });
            }
            if (action.triggerHediff != null && target is Pawn hediffPawn)
            {
                BattleHediffSnapshotUtility.RegisterSnapshotIfNeeded(hediffPawn, action.triggerHediff, snapshot);
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

            bool isDamageAction = action.damageDef != null;
            if (target is Building building)
            {
                if (!action.canHitBuilding)
                {
                    return false;
                }

                if (isDamageAction && IsOwnFaction(caster, building) && !action.canHitOwnBuilding)
                {
                    return false;
                }

                return true;
            }

            Pawn targetPawn = target as Pawn;
            if (targetPawn == null || targetPawn.Dead)
            {
                return false;
            }

            if (isDamageAction && IsOwnFaction(caster, targetPawn) && !action.canHitOwnPawn)
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

        //判断目标是否属于施法者阵营，负责阻止伤害动作误伤己方建筑和己方 Pawn。
        private static bool IsOwnFaction(Pawn caster, Thing target)
        {
            return caster?.Faction != null && target?.Faction != null && target.Faction == caster.Faction;
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

        // 判定暴击，负责把目标暴击抵抗和强制暴击标记纳入最终暴击乘区。
        private static bool TryRollCrit(Pawn casterPawn, Pawn targetPawn, BattleCasterSnapshot snapshot, bool canCrit, bool alwaysCrit, out float critMultiplier)
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
                critChance = casterPawn.GetStatValue(BattleStatDefOf.BANW_CriticalChance);
                critDamage = casterPawn.GetStatValue(BattleStatDefOf.BANW_CriticalDamage);
            }

            critChance = Mathf.Clamp01(critChance - GetCriticalChanceResistance(targetPawn));
            critDamage = Mathf.Max(1f, critDamage - GetCriticalDamageResistance(targetPawn));

            bool isCrit = alwaysCrit || Rand.Value < critChance;
            if (isCrit)
            {
                critMultiplier = critDamage;
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
