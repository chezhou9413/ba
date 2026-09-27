using UnityEngine;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //白子技能，负责无人机生命周期、武器发射追击、锁定连射和不死状态解除。
    public static class ShirokoSkills
    {
        //安全消耗自身生命并启动或刷新跟随无人机。
        public static void Cast(Hediff_SpecialSkillState s)
        {
            SpecialHealthUtility.SpendNonlethal(s.pawn, s.profile.lifeCostRatio);
            s.endTick = s.Now + s.profile.durationTicks;
        }

        //武器每发成功射出时追击瞄准目标，宿主是否命中和造成伤害不影响触发。
        public static void ShotFired(Hediff_SpecialSkillState s, Thing target)
        {
            if (!s.Active || target == null || !target.Spawned || target.Map != s.pawn.Map ||
                !target.HostileTo(s.pawn) || (target is Pawn pawn && pawn.Dead)) return;
            SpecialCombatUtility.Schedule(s, target, s.profile.droneAttack, drone: true);
        }

        //无人机持续期间或仍在完成锁定连射时保留机体。
        public static bool DronePresent(Hediff_SpecialSkillState s) => s.Active || s.castEndTick >= s.Now;

        //锁定本次普通技能目标，宿主攻击和无人机连射均使用白子的属性快照。
        public static void Normal(Hediff_SpecialSkillState s, Pawn target)
        {
            SpecialCombatUtility.Schedule(s, target, s.profile.normalAttack);
            if (!s.Active) return;
            //已开始的整轮连射在无人机到期后仍完成，普攻追击依然只在EX有效期内触发。
            s.castEndTick = Mathf.Max(s.castEndTick, s.Now + 2 +
                (s.profile.burstAttack.shots - 1) * s.profile.burstAttack.shotIntervalTicks);
            SpecialCombatUtility.Schedule(s, target, s.profile.burstAttack, drone: true);
        }

        //无人机到期结束表现，满血则清除不死层数但保留被动冷却。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.endTick > 0 && !DronePresent(s))
            {
                s.endTick = -1;
                s.castEndTick = -1;
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
