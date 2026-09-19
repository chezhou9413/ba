using Verse;

namespace BANWlLib.Skills
{
    //爱丽丝技能，负责两档充能、阶段特效和按有效命中触发普通技能。
    public static class ArisuSkills
    {
        //点自己充能，点敌人按当前档位攻击并消费全部充能。
        public static void Cast(Hediff_SpecialSkillState s, Pawn target)
        {
            if (target == s.pawn)
            {
                s.stage = UnityEngine.Mathf.Min(2, s.stage + 1);
                SpecialEffects.Trigger(SpecialEffects.Charge(s.profile.chargeCastEffecters, s.stage), s.pawn);
                return;
            }
            int stage = s.stage;
            SpecialEffects.Trigger(SpecialEffects.Charge(s.profile.chargeCastEffecters, stage), s.pawn);
            SpecialCombatUtility.Schedule(s, target, s.profile.exAttack, 1f + stage,
                impactEffecter: SpecialEffects.Charge(s.profile.chargeImpactEffecters, stage));
            s.stage = 0;
        }

        //消费三个命中计数执行普通攻击技能，保留超出阈值的次数。
        public static void Normal(Hediff_SpecialSkillState s, Pawn target)
        {
            s.hits = UnityEngine.Mathf.Max(0, s.hits - s.profile.requiredHits);
            SpecialCombatUtility.Schedule(s, target, s.profile.normalAttack);
        }
    }
}

