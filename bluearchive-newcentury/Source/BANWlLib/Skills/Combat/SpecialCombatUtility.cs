using System.Linq;
using BANWlLib.BattleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //特殊技能战斗工具，负责建立快照、安排连射、发射实体弹丸与结算范围伤害。
    public static class SpecialCombatUtility
    {
        //读取当前配置口径的攻击基数，供伤害记录上限使用。
        public static float Power(Pawn pawn, SpecialAttackConfig action)
        {
            return action.useBattleStats ? BattleStatUtility.GetFinalAttackPower(pawn) : action.basePower;
        }

        //判定角色是否可执行自动技能。
        public static bool CanAct(Pawn pawn)
        {
            return pawn.Spawned && !pawn.Dead && !pawn.Downed && pawn.Drafted &&
                !pawn.stances.stunner.Stunned && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
        }

        //优先使用当前攻击对象，再查找范围内最近的可见敌方角色。
        public static Pawn FindEnemy(Pawn pawn, float range)
        {
            Pawn current = pawn.CurJob?.targetA.Pawn;
            if (ValidEnemy(pawn, current, range)) return current;
            return pawn.Map.mapPawns.AllPawnsSpawned.Where(p => ValidEnemy(pawn, p, range))
                .OrderBy(p => p.Position.DistanceToSquared(pawn.Position)).FirstOrDefault();
        }

        //检查敌方目标是否处于当前地图、有效射程和视线内。
        public static bool ValidEnemy(Pawn pawn, Pawn target, float range)
        {
            return target != null && target.Spawned && !target.Dead && target.Map == pawn.Map &&
                target.HostileTo(pawn) && target.Position.DistanceTo(pawn.Position) <= range &&
                GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map);
        }

        //把一个总倍率攻击段拆为多个延迟发射事件，保留同一次施放的属性快照。
        public static void Schedule(Hediff_SpecialSkillState state, Thing target, SpecialAttackConfig action,
            float multiplier = 1f, bool drone = false, float resolved = -1f, bool startRecord = false,
            EffecterDef impactEffecter = null, int extraDelay = 0, IntVec3? center = null, bool normalHit = false, Thing excludedTarget = null)
        {
            if (action == null || state.pawn.Map == null) return;
            var snapshot = BattleStatUtility.CreateSnapshot(state.pawn);
            int count = Mathf.Max(1, action.shots);
            state.pendingAttacks += count;
            for (int i = 0; i < count; i++)
                state.pawn.Map.GetComponent<MapComponent_SpecialSkills>().Enqueue(new SpecialPendingAttack
                {
                    caster = state.pawn, state = state, target = target, action = action,
                    snapshot = snapshot, dueTick = state.Now + 1 + extraDelay + i * action.shotIntervalTicks,
                    multiplier = multiplier / count, resolvedAmount = resolved < 0 ? -1 : resolved / count,
                    canAccumulate = resolved < 0, startRecord = startRecord && i == count - 1,
                    droneOrigin = drone, impactEffecter = impactEffecter, normalHit = normalHit,
                    center = center ?? IntVec3.Invalid, excludedTarget = excludedTarget
                });
        }

        //发射技能弹丸或直接执行配置中的伤害段。
        public static void Launch(SpecialPendingAttack attack, Map map)
        {
            if (attack.target != null && (attack.target.Destroyed || !attack.target.Spawned || attack.target.Map != map))
            { attack.Complete(); return; }
            IntVec3 cell = attack.center.IsValid ? attack.center : attack.target?.Position ?? IntVec3.Invalid;
            if (!cell.IsValid || !cell.InBounds(map)) { attack.Complete(); return; }
            if (attack.action.projectileDef == null)
            {
                Impact(attack, attack.target, cell, map);
                attack.Complete();
                return;
            }
            var projectile = (Projectile_SpecialSkill)ThingMaker.MakeThing(attack.action.projectileDef);
            projectile.attack = attack;
            Vector3 origin = attack.droneOrigin ? DroneRenderer.Position(attack.state) : attack.caster.DrawPos;
            GenSpawn.Spawn(projectile, origin.ToIntVec3(), map);
            LocalTargetInfo target = attack.target != null ? new LocalTargetInfo(attack.target) : new LocalTargetInfo(cell);
            projectile.Launch(attack.caster, origin, target, target, ProjectileHitFlags.IntendedTarget);
        }

        //按实际命中位置处理单体或范围效果，并在首段完成后开始若藻记录。
        public static void Impact(SpecialPendingAttack attack, Thing hit, IntVec3 cell, Map map)
        {
            SpecialEffects.Trigger(attack.impactEffecter ?? attack.action.effecterDef, cell, map);
            if (attack.action.radius > 0f)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.ToArray())
                    if (!pawn.Dead && pawn != attack.excludedTarget && pawn.Position.DistanceTo(cell) <= attack.action.radius &&
                        BattleStatUtility.ShouldAffectTarget(attack.caster, pawn, attack.action))
                        Damage(attack, pawn);
            }
            else if (hit != null && BattleStatUtility.ShouldAffectTarget(attack.caster, hit, attack.action))
                Damage(attack, hit);
            if (attack.startRecord && hit is Pawn target && !target.Dead && attack.state != null)
                WakamoSkills.BeginRecord(attack.state, target, attack.snapshot);
        }

        //使用统一请求结算伤害，明确区分普攻、额外攻击和已蓄积金额。
        private static void Damage(SpecialPendingAttack attack, Thing target)
        {
            BattleActionConfig a = attack.action;
            BattleStatUtility.ApplyDamage(new BattleDamageRequest
            {
                instigator = attack.caster, target = target, damageDef = a.damageDef,
                attackPowerRatio = a.attackPowerRatio, weaponBaseAttack = a.weaponBaseAttack,
                baseMasteryMultiplier = a.baseMasteryMultiplier, penetration = a.penetration,
                useBattleStats = a.useBattleStats, basePower = a.basePower,
                mechanismMultiplier = attack.multiplier, snapshot = attack.snapshot,
                canCrit = a.canCrit, alwaysCrit = a.alwaysCrit, applyAffinity = a.applyAffinity,
                isExSkill = a.isExSkill, isNormalAttack = false, normalHit = attack.normalHit,
                areaSecondary = attack.excludedTarget != null,
                canAccumulate = attack.canAccumulate, resolvedAmount = attack.resolvedAmount
            });
        }
    }
}
