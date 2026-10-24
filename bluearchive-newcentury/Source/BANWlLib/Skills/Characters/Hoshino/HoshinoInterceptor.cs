using UnityEngine;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //星野前向护盾几何逻辑，逐段拦截从外部进入前方半圆的敌方弹丸。
    public static class HoshinoInterceptor
    {
        //检查弹丸本次位移线段，扣除同一护盾余额后销毁弹丸而不触发爆炸。
        public static bool TryIntercept(Projectile projectile, Vector3 from, Vector3 to)
        {
            if (projectile.Map == null || projectile.Launcher == null) return false;
            foreach (var s in projectile.Map.GetComponent<MapComponent_SpecialSkills>().States)
            {
                if (s.profile.role != SpecialSkillRole.Hoshino || s.stage != 1 || !s.Active ||
                    !s.pawn.Spawned || !projectile.Launcher.HostileTo(s.pawn) || !HasShield(s)) continue;
                Vector3 center = s.pawn.DrawPos;
                Vector2 start = new Vector2(from.x - center.x, from.z - center.z);
                Vector2 delta = new Vector2(to.x - from.x, to.z - from.z);
                float radius = s.profile.interceptRadius;
                if (start.sqrMagnitude <= radius * radius || delta.sqrMagnitude < 0.000001f) continue;
                float a = delta.sqrMagnitude;
                float b = 2f * Vector2.Dot(start, delta);
                float c = start.sqrMagnitude - radius * radius;
                float discriminant = b * b - 4f * a * c;
                if (discriminant < 0f) continue;
                float t = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
                if (t < 0f || t > 1f) continue;
                Vector2 point = start + delta * t;
                Vector3 forward = Quaternion.AngleAxis(SpecialDirectionalEffects.AimAngle(s.pawn, LocalTargetInfo.Invalid), Vector3.up) * Vector3.forward;
                if (Vector2.Dot(point, new Vector2(forward.x, forward.z)) < 0f) continue;
                HoshinoInterceptDamage.Apply(projectile, s.pawn);
                SpecialEffects.Trigger(s.profile.hoshino.interceptEffecter, Vector3.Lerp(from, to, t).ToIntVec3(), projectile.Map);
                projectile.Destroy();
                return true;
            }
            return false;
        }

        //拦截与边界绘制都以仍在角色身上的真实护盾余额为准。
        public static bool HasShield(Hediff_SpecialSkillState s)
        {
            return s.appliedShield != null && s.pawn.health.hediffSet.hediffs.Contains(s.appliedShield) &&
                s.appliedShield.TryGetComp<HediffComp_HoshinoShield>()?.RemainingShield > 0.01f;
        }

        //绘制前向半圆边界，使玩家可以判断实际拦截方向。
        public static void Draw(Hediff_SpecialSkillState s)
        {
            if (!s.profile.hoshino.drawInterceptBoundary) return;
            Vector3 center = s.pawn.DrawPos;
            center.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            float angle = SpecialDirectionalEffects.AimAngle(s.pawn, LocalTargetInfo.Invalid);
            Vector3 previous = center + Quaternion.AngleAxis(angle - 90f, Vector3.up) * Vector3.forward * s.profile.interceptRadius;
            for (int i = 1; i <= 18; i++)
            {
                Vector3 next = center + Quaternion.AngleAxis(angle - 90f + i * 10f, Vector3.up) * Vector3.forward * s.profile.interceptRadius;
                GenDraw.DrawLineBetween(previous, next, SimpleColor.Cyan);
                previous = next;
            }
        }

        //个人护盾与外沿拦截使用相同正面半圆，背后和无来源环境伤害不消耗盾。
        public static bool CanBlockDamage(Pawn pawn, DamageInfo damage)
        {
            if (HoshinoInterceptDamage.InterceptingPawn == pawn) return true;
            Thing source = damage.Instigator;
            bool projectileDirection = damage.Weapon?.IsRangedWeapon == true;
            if (!projectileDirection && (source == null || !source.Spawned || source.Map != pawn.Map)) return false;
            //普通武器弹按真正入射角判断，直接技能伤害按施法来源判断。
            Vector3 towardSource = projectileDirection
                ? -(Quaternion.AngleAxis(damage.Angle, Vector3.up) * Vector3.forward)
                : (source.DrawPos - pawn.DrawPos).Yto0();
            Vector3 forward = Quaternion.AngleAxis(SpecialDirectionalEffects.AimAngle(pawn, LocalTargetInfo.Invalid), Vector3.up) * Vector3.forward;
            return towardSource.sqrMagnitude > 0.000001f && Vector3.Dot(towardSource, forward) >= 0f;
        }
    }
}
