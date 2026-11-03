using UnityEngine;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //白子技能，无人机生命周期、EX自动技能和不死状态解除。
    public static class ShirokoSkills
    {
        //安全消耗自身生命并启动或刷新跟随无人机。
        public static void Cast(Hediff_SpecialSkillState s)
        {
            SpecialHealthUtility.SpendNonlethal(s.pawn, s.profile.lifeCostRatio);
            s.endTick = s.Now + s.profile.durationTicks;
            s.nextNormalTick = s.Now;
        }

        //武器每发成功射出时追击瞄准目标，宿主是否命中和造成伤害不影响触发。
        public static void ShotFired(Hediff_SpecialSkillState s, Thing target)
        {
            if (!s.Active || target == null || !target.Spawned || target.Map != s.pawn.Map ||
                !target.HostileTo(s.pawn) || (target is Pawn pawn && pawn.Dead)) return;
            SpecialCombatUtility.Schedule(s, target, s.profile.droneAttack, drone: true);
            s.castEndTick = Mathf.Max(s.castEndTick, s.Now + s.profile.droneAttack.ShotDelay(s.profile.droneAttack.shots - 1));
        }

        //无人机持续期间或仍在完成锁定连射时保留机体。
        public static bool DronePresent(Hediff_SpecialSkillState s) => s.Active || s.castEndTick >= s.Now;

        //无人机到期结束表现，满血则清除不死层数但保留被动冷却。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            //EX到期只取消尚未投掷的前摇，已启动的连射仍保持普攻锁定。
            if (!s.Active && s.shiroko.warmupStartTick >= 0) ShirokoNormalSkill.CancelCast(s);
            if (s.endTick > 0 && !DronePresent(s))
            {
                s.endTick = -1;
                s.castEndTick = -1;
                SpecialEffects.Trigger(s.profile.endEffecter, s.pawn);
            }
            if (s.stacks > 0 && s.pawn.health.summaryHealth.SummaryHealthPercent >= 0.99999f)
            {
                s.stacks = 0;
                RemoveBuffer(s);
            }
            else AdvanceBuffer(s);
            if (s.forceDeath) return;
            if (s.Active) ShirokoNormalSkill.TryCast(s);
        }

        //满血或所属技能移除时同步移除退场缓冲，不改变被动冷却。
        public static void RemoveBuffer(Hediff_SpecialSkillState s)
        {
            s.shiroko.nextBufferStackTick = -1;
            if (s.countBuff != null && s.pawn.health.hediffSet.hediffs.Contains(s.countBuff))
                s.pawn.health.RemoveHediff(s.countBuff);
            s.countBuff = null;
        }

        //离图或状态移除时清理技能工作和未发射攻击，飞行中的弹丸独立完成。
        public static void Clear(Hediff_SpecialSkillState s)
        {
            ShirokoNormalSkill.CancelCast(s);
            s.activeMap?.GetComponent<MapComponent_SpecialSkills>().CancelPending(s);
            s.castEndTick = s.shiroko.normalEndTick = -1;
            if (s.pawn.Dead) RemoveBuffer(s);
        }

        //致命伤只负责首次触发；eventId保留入口兼容，不再参与层数或计时。
        public static bool ProtectLethal(Hediff_SpecialSkillState s, int eventId)
        {
            if (!s.native || s.forceDeath || (s.stacks == 0 && s.Now < s.passiveReadyTick)) return false;
            bool triggered = s.stacks == 0;
            if (triggered)
            {
                s.stacks = 1;
                s.passiveReadyTick = s.Now + s.profile.immortalityCooldownTicks;
                s.shiroko.nextBufferStackTick = s.Now + GenTicks.TicksPerRealSecond;
            }
            //伤害先于本Tick的Hediff更新时也使用同一截止时间，不能多保护一帧。
            AdvanceBuffer(s);
            if (s.forceDeath) return false;
            if (s.countBuff == null || !s.pawn.health.hediffSet.hediffs.Contains(s.countBuff))
            {
                var buffer = (Hediff_ShirokoRetirementBuffer)HediffMaker.MakeHediff(s.profile.shiroko.retirementBufferHediff, s.pawn);
                buffer.owner = s;
                s.countBuff = buffer;
                s.pawn.health.AddHediff(buffer);
            }
            if (triggered) SpecialEffects.Trigger(s.profile.stageEffecter, s.pawn);
            return true;
        }

        //每满60个游戏Tick增长一层；重复调用、暂停和伤害包数量不改变时钟。
        private static void AdvanceBuffer(Hediff_SpecialSkillState s)
        {
            if (s.stacks <= 0 || s.forceDeath) return;
            //旧档只有层数，没有秒内进度；保留层数和冷却，从完整一秒后续增。
            if (s.shiroko.nextBufferStackTick < 0)
                s.shiroko.nextBufferStackTick = s.Now + GenTicks.TicksPerRealSecond;
            while (s.stacks < s.profile.lethalStackLimit && s.Now >= s.shiroko.nextBufferStackTick)
            {
                s.shiroko.nextBufferStackTick += GenTicks.TicksPerRealSecond;
                s.stacks++;
                SpecialEffects.Trigger(s.profile.stageEffecter, s.pawn);
            }
            if (s.stacks < s.profile.lethalStackLimit) return;
            s.stacks = s.profile.lethalStackLimit;
            s.shiroko.nextBufferStackTick = -1;
            s.forceDeath = true;
            s.pawn.Kill(null);
        }
    }
}
