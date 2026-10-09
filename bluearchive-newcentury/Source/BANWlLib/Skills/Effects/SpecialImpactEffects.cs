using System;
using BANWlLib.Projectiles;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //命中特效职责：按攻击段开关固定弹道方向，保留原版粒子、音效和延迟生命周期。
    public static class SpecialImpactEffects
    {
        //触发职责：普通攻击段沿用落点特效，定向段为每个喷射子节点创建独立旋转参数。
        public static void Trigger(EffecterDef def, SpecialPendingAttack attack, Thing hit, IntVec3 cell, Map map)
        {
            if (def == null) return;
            SpecialAttackConfig action = attack.action;
            if (!action.rotateImpactEffect) { SpecialEffects.Trigger(def, cell, map); return; }
            float angle = attack.impactDirection.AngleFlat();
            Effecter effect = def.Spawn();
            TargetInfo point = new TargetInfo(cell, map);
            int maintainTicks = Math.Max(1, def.maintainTicks);
            for (int i = 0; i < effect.children.Count; i++)
            {
                SubEffecterDef childDef = def.children[i];
                SubEffecter child = effect.children[i];
                if (child is SubEffecter_Sprayer)
                {
                    //只替换本次Effecter的子节点，所有角色共享的原始Def保持不变。
                    childDef = SpecialImpactSprayerConfig.Create(childDef, angle, action.impactRotationOffset);
                    child = childDef.Spawn(effect);
                    effect.children[i] = child;
                }
                if (child is SubEffecter_DirectionalImpactMote)
                    TriggerDirectional(child, childDef, attack, hit, point);
                else child.SubTrigger(point, point);
                maintainTicks = Math.Max(maintainTicks, childDef.initialDelayTicks + 1);
            }
            //固定格子作为延迟播放锚点，避免目标移动或倒地重新计算方向。
            map.effecterMaintainer.AddEffecterToMaintain(effect, point, point, maintainTicks);
        }

        //方向上下文职责：沿用已有定向命中子特效的队列，并在触发结束后释放临时目标记录。
        private static void TriggerDirectional(SubEffecter child, SubEffecterDef source,
            SpecialPendingAttack attack, Thing hit, TargetInfo point)
        {
            if (hit == null) return;
            var action = attack.action;
            DirectionalImpactEffectContext.Register(hit, attack.impactDirection, action.impactEffectSpeed,
                action.impactOffsetForward, action.impactOffsetUp);
            try
            {
                var directional = new SubEffecter_DirectionalImpactMote(
                    SpecialImpactSprayerConfig.Create(source, 0f, action.impactRotationOffset), child.parent);
                directional.SubTrigger(point, new TargetInfo(hit));
            }
            finally { DirectionalImpactEffectContext.Clear(hit); }
        }
    }
}
