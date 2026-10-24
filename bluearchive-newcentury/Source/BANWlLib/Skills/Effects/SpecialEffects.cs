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
            if (def != null && target?.Map != null) Trigger(def, new TargetInfo(target));
        }

        //将控制器交给地图维护，让延迟和持续子特效有机会完成播放。
        public static void Trigger(EffecterDef def, IntVec3 cell, Map map)
        {
            if (def == null || map == null) return;
            Trigger(def, new TargetInfo(cell, map));
        }

        //保留实体锚点，让附着子特效在整个生命周期内跟随角色。
        private static void Trigger(EffecterDef def, TargetInfo point)
        {
            Effecter effecter = def.Spawn();
            effecter.Trigger(point, point);
            point.Map.effecterMaintainer.AddEffecterToMaintain(effecter, point, point, DurationTicks(def));
        }

        //维护到最后一个延迟子节点播放完，粒子本身仍使用各自配置的完整时长。
        public static int DurationTicks(EffecterDef def)
        {
            return Mathf.Max(1, def.maintainTicks,
                def.children.NullOrEmpty() ? 0 : def.children.Max(ChildDuration));
        }

        //组合子节点按延迟加粒子寿命计算，不用统一一秒截断。
        private static int ChildDuration(SubEffecterDef child)
        {
            float seconds = Mathf.Max(child.moteDef?.mote?.Lifespan ?? 0f, child.fleckDef?.Lifetime ?? 0f);
            int nested = child.children.NullOrEmpty() ? 0 : child.children.Max(ChildDuration);
            return child.initialDelayTicks + Mathf.Max(1, Mathf.CeilToInt(seconds * 60f), nested);
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
                (command == SpecialSkillCommand.SwitchForm || command == SpecialSkillCommand.Normal ||
                 command == SpecialSkillCommand.AlternateEx)) return;
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
