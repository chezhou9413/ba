using BANWlLib.BattleSystem;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //若藻技能，负责单体连射、伤害附加敌方标记和BA蓄积爆发。
    public static class WakamoSkills
    {
        //开始独立施法批次，任一子弹首次造成实际伤害时建立标记。
        public static void Cast(Hediff_SpecialSkillState s, Pawn target)
        {
            Clear(s);
            SpecialCombatUtility.Schedule(s, target, s.profile.exAttack, startRecord: true);
        }

        //首发有效伤害后附加状态并计入首发金额，后续伤害由公共事件统一累计。
        public static void OnExDamage(SpecialPendingAttack attack, Pawn target, float dealt)
        {
            var s = attack.state;
            if (s == null || attack.recordCastId != s.wakamo.castId || s.wakamo.mark != null ||
                s.pawn.Dead || s.pawn.Map != target.Map) return;
            BattleCasterSnapshot snapshot = attack.snapshot;
            var result = WakamoBurstCalculator.CalculateUnit(s, target, snapshot);
            var mark = (Hediff_WakamoMark)HediffMaker.MakeHediff(s.profile.buffHediff, target);
            mark.owner = s;
            mark.rawCap = (s.profile.exAttack.useBattleStats ? snapshot.attackPower : s.profile.exAttack.basePower) * s.profile.recordCapRatio;
            mark.damageFactor = result.finalAmount;
            mark.critical = result.isCrit;
            s.recordTarget = target;
            s.snapshot = snapshot;
            s.recorded = 0f;
            s.recordCap = mark.rawCap * mark.damageFactor;
            s.endTick = s.Now + s.profile.durationTicks;
            s.wakamo.mark = mark;
            target.health.AddHediff(mark);
            //首次伤害的公共通知发生在状态创建前，因此只在这里补计这一次。
            mark.AddDamage(dealt);
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
