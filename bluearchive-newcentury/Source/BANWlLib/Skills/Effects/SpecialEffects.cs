using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //技能特效工具，一次性特效、阶段特效与施法声音。
    public static class SpecialEffects
    {
        //在角色当前位置触发一次特效。
        public static void Trigger(EffecterDef def, Thing target)
        {
            if (target?.Map != null) Trigger(def, target.Position, target.Map);
        }

        //将控制器交给地图维护，让延迟和持续子特效有机会完成播放。
        public static void Trigger(EffecterDef def, IntVec3 cell, Map map)
        {
            if (def == null || map == null) return;
            Effecter effecter = def.Spawn();
            TargetInfo point = new TargetInfo(cell, map);
            effecter.Trigger(point, point);
            int delay = def.children.NullOrEmpty() ? 0 : def.children.Max(child => child.initialDelayTicks);
            map.effecterMaintainer.AddEffecterToMaintain(effecter, point, point,
                System.Math.Max(def.maintainTicks, delay + 1));
        }

        //读取充能阶段绑定的特效，不使用静默替换。
        public static EffecterDef Charge(List<EffecterDef> effects, int stage)
        {
            return effects == null || effects.Count == 0 ? null : effects[Mathf.Clamp(stage, 0, effects.Count - 1)];
        }

        //妮露一段只使用自身前摇与阶段特效，二段施法特效交给引导工作维护。
        public static void Cast(Hediff_SpecialSkillState state, SpecialSkillCommand command, LocalTargetInfo target)
        {
            if (state.profile.role == SpecialSkillRole.Hoshino &&
                (command == SpecialSkillCommand.SwitchForm || command == SpecialSkillCommand.Normal)) return;
            if (state.profile.role == SpecialSkillRole.Nero && command != SpecialSkillCommand.AlternateEx) return;
            var channel = state.pawn.jobs.curDriver as JobDriver_NeroExChannel;
            if (state.profile.role == SpecialSkillRole.Nero && channel != null)
                channel.BeginChannelEffect(state.profile.casterEffecter);
            else SpecialDirectionalEffects.Trigger(state.profile.casterEffecter, state.pawn, target);
            Trigger(state.profile.targetEffecter, target.Cell, state.pawn.Map);
            state.profile.castSound?.PlayOneShot(new TargetInfo(state.pawn));
        }
    }
}
