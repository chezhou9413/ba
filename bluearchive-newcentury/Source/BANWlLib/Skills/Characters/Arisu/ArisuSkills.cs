using Verse;

namespace BANWlLib.Skills
{
    //爱丽丝技能，两档充能、阶段特效和按有效命中触发普通技能。
    public static class ArisuSkills
    {
        //点自己充能，点配置允许的其他生物时按当前档位攻击并消费全部充能。
        public static void Cast(Hediff_SpecialSkillState s, Pawn target)
        {
            ArisuSkillConfig config = s.profile.arisu;
            if (target == s.pawn)
            {
                s.stage = UnityEngine.Mathf.Min(2, s.stage + 1);
                s.CleanupEffect();
                return;
            }
            int stage = s.stage;
            //准备期保留实体目标，光束出射时才由施法工作锁定直线范围。
            ((JobDriver_ArisuExChannel)s.pawn.jobs.curDriver).BeginAttack(stage);
            s.castEndTick = s.Now + config.damageDelayTicks + s.profile.exAttack.ShotDelay(s.profile.exAttack.shots - 1);
            s.stage = 0;
            s.CleanupEffect();
        }

        //清理EX发射时间，不改变独立普通技能冷却或有效命中进度。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.castEndTick >= 0 && s.Now > s.castEndTick) s.castEndTick = -1;
        }
    }
}
