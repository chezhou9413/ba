using System;
using System.Linq;
using BANWlLib.BaClass;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //技能定向特效工具，负责沿用水花子的旋转偏移规则并固定延迟表现的瞄准方向。
    public static class SpecialDirectionalEffects
    {
        //在施法者位置触发特效，旋转偏移子节点按本次瞄准角度生成或排队。
        public static void Trigger(EffecterDef def, Pawn caster, LocalTargetInfo target)
        {
            if (def == null) return;
            Map map = caster.Map;
            TargetInfo source = new TargetInfo(caster);
            TargetInfo focus = new TargetInfo(target.Cell, map);
            float angle = AimAngle(caster, target);
            Effecter effect = def.Spawn();
            for (int i = 0; i < effect.children.Count; i++)
            {
                SubEffecter child = effect.children[i];
                SubEffecterDef childDef = def.children[i];
                if (child is SubEffecter_SprayerTriggeredRotatedOffset)
                    SpawnRotated(childDef, caster, angle);
                else child.SubTrigger(source, focus);
            }
            //方向子节点由地图队列托管，其余延迟子节点保留原版EffectTick生命周期。
            int maintainTicks = Math.Max(def.maintainTicks, 1 + def.children.Max(child => child.initialDelayTicks));
            map.effecterMaintainer.AddEffecterToMaintain(effect, source, focus, maintainTicks);
        }

        //读取本次目标的视觉方向，自身施法时使用当前瞄准目标或角色朝向。
        private static float AimAngle(Pawn caster, LocalTargetInfo target)
        {
            if (!target.IsValid || target.Thing == caster || target.Cell == caster.Position)
            {
                var stance = caster.stances.curStance as Stance_Busy;
                target = stance?.focusTarg ?? LocalTargetInfo.Invalid;
            }
            return target.IsValid && target.Thing != caster && target.Cell != caster.Position
                ? (target.CenterVector3 - caster.DrawPos).Yto0().AngleFlat()
                : caster.Rotation.AsAngle;
        }

        //把旋转后的偏移和角度固定下来，避免目标移动或倒地改变后续特效方向。
        private static void SpawnRotated(SubEffecterDef def, Pawn caster, float angle)
        {
            Vector3 offset = def.absoluteAngle ? def.positionOffset : def.positionOffset.RotatedBy(angle);
            Vector3 position = caster.Position.ToVector3Shifted() + offset;
            float rotation = def.rotation.RandomInRange + (def.absoluteAngle ? 0f : angle);
            float scale = def.scale.RandomInRange;
            if (def.initialDelayTicks > 0)
                RotatedOffsetMoteDelayComponent.Queue(caster.Map, def.moteDef, position, scale, rotation, def.initialDelayTicks);
            else SubEffecter_SprayerTriggeredRotatedOffset.SpawnMote(def.moteDef, position, caster.Map, scale, rotation);
        }
    }
}
