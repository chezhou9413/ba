using Verse;

namespace BANWlLib.Skills
{
    //妮露技能，负责对决、锐气叠层和双阶段EX循环。
    public static class NeroSkills
    {
        //一段给双方施加对决标记，二段叠加锐气后安排攻击。
        public static void Cast(Hediff_SpecialSkillState s, SpecialSkillCommand command, Pawn target)
        {
            if (command == SpecialSkillCommand.Ex)
            {
                Hediff_NeroDuel.Apply(s.pawn, s.profile.durationTicks);
                Hediff_NeroDuel.Apply(target, s.profile.durationTicks);
                s.stage = 1;
                s.endTick = s.Now + s.profile.durationTicks;
                SpecialDirectionalEffects.Trigger(s.profile.stageEffecter, s.pawn, target);
                return;
            }
            s.stacks = UnityEngine.Mathf.Min(s.profile.maxStacks, s.stacks + 1);
            float multiplier = 1f + s.stacks * s.profile.stackMultiplier;
            s.castEndTick = s.Now + s.profile.exAttack.ShotDelay(s.profile.exAttack.shots - 1);
            SpecialCombatUtility.Schedule(s, target, s.profile.exAttack, multiplier, castingJob: s.pawn.CurJob);
        }

        //对决结束后清空锐气并恢复默认EX入口。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.stage != 1 || s.Active) return;
            s.stage = s.stacks = 0;
            s.endTick = -1;
            SpecialEffects.Trigger(s.profile.endEffecter, s.pawn);
        }
    }
}
