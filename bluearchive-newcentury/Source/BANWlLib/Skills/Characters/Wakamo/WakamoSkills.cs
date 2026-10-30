using BANWlLib.BattleSystem;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //若藻技能，单体连射、伤害附加敌方标记和BA蓄积爆发。
    public static class WakamoSkills
    {
        //各段按独立参数发射，最后一发命中结算后才建立蓄积标记。
        public static void Cast(Hediff_SpecialSkillState s, Pawn target)
        {
            Clear(s);
            int lastShot = 0;
            WakamoAttackStage finalStage = null;
            foreach (WakamoAttackStage stage in s.profile.wakamo.exStages)
            {
                int end = stage.delayTicks + stage.attack.ShotDelay(stage.attack.shots - 1);
                if (end >= lastShot) { lastShot = end; finalStage = stage; }
            }
            foreach (WakamoAttackStage stage in s.profile.wakamo.exStages)
                SpecialCombatUtility.Schedule(s, target, stage.attack, startRecord: stage == finalStage,
                    extraDelay: stage.delayTicks, castingJob: s.pawn.CurJob, retainTargetPosition: true);
            s.castEndTick = s.Now + lastShot;
        }

        //末发有效伤害后附加状态，只累计标记建立之后的伤害。
        public static void OnExDamage(SpecialPendingAttack attack, Pawn target, float dealt)
        {
            var s = attack.state;
            if (s == null || attack.recordCastId != s.wakamo.castId || s.wakamo.mark != null ||
                s.pawn.Dead || s.pawn.Map != target.Map) return;
            BattleCasterSnapshot snapshot = attack.snapshot;
            var result = WakamoBurstCalculator.CalculateUnit(s, target, snapshot);
            var mark = (Hediff_WakamoMark)HediffMaker.MakeHediff(s.profile.buffHediff, target);
            mark.owner = s;
            //蓄积基数取实际建立标记的伤害段，在标记建立后保持不变。
            mark.rawCap = (attack.action.useBattleStats ? snapshot.attackPower : attack.action.basePower) * s.profile.recordCapRatio;
            mark.damageFactor = result.finalAmount;
            mark.critical = result.isCrit;
            s.recordTarget = target;
            s.snapshot = snapshot;
            s.recorded = 0f;
            s.recordCap = mark.rawCap * mark.damageFactor;
            s.endTick = s.Now + s.profile.durationTicks;
            s.wakamo.mark = mark;
            target.health.AddHediff(mark);
        }

        //累计同阵营单位对被标记目标造成的实际伤害。
        public static void Record(Hediff_SpecialSkillState s, Pawn attacker, Thing target, float amount)
        {
            if (!s.Active || target != s.recordTarget || attacker?.Faction != s.pawn.Faction || s.wakamo.mark == null) return;
            s.wakamo.mark.AddDamage(amount);
        }

        //目标失效时取消记录，到期时先关闭记录再安排已确定金额的爆发。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.recordTarget == null) return;
            if (s.recordTarget.Dead || !s.recordTarget.Spawned || s.recordTarget.Map != s.pawn.Map)
            {
                Clear(s);
                return;
            }
            if (s.Active) return;
            Pawn target = s.recordTarget;
            float amount = Mathf.Min(s.recorded, s.recordCap);
            bool critical = s.wakamo.mark.critical;
            Clear(s);
            SpecialCombatUtility.Schedule(s, target, s.profile.burstAttack, resolved: amount, resolvedCritical: critical);
            SpecialEffects.Trigger(s.profile.endEffecter, target);
        }

        //撤销当前标记并废弃旧批次，旧弹丸仍能伤害但不能重建已取消的记录。
        public static void Clear(Hediff_SpecialSkillState s)
        {
            var mark = s.wakamo.mark;
            s.wakamo.mark = null;
            s.wakamo.castId++;
            s.recordTarget = null;
            s.recorded = s.recordCap = 0f;
            s.endTick = -1;
            s.snapshot = null;
            if (mark != null) mark.pawn.health.RemoveHediff(mark);
        }
    }
}
