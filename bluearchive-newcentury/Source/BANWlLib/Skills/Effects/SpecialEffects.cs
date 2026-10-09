using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //技能特效工具，负责一次性特效、阶段特效与施法声音。
    public static class SpecialEffects
    {
        //在角色当前位置触发一次特效。
        public static void Trigger(EffecterDef def, Thing target)
        {
            if (target?.Map != null) Trigger(def, target.Position, target.Map);
        }

        //在指定格触发一次特效并释放控制器。
        public static void Trigger(EffecterDef def, IntVec3 cell, Map map)
        {
            if (def == null || map == null) return;
            Effecter effecter = def.Spawn();
            effecter.Trigger(new TargetInfo(cell, map), TargetInfo.Invalid);
            effecter.Cleanup();
        }

        //读取充能阶段绑定的特效，不使用静默替换。
        public static EffecterDef Charge(List<EffecterDef> effects, int stage)
        {
            return effects == null || effects.Count == 0 ? null : effects[Mathf.Clamp(stage, 0, effects.Count - 1)];
        }

        //将妮露二段施法特效交给引导工作，其他入口按本次方向播放并保留落点与声音。
        public static void Cast(Hediff_SpecialSkillState state, LocalTargetInfo target)
        {
            var channel = state.pawn.jobs.curDriver as JobDriver_NeroExChannel;
            if (state.profile.role == SpecialSkillRole.Nero && channel != null)
                channel.BeginChannelEffect(state.profile.casterEffecter);
            else SpecialDirectionalEffects.Trigger(state.profile.casterEffecter, state.pawn, target);
            Trigger(state.profile.targetEffecter, target.Cell, state.pawn.Map);
            state.profile.castSound?.PlayOneShot(new TargetInfo(state.pawn));
        }
    }
}
