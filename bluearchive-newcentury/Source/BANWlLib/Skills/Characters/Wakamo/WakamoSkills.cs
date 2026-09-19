using BANWlLib.BattleSystem;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //若藻技能，负责初始攻击、施法者独立伤害记录和到期爆发。
    public static class WakamoSkills
    {
        //重置本施法者旧记录，首段命中结算完毕后才开始新记录。
        public static void Cast(Hediff_SpecialSkillState s, Pawn target)
        {
            s.recordTarget = null;
            s.recorded = 0f;
            s.endTick = -1;
            SpecialCombatUtility.Schedule(s, target, s.profile.exAttack, startRecord: true);
        }

        //从首段的属性快照生成上限，首段伤害本身不计入蓄积。
        public static void BeginRecord(Hediff_SpecialSkillState s, Pawn target, BattleCasterSnapshot snapshot)
        {
            s.recordTarget = target;
            s.snapshot = snapshot;
            s.recorded = 0f;
            s.recordCap = (s.profile.exAttack.useBattleStats ? snapshot.attackPower : s.profile.exAttack.basePower) * s.profile.recordCapRatio;
            s.endTick = s.Now + s.profile.durationTicks;
        }

        //累计同阵营单位对被标记目标造成的实际伤害。
        public static void Record(Hediff_SpecialSkillState s, Pawn attacker, Thing target, float amount)
        {
            if (!s.Active || target != s.recordTarget || attacker?.Faction != s.pawn.Faction) return;
            s.recorded = Mathf.Min(s.recordCap, s.recorded + amount * s.profile.recordRatio);
        }

        //目标失效时取消记录，到期时先关闭记录再安排已确定金额的爆发。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.recordTarget == null) return;
            if (s.recordTarget.Dead || !s.recordTarget.Spawned || s.recordTarget.Map != s.pawn.Map)
            {
                s.recordTarget = null;
                s.recorded = 0;
                s.endTick = -1;
                return;
            }
            if (s.Active) return;
            Pawn target = s.recordTarget;
            float amount = Mathf.Min(s.recorded, s.recordCap);
            s.recordTarget = null;
            s.endTick = -1;
            s.recorded = 0;
            SpecialCombatUtility.Schedule(s, target, s.profile.burstAttack, resolved: amount);
            SpecialEffects.Trigger(s.profile.endEffecter, target);
        }
    }
}

