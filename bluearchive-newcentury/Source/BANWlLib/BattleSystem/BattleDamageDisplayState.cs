using System.Collections.Generic;
using Verse;

namespace BANWlLib.BattleSystem
{
    public static class BattleDamageDisplayState
    {
        private static readonly Dictionary<int, bool> ManualCritStates = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> PendingCriticalFloatTexts = new Dictionary<int, bool>();

        // 注册统一战斗层伤害，负责阻止原版伤害补丁重复暴击。
        public static void RegisterManualDamage(Thing target, Thing instigator, bool isCrit)
        {
            if (target == null || !(target is Pawn))
            {
                return;
            }

            ManualCritStates[target.thingIDNumber] = isCrit;
        }

        // 注册待显示暴击飘字，负责让 PostApplyDamage 使用真实造成伤害显示文本。
        public static void RegisterCriticalFloatText(Thing target, bool isCrit)
        {
            if (target == null || !(target is Pawn))
            {
                return;
            }

            if (isCrit)
            {
                PendingCriticalFloatTexts[target.thingIDNumber] = true;
            }
            else
            {
                PendingCriticalFloatTexts.Remove(target.thingIDNumber);
            }
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

        // 读取待显示暴击飘字状态，负责在真实伤害回调后消费状态。
        public static bool TryConsumeCriticalFloatText(Thing target, out bool isCrit)
        {
            isCrit = false;
            if (target == null)
            {
                return false;
            }

            if (PendingCriticalFloatTexts.TryGetValue(target.thingIDNumber, out isCrit))
            {
                PendingCriticalFloatTexts.Remove(target.thingIDNumber);
                return true;
            }

            return false;
        }
    }
}
