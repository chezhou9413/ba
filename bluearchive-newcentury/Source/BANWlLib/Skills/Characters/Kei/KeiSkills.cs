using BANWlLib.BattleSystem;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //凯伊技能，每个友军场地独立蓄积，到期后逐次释放普通技能。
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
            if (attacker == null || attacker == s.pawn || attacker.Faction != s.pawn.Faction ||
                !target.HostileTo(attacker)) return;
            var runtime = KeiRuntimeState.Get(s);
            foreach (var charge in runtime.charges)
                if (!charge.ready && charge.endTick > s.Now && KeiFieldBinding.Contains(s, charge.field, attacker))
                    charge.recorded = Mathf.Min(charge.recordCap, charge.recorded + amount * s.profile.recordRatio);
            runtime.Sync(s);
        }

        //每个场地各自到期；没有合法敌人时保留全部机会。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            var runtime = KeiRuntimeState.Get(s);
            int completed = runtime.Advance(s.Now);
            runtime.Sync(s);
            for (int i = 0; i < completed; i++)
                SpecialEffects.Trigger(s.profile.endEffecter, s.pawn);
            if (s.releaseReady && s.pawn.Spawned) SpecialSkillDispatcher.TryNormal(s);
        }

        //同次施法分别发射攻击力伤害与蓄积伤害，消耗一次释放机会。
        public static void Release(Hediff_SpecialSkillState s, Pawn target)
        {
            var runtime = KeiRuntimeState.Get(s);
            if (!runtime.TryConsume(out float amount)) return;
            runtime.Sync(s);
            var config = s.profile.kei;
            config.castSound?.PlayOneShot(new TargetInfo(s.pawn));
            SpecialDirectionalEffects.Trigger(config.casterEffecter, s.pawn, target);
            SpecialEffects.Trigger(config.targetEffecter, target);
            SpecialCombatUtility.Schedule(s, target, s.profile.normalAttack);
            SpecialCombatUtility.Schedule(s, target, s.profile.burstAttack,
                resolved: amount * s.profile.burstAttack.attackPowerRatio);
        }
    }
}
