using UnityEngine;
using Verse;

namespace SandWormLib
{
    public sealed class SandWormBlackboard : IExposable
    {
        public Thing CurrentTarget;
        public bool ChargeLocked;
        public Vector3 ChargeDirection = Vector3.zero;
        public int LastBehaviorChangeTick = -999999;
        public int LastDamageTick = -999999;
        public int LastChargeTargetAcquireTick = -999999;

        public void ExposeData()
        {
            Scribe_References.Look(ref CurrentTarget, "currentTarget");
            Scribe_Values.Look(ref ChargeLocked, "chargeLocked", false);
            Scribe_Values.Look(ref ChargeDirection, "chargeDirection");
            Scribe_Values.Look(ref LastBehaviorChangeTick, "lastBehaviorChangeTick", -999999);
            Scribe_Values.Look(ref LastDamageTick, "lastDamageTick", -999999);
            Scribe_Values.Look(ref LastChargeTargetAcquireTick, "lastChargeTargetAcquireTick", -999999);
        }
    }
}
