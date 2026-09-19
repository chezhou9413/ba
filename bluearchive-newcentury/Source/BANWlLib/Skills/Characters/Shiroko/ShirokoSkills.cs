using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //黑子技能，负责无人机生命周期、联合普通技能和不死状态解除。
    public static class ShirokoSkills
    {
        //安全消耗自身生命并启动或刷新跟随无人机。
        public static void Cast(Hediff_SpecialSkillState s)
        {
            SpecialHealthUtility.SpendNonlethal(s.pawn, s.profile.lifeCostRatio);
            s.endTick = s.Now + s.profile.durationTicks;
        }

        //安排宿主攻击和无人机连射，两段分别配置但使用同一宿主属性。
        public static void Normal(Hediff_SpecialSkillState s, Pawn target)
        {
            SpecialCombatUtility.Schedule(s, target, s.profile.normalAttack);
            if (s.Active) SpecialCombatUtility.Schedule(s, target, s.profile.burstAttack, drone: true);
        }

        //无人机到期结束表现，满血则清除不死层数但保留被动冷却。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.endTick > 0 && !s.Active)
            {
                s.endTick = -1;
                SpecialEffects.Trigger(s.profile.endEffecter, s.pawn);
            }
            if (s.stacks > 0 && s.pawn.health.summaryHealth.SummaryHealthPercent >= 0.99999f)
                s.stacks = 0;
        }

        //按一次真实伤害事件增加不死层数，第十五层交给强制死亡入口。
        public static bool ProtectLethal(Hediff_SpecialSkillState s, int eventId)
        {
            if (!s.native || s.forceDeath || (s.stacks == 0 && s.Now < s.passiveReadyTick)) return false;
            if (s.lastLethalEvent != eventId)
            {
                s.lastLethalEvent = eventId;
                if (s.stacks == 0) s.passiveReadyTick = s.Now + s.profile.immortalityCooldownTicks;
                s.stacks++;
                SpecialEffects.Trigger(s.profile.stageEffecter, s.pawn);
            }
            if (s.stacks < s.profile.lethalStackLimit) return true;
            s.forceDeath = true;
            s.pawn.Kill(null);
            return false;
        }
    }
}

