using System.Linq;
using BANWlLib.BaVerb;
using BANWlLib.BattleSystem;
using BANWlLib.Projectiles;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //星野强化普攻，负责用定向特效和单次范围伤害替代每一发武器弹丸。
    public static class HoshinoEmpoweredAttack
    {
        //按当前射击方向播放指定特效，每个范围目标只接受本发的一次普攻伤害。
        public static bool Fire(Verb_LaunchProjectile verb, Hediff_SpecialSkillState s)
        {
            LocalTargetInfo target = verb.CurrentTarget;
            Pawn pawn = s.pawn;
            if (!target.IsValid || (target.HasThing && target.Thing.Map != pawn.Map) ||
                !target.Cell.InBounds(pawn.Map) || !verb.TryFindShootLineFromTo(pawn.Position, target, out ShootLine line)) return false;
            HoshinoSkillConfig config = s.profile.hoshino;
            SpecialAttackConfig attack = config.empoweredAttack;
            var cells = BattleTargetPreviewUtility.CalculateFanCells(pawn, target, verb.EffectiveRange, config.empoweredFanArc);
            PlayEffect(pawn, target, config);
            SpecialEffects.Trigger(attack.effecterDef, target.Cell, pawn.Map);
            //目标列表在伤害前固定，死亡或其他伤害事件不会改变本轮枚举。
            foreach (Pawn victim in pawn.Map.mapPawns.AllPawnsSpawned.ToArray())
            {
                if (victim.Dead || !cells.Contains(victim.Position) ||
                    !BattleStatUtility.ShouldAffectTarget(pawn, victim, attack)) continue;
                BattleStatUtility.ApplyDamage(new BattleDamageRequest
                {
                    instigator = pawn, target = victim, damageDef = attack.damageDef,
                    useBattleStats = attack.useBattleStats, basePower = attack.basePower,
                    weaponBaseAttack = attack.weaponBaseAttack, baseMasteryMultiplier = attack.baseMasteryMultiplier,
                    penetration = attack.penetration, mechanismMultiplier = attack.attackPowerRatio,
                    isNormalAttack = true, useNormalAttackStat = true, normalHit = true,
                    canCrit = attack.canCrit, alwaysCrit = attack.alwaysCrit, applyAffinity = attack.applyAffinity
                });
            }
            return true;
        }

        //从施法者位置沿目标方向生成特效，方向上下文仅在本次触发期间有效。
        private static void PlayEffect(Pawn pawn, LocalTargetInfo target, HoshinoSkillConfig config)
        {
            Vector3 direction = (target.CenterVector3 - pawn.DrawPos).Yto0().normalized;
            DirectionalImpactEffectContext.Register(pawn, direction, config.effectSpeed, config.effectOffsetForward, config.effectOffsetUp);
            try
            {
                Effecter effect = config.empoweredShotEffecter.Spawn();
                effect.Trigger(new TargetInfo(pawn), new TargetInfo(pawn));
                effect.Cleanup();
            }
            finally { DirectionalImpactEffectContext.Clear(pawn); }
        }
    }
}
