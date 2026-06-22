using System.Collections.Generic;
using Verse;

namespace BANWlLib.BattleSystem
{
    public static class BattleDamageDisplayState
    {
        private static readonly Dictionary<int, bool> ManualCritStates = new Dictionary<int, bool>();

        public static void RegisterManualDamage(Thing target, Thing instigator, bool isCrit)
        {
            if (target == null || !(target is Pawn))
            {
                return;
            }

            ManualCritStates[target.thingIDNumber] = isCrit;
        }

        public static bool TryConsumeManualCritState(Thing target, out bool isCrit)
        {
            isCrit = false;
            if (target == null)
            {
                return false;
            }

            if (ManualCritStates.TryGetValue(target.thingIDNumber, out isCrit))
            {
                ManualCritStates.Remove(target.thingIDNumber);
                return true;
            }

            return false;
        }
    }
}
