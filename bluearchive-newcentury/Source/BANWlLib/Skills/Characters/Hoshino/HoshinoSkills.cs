using System.Linq;
using BANWlLib.BattleMovement;
using BANWlLib.BattleSystem;
using BANWlLib.KindStats;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //星野形态技能，负责同一角色的基础属性、范围攻击、移动护盾和命中次数。
    public static class HoshinoSkills
    {
        //读取原生星野当前形态的基础攻击，不改变共享武器定义。
        public static float BaseAttack(Pawn pawn)
        {
            var s = Hediff_SpecialSkillState.Find(pawn, SpecialSkillRole.Hoshino);
            return s == null ? -1f : s.stage == 0 ? s.profile.outputBaseAttack : s.profile.tankBaseAttack;
        }

        //读取形态基础生命，并换算为项目的百点生命单位。
        public static float BaseHealth(Pawn pawn)
        {
            var s = Hediff_SpecialSkillState.Find(pawn, SpecialSkillRole.Hoshino);
            return s == null ? -1f : (s.stage == 0 ? s.profile.outputBaseHealth : s.profile.tankBaseHealth) / 100f;
        }

        //处理免费切换、攻击分段连射、防御普通技能和防御平面位移。
        public static void Cast(Hediff_SpecialSkillState s, SpecialSkillCommand command, LocalTargetInfo target)
        {
            if (command == SpecialSkillCommand.SwitchForm) { Switch(s); return; }
            if (command == SpecialSkillCommand.Normal) { HoshinoNormalSkill.Defend(s); return; }
            if (command == SpecialSkillCommand.Ex)
            {
                int lastShot = 0;
                foreach (HoshinoAttackStage stage in s.profile.hoshino.exStages)
                {
                    int delay = s.profile.outputCastTicks + stage.delayTicks;
                    SpecialCombatUtility.Schedule(s, stage.targetLocation ? null : target.Thing, stage.attack,
                        extraDelay: delay - 1, center: stage.targetLocation ? (IntVec3?)target.Cell : null);
                    lastShot = Mathf.Max(lastShot, delay + (stage.attack.shots - 1) * stage.attack.shotIntervalTicks);
                }
                s.castEndTick = s.Now + lastShot;
                s.hoshino.grantAfterCast = s.native;
                s.hoshino.outputExCasting = true;
                return;
            }
            IntVec3 destination = BattleMovementPathUtility.ResolveBlockedDestination(s.pawn, target.Cell, false, false);
            if (destination == s.pawn.Position) { Arrive(s); return; }
            Map map = s.pawn.Map;
            IntVec3 start = s.pawn.Position;
            s.castEndTick = s.Now + Mathf.CeilToInt(start.DistanceTo(destination) / s.profile.moveSpeed);
            //位移载体暂时收纳角色，不应被当作真正离图而清除独立普通技能。
            s.hoshino.moving = true;
            var flyer = (HoshinoMovementFlyer)PawnFlyer.MakeFlyer(
                DefDatabase<ThingDef>.GetNamed("BANW_SpecialHoshinoFlyer"), s.pawn, destination, null, null);
            flyer.state = s;
            flyer.speed = s.profile.moveSpeed;
            GenSpawn.Spawn(flyer, start, map);
        }

        //切换后按生命倍率缩放已有伤口，保留同一角色与其他状态。
        public static void Switch(Hediff_SpecialSkillState s)
        {
            float previousScale = s.pawn.HealthScale;
            //终止旧形态已经开始的连射，使下一次攻击完整使用目标形态的发数。
            foreach (Verb verb in s.pawn.equipment.Primary?.GetComp<CompEquippable>()?.AllVerbs ?? Enumerable.Empty<Verb>())
                if (verb is Verb_LaunchProjectile) verb.Reset();
            ClearFormEffects(s);
            s.stage = s.stage == 0 ? 1 : 0;
            HealthScaleCache.Invalidate(s.pawn);
            float factor = s.pawn.HealthScale / previousScale;
            foreach (Hediff_Injury injury in s.pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList())
                injury.Severity *= factor;
            s.pawn.health.summaryHealth.Notify_HealthChanged();
            SpecialEffects.Trigger(s.profile.stageEffecter, s.pawn);
        }

        //落地后启动属性增益、个人护盾和前向拦截。
        public static void Arrive(Hediff_SpecialSkillState s)
        {
            s.hoshino.moving = false;
            s.activeMap = s.pawn.Map;
            s.castEndTick = -1;
            s.endTick = s.Now + s.profile.durationTicks;
            s.appliedBuff = TimedSkillBuff.Apply(s.pawn, s.profile.buffHediff, s.profile.durationTicks);
            BattleStatUtility.ApplyShield(new BattleShieldRequest
            {
                instigator = s.pawn, target = s.pawn, shieldHediffDef = s.profile.shieldHediff,
                shieldPowerRatio = s.profile.shieldRatio, source = BattleShieldSource.MaxHealth,
                useBattleStats = s.profile.shieldUseBattleStats, basePower = s.profile.shieldBasePower
            });
            s.appliedShield = s.pawn.health.hediffSet.GetFirstHediffOfDef(s.profile.shieldHediff);
        }

        //结束持续状态，延迟伤害已经独立保存在地图调度器。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.castEndTick > 0 && s.Now > s.castEndTick)
            {
                s.castEndTick = -1;
                if (s.hoshino.grantAfterCast && s.stage == 0) s.hoshino.empoweredReady = true;
                s.hoshino.grantAfterCast = false;
                s.hoshino.outputExCasting = false;
            }
            if (s.endTick > 0 && !s.Active)
            {
                ClearExEffects(s);
                SpecialEffects.Trigger(s.profile.endEffecter, s.pawn);
            }
            HoshinoNormalSkill.Tick(s);
        }

        //结束防御EX的增益与护盾，保留独立普通技能的受伤计数。
        private static void ClearExEffects(Hediff_SpecialSkillState s)
        {
            foreach (Hediff h in new[] { s.appliedBuff, s.appliedShield })
                if (h != null && s.pawn.health.hediffSet.hediffs.Contains(h)) s.pawn.health.RemoveHediff(h);
            s.appliedBuff = s.appliedShield = null;
            s.endTick = -1;
        }

        //切形态或离图时清理当前形态状态，保留普攻累计和技能冷却。
        public static void ClearFormEffects(Hediff_SpecialSkillState s)
        {
            ClearExEffects(s);
            if (s.countBuff != null) s.pawn.health.RemoveHediff(s.countBuff);
            s.countBuff = null;
            s.castEndTick = -1;
            s.remainingHits = 0;
            s.hoshino.empoweredReady = s.hoshino.empoweredBurst = s.hoshino.grantAfterCast = false;
            s.hoshino.outputExCasting = s.hoshino.moving = false;
        }
    }
}
