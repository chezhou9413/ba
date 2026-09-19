using Verse;

namespace BANWlLib.Skills
{
    //妮露技能，负责大亢奋、锐气叠层和双阶段EX循环。
    public static class NeroSkills
    {
        //根据按钮阶段施加大亢奋或消耗COST后的攻击。
        public static void Cast(Hediff_SpecialSkillState s, SpecialSkillCommand command, Pawn target)
        {
            if (command == SpecialSkillCommand.Ex)
            {
                ExMechanismBuff.Apply(s.pawn, s.profile.exMultiplier, s.profile.durationTicks);
                ExMechanismBuff.Apply(target, s.profile.exMultiplier, s.profile.durationTicks);
                s.stage = 1;
                s.endTick = s.Now + s.profile.durationTicks;
                SpecialEffects.Trigger(s.profile.stageEffecter, s.pawn);
                return;
            }
            s.stacks = UnityEngine.Mathf.Min(s.profile.maxStacks, s.stacks + 1);
            float multiplier = 1f + s.stacks * s.profile.stackMultiplier;
            if (!s.profile.exAttack.useBattleStats) multiplier *= s.profile.exMultiplier;
            SpecialCombatUtility.Schedule(s, target, s.profile.exAttack, multiplier);
        }

        //大亢奋结束后清空锐气并恢复默认EX入口。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.stage != 1 || s.Active) return;
            s.stage = s.stacks = 0;
            s.endTick = -1;
            SpecialEffects.Trigger(s.profile.endEffecter, s.pawn);
        }
    }
}

