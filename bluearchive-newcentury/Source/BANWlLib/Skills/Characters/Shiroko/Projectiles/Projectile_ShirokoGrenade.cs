using UnityEngine;
using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //固定飞行时长的跟踪手榴弹，落点跟随目标但不重置飞行进度。
    [StaticConstructorOnStartup]
    public class Projectile_ShirokoGrenade : Projectile_SpecialSkill
    {
        private static readonly Material Shadow = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent);
        private int flightTicks;
        private float arcHeight;
        private float Progress => flightTicks > 0 ? Mathf.Clamp01(1f - (float)ticksToImpact / flightTicks) : 0f;
        public override Vector3 ExactPosition => Vector3.Lerp(origin.Yto0(), destination.Yto0(), Progress) + Vector3.up * def.Altitude;

        //记录初始飞行时长与抛物线高度，之后目标移动只改变落点。
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false,
            Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            UpdateDestination();
            ticksToImpact = Mathf.Max(1, Mathf.CeilToInt(StartingTicksToImpact));
            flightTicks = ticksToImpact;
            arcHeight = Mathf.Min(def.projectile.arcHeightFactor, (destination - origin).MagnitudeHorizontal() * 0.2f);
        }

        //沿用原版飞行和护盾拦截检查，目标失效时保留最后有效落点。
        protected override void TickInterval(int delta)
        {
            UpdateDestination();
            base.TickInterval(delta);
        }

        //只跟踪仍在本地图的存活锁定目标。
        private bool UpdateDestination()
        {
            Thing target = attack?.target;
            if (target == null || !target.Spawned || target.Map != Map || target is Pawn pawn && pawn.Dead) return false;
            destination = target.DrawPos.Yto0();
            return true;
        }

        //到达目标当前格时仅执行一次技能范围结算，不调用原版爆炸伤害。
        protected override void ImpactSomething()
        {
            bool validTarget = UpdateDestination();
            Position = DestinationCell;
            Impact(validTarget ? attack.target : null);
        }

        //爆炸声音随真正落地播放，被护盾拦截时不播放落地爆炸。
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (!blockedByShield) attack?.action.damageDef.soundExplosion?.PlayOneShot(new TargetInfo(Position, Map));
            base.Impact(hitThing, blockedByShield);
        }

        //用固定进度绘制抛物线及地面阴影，避免目标移动令弹道高度倒退。
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            float height = arcHeight * GenMath.InverseParabola(Progress);
            if (def.projectile.shadowSize > 0f)
            {
                float size = def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
                Vector3 shadowPosition = drawLoc;
                shadowPosition.y = AltitudeLayer.Shadows.AltitudeFor();
                Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(shadowPosition, Quaternion.identity,
                    new Vector3(size, 1f, size)), Shadow, 0);
            }
            Quaternion rotation = def.projectile.spinRate == 0f ? ExactRotation :
                Quaternion.AngleAxis(Find.TickManager.TicksGame * def.projectile.spinRate / 60f, Vector3.up);
            Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), drawLoc + Vector3.forward * height,
                rotation, DrawMat, 0);
            Comps_PostDraw();
        }

        //读档保留剩余飞行时间、初始进度基准与最后有效落点。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref flightTicks, "flightTicks");
            Scribe_Values.Look(ref arcHeight, "arcHeight");
        }
    }
}
