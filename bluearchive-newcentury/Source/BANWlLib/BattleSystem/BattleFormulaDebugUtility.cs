using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 战斗公式调试工具，负责统一输出伤害与治疗的预估公式、最终结算和实际生效值。
    public static class BattleFormulaDebugUtility
    {
        private static int nextTraceId = 1;
        private static bool enabled;
        private static readonly Dictionary<int, Queue<PendingDamageLog>> pendingDamageLogsByTargetId = new Dictionary<int, Queue<PendingDamageLog>>();

        // 设置公式调试开关，负责让开发者按需开启或关闭详细日志。
        public static void SetEnabled(bool value)
        {
            enabled = value;
            Log.Message("[BANW公式] 调试日志已" + (enabled ? "开启" : "关闭") + "。");
        }

        // 获取公式调试开关状态，负责给调试入口显示当前状态。
        public static bool IsEnabled()
        {
            return enabled;
        }

        // 记录统一伤害预估，负责在真正受伤前输出完整公式并登记待完成日志。
        public static void LogDamagePreview(BattleDamageRequest request, BattleDamageResult result)
        {
            if (!enabled || request?.target == null || result == null)
            {
                return;
            }

            Pawn casterPawn = request.instigator as Pawn;
            Pawn targetPawn = request.target as Pawn;
            float weaponBaseAttack = ResolveWeaponBaseAttack(request, casterPawn);
            float attackPower = request.snapshot != null ? request.snapshot.attackPower : BattleStatUtility.GetFinalAttackPower(casterPawn, weaponBaseAttack);
            float attackMultiplier = request.snapshot != null ? request.snapshot.attackMultiplier : BattleStatUtility.GetAttackMultiplier(casterPawn);
            float actionMultiplier = request.isNormalAttack ? Mathf.Max(0f, request.normalAttackMultiplier) : Mathf.Max(0f, request.attackPowerRatio);
            float baseMasteryMultiplier = request.snapshot != null ? request.snapshot.baseMasteryMultiplier : BattleStatUtility.GetBaseMasteryMultiplier(casterPawn);
            float masteryMultiplier = request.isNormalAttack ? Mathf.Max(0f, request.baseMasteryMultiplier) * baseMasteryMultiplier : 1f;
            float critChance = request.canCrit ? ResolveCritChance(casterPawn, targetPawn, request.snapshot) : 0f;
            float critDamage = request.canCrit ? ResolveCritDamage(casterPawn, targetPawn, request.snapshot) : 1f;
            float critMultiplier = result.isCrit ? critDamage : 1f;
            int traceId = nextTraceId++;

            if (request.target is Pawn)
            {
                EnqueuePendingDamageLog(request.target, new PendingDamageLog
                {
                    traceId = traceId,
                    instigatorLabel = request.instigator?.LabelShortCap ?? "空",
                    targetLabel = request.target?.LabelShortCap ?? "空",
                    damageDefName = request.damageDef?.defName ?? "空",
                    finalAmount = result.finalAmount
                });
            }

            Log.Message("[BANW公式][伤害#" + traceId + "][预估] 施法者=" + (request.instigator?.LabelShortCap ?? "空")
                + "，目标=" + (request.target?.LabelShortCap ?? "空")
                + "，DamageDef=" + (request.damageDef?.defName ?? "空")
                + "，是否普通攻击=" + request.isNormalAttack
                + "，是否EX=" + request.isExSkill
                + "，是否强制暴击=" + request.alwaysCrit);

            if (request.snapshot != null)
            {
                Log.Message("[BANW公式][伤害#" + traceId + "][预估] 使用施法者快照：武器基础攻击="
                    + FormatNumber(weaponBaseAttack)
                    + "，快照攻击力=" + FormatNumber(attackPower)
                    + "，快照攻击力加成=" + FormatNumber(attackMultiplier)
                    + "，快照基础精通=" + FormatNumber(baseMasteryMultiplier));
            }
            else
            {
                Log.Message("[BANW公式][伤害#" + traceId + "][预估] 角色攻击力=("
                    + FormatNumber(weaponBaseAttack) + " x " + FormatNumber(BattleStatUtility.GetAttackLevelMultiplier(casterPawn))
                    + " x " + FormatNumber(BattleStatUtility.GetAttackStarMultiplier(casterPawn))
                    + ") + " + FormatNumber(BattleStatUtility.GetAttackFlatBonus(casterPawn))
                    + " = " + FormatNumber(attackPower)
                    + "；攻击力加成=" + FormatNumber(attackMultiplier)
                    + "；基础精通=" + FormatNumber(baseMasteryMultiplier));
            }

            Log.Message("[BANW公式][伤害#" + traceId + "][预估] 结算="
                + FormatNumber(attackPower)
                + " x 技能倍率" + FormatNumber(actionMultiplier)
                + " x 攻击力加成" + FormatNumber(attackMultiplier)
                + " x 熟练补正(" + FormatNumber(request.baseMasteryMultiplier) + " x " + FormatNumber(baseMasteryMultiplier) + ")=" + FormatNumber(masteryMultiplier)
                + " x 暴击" + FormatNumber(critMultiplier)
                + " x 克制" + FormatNumber(result.affinityMultiplier)
                + " x EX" + FormatNumber(result.exSkillMultiplier)
                + " = 送入伤害" + FormatNumber(result.finalAmount)
                + "；暴击率(扣抵抗后)=" + FormatPercent(critChance)
                + "；暴击伤害(扣抵抗后)=" + FormatNumber(critDamage));
        }

        // 记录统一伤害实际结果，负责在真正受伤后补上 DamageInfo 和最终生效值。
        public static void LogDamageActual(Thing target, DamageInfo dinfo, float actualDamage)
        {
            if (!enabled || target == null)
            {
                return;
            }

            PendingDamageLog pending = DequeuePendingDamageLog(target);
            if (pending == null)
            {
                return;
            }

            Log.Message("[BANW公式][伤害#" + pending.traceId + "][实际] 施法者=" + pending.instigatorLabel
                + "，目标=" + pending.targetLabel
                + "，DamageDef=" + pending.damageDefName
                + "，送入DamageInfo=" + FormatNumber(dinfo.Amount)
                + "，预估最终伤害=" + FormatNumber(pending.finalAmount)
                + "，实际生效伤害=" + FormatNumber(actualDamage));
        }

        // 记录统一治疗公式，负责输出治愈力、受回复率、最终治疗量和实际恢复量。
        public static void LogHealing(BattleHealRequest request, BattleHealResult result)
        {
            if (!enabled || request?.target == null || result == null)
            {
                return;
            }

            Pawn casterPawn = request.instigator as Pawn;
            float finalHealPower = request.snapshot != null ? request.snapshot.healPower : BattleStatUtility.GetFinalHealPower(casterPawn);
            float healReceivedMultiplier = BattleStatUtility.GetHealReceivedMultiplier(request.target);
            int traceId = nextTraceId++;

            Log.Message("[BANW公式][治疗#" + traceId + "] 施法者=" + (request.instigator?.LabelShortCap ?? "空")
                + "，目标=" + request.target.LabelShortCap
                + "，技能治疗倍率=" + FormatNumber(request.healPowerRatio)
                + "，允许治疗永久伤=" + request.allowPermanentInjuryHealing
                + "，是否EX=" + request.isExSkill);

            if (request.snapshot != null)
            {
                Log.Message("[BANW公式][治疗#" + traceId + "] 使用施法者快照：快照施法者=" + (request.snapshot.casterLabel ?? "空")
                    + "，快照最终治愈力=" + FormatNumber(finalHealPower)
                    + "，目标ID=" + request.target.thingIDNumber);
            }
            else
            {
                Log.Message("[BANW公式][治疗#" + traceId + "] 治愈力=((" 
                    + FormatNumber(BattleStatUtility.GetInitialHeal(casterPawn) + casterPawn.GetStatValue(BattleStatDefOf.BANW_InitialHeal))
                    + " x " + FormatNumber(BattleStatUtility.GetHealLevelMultiplier(casterPawn))
                    + " x " + FormatNumber(BattleStatUtility.GetHealStarMultiplier(casterPawn))
                    + ") + " + FormatNumber(BattleStatUtility.GetHealFlatBonus(casterPawn))
                    + ") x " + FormatNumber(BattleStatUtility.GetHealBonusMultiplier(casterPawn))
                    + " = " + FormatNumber(finalHealPower));
            }

            Log.Message("[BANW公式][治疗#" + traceId + "] 结算="
                + FormatNumber(finalHealPower)
                + " x 技能治疗倍率" + FormatNumber(request.healPowerRatio)
                + " x 受回复率" + FormatNumber(healReceivedMultiplier)
                + " = 最终治疗量" + FormatNumber(result.finalAmount)
                + "；实际恢复量=" + FormatNumber(result.actualHealedAmount));
        }

        // 记录受伤链细分过程，负责输出命中部位、护甲后伤害与承伤系数后的最终伤害。
        public static void LogDamagePipeline(Pawn target, DamageInfo dinfo, BodyPartRecord hitPart, float beforeArmor, float afterArmor, float incomingDamageFactor, float afterIncomingDamageFactor, bool deflectedByMetalArmor)
        {
            if (!enabled || target == null)
            {
                return;
            }

            Log.Message("[BANW公式][受伤链] 目标=" + target.LabelShortCap
                + "，部位=" + (hitPart?.LabelCap ?? hitPart?.def?.defName ?? "空")
                + "，护甲前=" + FormatNumber(beforeArmor)
                + "，护甲后=" + FormatNumber(afterArmor)
                + "，承伤系数=" + FormatNumber(incomingDamageFactor)
                + "，承伤后=" + FormatNumber(afterIncomingDamageFactor)
                + "，金属偏转=" + deflectedByMetalArmor
                + "，DamageDef=" + (dinfo.Def?.defName ?? "空"));
        }

        // 清理目标残留的伤害调试记录，负责防止异常中断后旧日志串到下一次伤害。
        public static void ClearPendingDamage(Thing target)
        {
            if (target == null)
            {
                return;
            }

            pendingDamageLogsByTargetId.Remove(target.thingIDNumber);
        }

        // 解析本次伤害使用的武器基础攻击，负责让调试输出与正式结算保持一致。
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

            return BattleStatUtility.GetWeaponBaseAttack(casterPawn);
        }

        // 解析最终暴击率，负责把目标暴击抵抗减算后的值输出到日志。
        private static float ResolveCritChance(Pawn casterPawn, Pawn targetPawn, BattleCasterSnapshot snapshot)
        {
            float critChance = snapshot != null ? snapshot.criticalChance : casterPawn?.GetStatValue(BattleStatDefOf.BANW_CriticalChance) ?? 0f;
            return Mathf.Clamp01(critChance - BattleStatUtility.GetCriticalChanceResistance(targetPawn));
        }

        // 解析最终暴击伤害，负责把目标暴击伤害抵抗减算后的值输出到日志。
        private static float ResolveCritDamage(Pawn casterPawn, Pawn targetPawn, BattleCasterSnapshot snapshot)
        {
            float critDamage = snapshot != null ? snapshot.criticalDamage : casterPawn?.GetStatValue(BattleStatDefOf.BANW_CriticalDamage) ?? 2f;
            return Mathf.Max(1f, critDamage - BattleStatUtility.GetCriticalDamageResistance(targetPawn));
        }

        // 缓存待完成伤害日志，负责把预估日志与实际伤害回调关联起来。
        private static void EnqueuePendingDamageLog(Thing target, PendingDamageLog pending)
        {
            if (target == null || pending == null)
            {
                return;
            }

            Queue<PendingDamageLog> queue;
            if (!pendingDamageLogsByTargetId.TryGetValue(target.thingIDNumber, out queue))
            {
                queue = new Queue<PendingDamageLog>();
                pendingDamageLogsByTargetId[target.thingIDNumber] = queue;
            }

            queue.Enqueue(pending);
        }

        // 取出待完成伤害日志，负责按伤害实际落地顺序补全日志。
        private static PendingDamageLog DequeuePendingDamageLog(Thing target)
        {
            if (target == null)
            {
                return null;
            }

            Queue<PendingDamageLog> queue;
            if (!pendingDamageLogsByTargetId.TryGetValue(target.thingIDNumber, out queue) || queue.Count == 0)
            {
                return null;
            }

            PendingDamageLog pending = queue.Dequeue();
            if (queue.Count == 0)
            {
                pendingDamageLogsByTargetId.Remove(target.thingIDNumber);
            }

            return pending;
        }

        // 格式化普通数值，负责让日志长度可读且保留关键小数位。
        private static string FormatNumber(float value)
        {
            return value.ToString("0.###");
        }

        // 格式化百分比，负责把暴击率这类概率输出成人类可读文本。
        private static string FormatPercent(float value)
        {
            return value.ToString("P1");
        }

        // 待完成伤害日志，负责把预估阶段的核心数据带到实际伤害回调。
        private class PendingDamageLog
        {
            public int traceId;
            public string instigatorLabel;
            public string targetLabel;
            public string damageDefName;
            public float finalAmount;
        }
    }
}
