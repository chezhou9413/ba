using UnityEngine;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //星野移动载体，负责可存档的平面位移和抵达后的坦克技能结算。
    public class HoshinoMovementFlyer : PawnFlyer
    {
        public Hediff_SpecialSkillState state;
        public float speed = 0.35f;
        public override Vector3 DrawPos => Vector3.Lerp(startVec, DestinationPos,
            Mathf.Clamp01((float)ticksFlying / Mathf.Max(1, ticksFlightTime)));

        //按配置速度确定位移时间，读档保留原有进度。
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
                ticksFlightTime = Mathf.Max(1, Mathf.CeilToInt(startVec.ToIntVec3().DistanceTo(DestinationPos.ToIntVec3()) / speed));
        }

        //恢复角色后使用已保存的状态引用施加抵达效果。
        protected override void RespawnPawn()
        {
            base.RespawnPawn();
            if (state != null && !state.pawn.Dead && state.pawn.Spawned) HoshinoSkills.Arrive(state);
        }

        //保存抵达操作所需引用，避免使用不能序列化的委托。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref state, "specialState");
            Scribe_Values.Look(ref speed, "speed", 0.35f);
        }
    }
}
