using Verse;

namespace BANWlLib.Skills
{
    //星野普通技能，负责普攻发数触发单体连射和防御状态的受伤次数。
    public static class HoshinoNormalSkill
    {
        //只累计原生攻击形态主武器实际成功发射的普攻。
        public static void ShotFired(Hediff_SpecialSkillState s)
        {
            if (s.stage == 0) s.hits++;
        }

        //达到触发次数后等待当前连射与技能结束，再寻找目标释放普通技能。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (!s.native || s.stage != 0 || s.castEndTick >= s.Now ||
                s.hits < s.profile.hoshino.normalRequiredShots || !SpecialCombatUtility.CanAct(s.pawn)) return;
            var weapon = s.pawn.equipment.Primary?.GetComp<CompEquippable>();
            if (weapon?.PrimaryVerb.Bursting == true) return;
            Pawn target = SpecialCombatUtility.FindEnemy(s.pawn, s.profile.range);
            if (target != null) Attack(s, target);
        }

        //消耗一次触发进度并安排单体连射，发射结束后赋予下一轮强化。
        public static void Attack(Hediff_SpecialSkillState s, Pawn target)
        {
            s.hits -= s.profile.hoshino.normalRequiredShots;
            SpecialCombatUtility.Schedule(s, target, s.profile.normalAttack);
            s.castEndTick = s.Now + 1 + (s.profile.normalAttack.shots - 1) * s.profile.normalAttack.shotIntervalTicks;
            s.hoshino.grantAfterCast = true;
            SpecialEffects.Trigger(s.profile.stageEffecter, s.pawn);
        }

        //主动施放防御普通技能，重新设置受伤额度和独立状态。
        public static void Defend(Hediff_SpecialSkillState s)
        {
            if (s.countBuff != null) s.pawn.health.RemoveHediff(s.countBuff);
            s.remainingHits = s.profile.tankHits;
            s.countBuff = TimedSkillBuff.Apply(s.pawn, s.profile.countHediff, -1);
            SpecialEffects.Trigger(s.profile.stageEffecter, s.pawn);
        }

        //每次实际受到伤害消耗一次防御额度，完全闪避或护盾完全吸收不计数。
        public static void DamageTaken(Thing target)
        {
            var s = Hediff_SpecialSkillState.Find(target as Pawn, SpecialSkillRole.Hoshino);
            if (s == null || s.stage != 1 || s.countBuff == null || s.remainingHits <= 0) return;
            if (--s.remainingHits > 0) return;
            s.pawn.health.RemoveHediff(s.countBuff);
            s.countBuff = null;
        }
    }
}
