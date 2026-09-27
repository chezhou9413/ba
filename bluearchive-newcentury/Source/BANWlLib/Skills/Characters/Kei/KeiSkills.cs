using BANWlLib.BattleSystem;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //凯伊技能，负责友军场地、输出蓄积和场地结束后的单次释放。
    public static class KeiSkills
    {
        //生成友军增益场地并锁定记录上限。
        public static void Cast(Hediff_SpecialSkillState s, IntVec3 cell)
        {
            var source = s.profile.kei.FieldProperties;
            var field = (Thing_BattleFieldController)ThingMaker.MakeThing(source.fieldThingDef);
            GenSpawn.Spawn(field, cell, s.pawn.Map);
            field.Setup(s.pawn, source.durationTicksOverride);
            KeiFieldBinding.Begin(s, field);
        }

        //只统计当前增益区域内其他友军对敌人的实际有效伤害。
        public static void Record(Hediff_SpecialSkillState s, Pawn attacker, Thing target, float amount)
        {
            if (!s.Active || attacker == null || attacker == s.pawn || attacker.Faction != s.pawn.Faction ||
                !KeiFieldBinding.Contains(s, attacker) ||
                !target.HostileTo(attacker)) return;
            s.recorded = Mathf.Min(s.recordCap, s.recorded + amount * s.profile.recordRatio);
        }

        //场地结束后保留一次释放资格，有可行动角色和合法敌人时自动释放。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.endTick > 0 && !s.Active)
            {
                s.endTick = -1;
                s.field = null;
                s.releaseReady = true;
                SpecialEffects.Trigger(s.profile.endEffecter, s.pawn);
            }
            if (s.releaseReady && s.pawn.Spawned) SpecialSkillDispatcher.TryNormal(s);
        }

        //释放已确定的累计伤害，清空记录并允许下一次EX。
        public static void Release(Hediff_SpecialSkillState s, Pawn target)
        {
            float amount = Mathf.Min(s.recorded, s.recordCap);
            s.recorded = 0f;
            s.releaseReady = false;
            var config = s.profile.kei;
            config.castSound?.PlayOneShot(new TargetInfo(s.pawn));
            SpecialDirectionalEffects.Trigger(config.casterEffecter, s.pawn, target);
            SpecialEffects.Trigger(config.targetEffecter, target);
            SpecialCombatUtility.Schedule(s, target, s.profile.burstAttack, resolved: amount);
        }
    }
}
