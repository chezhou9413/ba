using System;
using System.Linq;
using BANWlLib.BattleSystem;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //技能公共事件入口，负责发布施法成功和已结算伤害并通知角色机制。
    public static class SpecialSkillEvents
    {
        public static event Action<Ability> AbilityActivated;
        public static event Action<SpecialDamageEvent> DamageApplied;
        public static SpecialDamageEvent CurrentDamage;
        private static int nextId;

        //在原版伤害之前取得完整来源，给致命伤与普攻计数提供一致的事件标识。
        public static SpecialDamageEvent Begin(Thing target, ref DamageInfo damage)
        {
            BattleDamageRequest request = SpecialDamageScope.Current;
            var e = new SpecialDamageEvent
            {
                attacker = damage.Instigator as Pawn, target = target,
                normalHit = request != null && (request.normalHit || request.isNormalAttack),
                canAccumulate = request?.canAccumulate ?? true,
                id = ++nextId, previous = CurrentDamage
            };
            CurrentDamage = e;
            HoshinoEmpoweredAttack.Prepare(e, ref damage);
            return e;
        }

        //发布一次实际伤害，护盾完全吸收或闪避不会进入有效命中通知。
        public static void Complete(SpecialDamageEvent e, DamageWorker.DamageResult result)
        {
            e.amount = result.totalDamageDealt;
            if (e.amount <= 0 || e.attacker == null) return;
            Map map = e.target.MapHeld ?? e.attacker.Map;
            if (map == null) return;
            foreach (var state in map.GetComponent<MapComponent_SpecialSkills>().States.ToArray())
            {
                if (e.canAccumulate)
                {
                    if (state.profile.role == SpecialSkillRole.Wakamo) WakamoSkills.Record(state, e.attacker, e.target, e.amount);
                    if (state.profile.role == SpecialSkillRole.Kei) KeiSkills.Record(state, e.attacker, e.target, e.amount);
                }
                if (e.normalHit && state.pawn == e.attacker)
                    SpecialSkillDispatcher.NormalHit(state, e.target);
            }
            DamageApplied?.Invoke(e);
        }

        //通知成功施法并回收使用完毕的一次性复制能力。
        public static void Activated(Ability ability)
        {
            AbilityActivated?.Invoke(ability);
            var owner = RioSkills.Owner(ability);
            if (owner != null) RioSkills.RemoveCopy(owner);
        }
    }
}
