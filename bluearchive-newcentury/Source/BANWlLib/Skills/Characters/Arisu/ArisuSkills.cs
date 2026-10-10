using System.Linq;
using BANWlLib.BaVerb;
using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //爱丽丝技能，负责两档充能、阶段特效和按有效命中触发普通技能。
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
                config.chargeSound?.PlayOneShot(new TargetInfo(s.pawn));
                SpecialDirectionalEffects.Trigger(config.chargeEffecters[s.stage - 1], s.pawn, s.pawn);
                return;
            }
            int stage = s.stage;
            //几何范围与阶段倍率在施法时固定，目标死亡后余下伤害段仍作用于这条直线。
            var cells = BattleTargetPreviewUtility.CalculateLineCells(s.pawn, target, config.lineLength, config.lineWidth).ToList();
            config.attackSound?.PlayOneShot(new TargetInfo(s.pawn));
            SpecialDirectionalEffects.Trigger(config.attackEffecters[stage], s.pawn, target);
            //工作目标固定为瞄准格，持续引导不再依赖原目标的移动或存活。
            var castingJob = s.pawn.CurJob;
            castingJob.targetA = new LocalTargetInfo(target.Position);
            SpecialCombatUtility.Schedule(s, null, s.profile.exAttack, 1f + stage,
                extraDelay: config.damageDelayTicks, center: s.pawn.Position, areaCells: cells, castingJob: castingJob);
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
