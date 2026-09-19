using BANWlLib.BattleSystem;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //星野强化普攻，负责替换原始命中伤害并限制范围伤害不递归扩散。
    public static class HoshinoEmpoweredAttack
    {
        //原始普攻命中使用强化配置，范围次级命中只消耗剩余额度。
        public static void Prepare(SpecialDamageEvent e, ref DamageInfo damage)
        {
            if (!e.normalHit) return;
            var s = Hediff_SpecialSkillState.Find(e.attacker, SpecialSkillRole.Hoshino);
            if (s == null || s.stage != 0) return;
            bool secondary = SpecialDamageScope.Current?.areaSecondary == true;
            e.expandArea = !secondary;
            if (secondary)
            {
                if (s.remainingHits <= 0) damage.SetAmount(0f);
                return;
            }
            if (s.remainingHits <= 0) return;
            SpecialAttackConfig a = s.profile.normalAttack;
            var request = new BattleDamageRequest
            {
                instigator = e.attacker, target = e.target, damageDef = a.damageDef,
                attackPowerRatio = a.attackPowerRatio, useBattleStats = a.useBattleStats,
                basePower = a.basePower, mechanismMultiplier = s.profile.empoweredMultiplier,
                canCrit = a.canCrit, applyAffinity = a.applyAffinity, normalHit = true
            };
            BattleDamageResult result = BattleStatUtility.BuildDamageResult(request);
            damage.Def = a.damageDef;
            damage.SetAmount(result.finalAmount);
            e.scope = SpecialDamageScope.Enter(request);
            BattleDamageDisplayState.RegisterManualDamage(e.target, e.attacker, result.isCrit);
            BattleDamageDisplayState.RegisterCriticalFloatText(e.target, result.isCrit);
        }
    }
}

