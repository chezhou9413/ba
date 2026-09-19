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

        //处理免费切换、输出延迟爆发或坦克平面位移。
        public static void Cast(Hediff_SpecialSkillState s, SpecialSkillCommand command, LocalTargetInfo target)
        {
            if (command == SpecialSkillCommand.SwitchForm) { Switch(s); return; }
            if (command == SpecialSkillCommand.Ex)
            {
                s.castEndTick = s.Now + s.profile.outputCastTicks;
                SpecialCombatUtility.Schedule(s, null, s.profile.exAttack,
                    extraDelay: s.profile.outputCastTicks - 1, center: target.Cell);
                return;
            }
            IntVec3 destination = BattleMovementPathUtility.ResolveBlockedDestination(s.pawn, target.Cell, false, false);
            if (destination == s.pawn.Position) { Arrive(s); return; }
            Map map = s.pawn.Map;
            IntVec3 start = s.pawn.Position;
            s.castEndTick = s.Now + Mathf.CeilToInt(start.DistanceTo(destination) / s.profile.moveSpeed);
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

        //周期普通技能授予形态对应的有效命中次数。
        public static void Normal(Hediff_SpecialSkillState s, Pawn target)
        {
            s.remainingHits = s.stage == 0 ? s.profile.empoweredHits : s.profile.tankHits;
            if (s.stage == 1 && s.profile.countHediff != null)
                s.countBuff = TimedSkillBuff.Apply(s.pawn, s.profile.countHediff, -1);
            SpecialEffects.Trigger(s.profile.stageEffecter, s.pawn);
        }

        //有效命中消耗一次额度，输出形态安排不递归触发普攻的范围追伤。
        public static void OnHit(Hediff_SpecialSkillState s, Thing target)
        {
            if (s.remainingHits <= 0) return;
            s.remainingHits--;
            if (s.stage == 0 && SpecialSkillEvents.CurrentDamage?.expandArea == true)
                SpecialCombatUtility.Schedule(s, null, s.profile.normalAttack,
                    multiplier: s.profile.empoweredMultiplier, normalHit: true, center: target.Position, excludedTarget: target);
            if (s.remainingHits == 0 && s.countBuff != null)
            {
                s.pawn.health.RemoveHediff(s.countBuff);
                s.countBuff = null;
            }
        }

        //结束持续状态，延迟伤害已经独立保存在地图调度器。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.castEndTick > 0 && s.Now > s.castEndTick) s.castEndTick = -1;
            if (s.endTick > 0 && !s.Active)
            {
                ClearFormEffects(s);
                SpecialEffects.Trigger(s.profile.endEffecter, s.pawn);
            }
        }

        //只清理当前形态拥有的状态和盾，不删除其他来源的增益。
        public static void ClearFormEffects(Hediff_SpecialSkillState s)
        {
            foreach (Hediff h in new[] { s.appliedBuff, s.appliedShield, s.countBuff })
                if (h != null && s.pawn.health.hediffSet.hediffs.Contains(h)) s.pawn.health.RemoveHediff(h);
            s.appliedBuff = s.appliedShield = s.countBuff = null;
            s.endTick = s.castEndTick = -1;
            s.remainingHits = 0;
        }
    }
}
