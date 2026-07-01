using System.Collections.Generic;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 战斗伤害显示状态，负责按目标和伤害顺序缓存暴击结果，避免连发伤害互相覆盖。
    public static class BattleDamageDisplayState
    {
        private static readonly Dictionary<int, Queue<bool>> ManualCritStates = new Dictionary<int, Queue<bool>>();
        private static readonly Dictionary<int, Queue<bool>> PendingCriticalFloatTexts = new Dictionary<int, Queue<bool>>();
        private static readonly Dictionary<int, Queue<float>> PendingForcedCriticalFloatAmounts = new Dictionary<int, Queue<float>>();

        // 注册统一战斗层伤害，负责阻止原版伤害补丁重复暴击。
        public static void RegisterManualDamage(Thing target, Thing instigator, bool isCrit)
        {
            if (target == null || !(target is Pawn))
            {
                return;
            }

            EnqueueValue(ManualCritStates, target.thingIDNumber, isCrit);
        }

        // 注册待显示暴击飘字，负责让 PostApplyDamage 使用真实造成伤害显示文本。
        public static void RegisterCriticalFloatText(Thing target, bool isCrit)
        {
            if (target == null || !(target is Pawn))
            {
                return;
            }

            EnqueueValue(PendingCriticalFloatTexts, target.thingIDNumber, isCrit);
        }

        // 注册强制暴击文字显示的固定数值，负责让必定弹字技能显示公式伤害而不是部位实际受伤值。
        public static void RegisterForcedCriticalFloatAmount(Thing target, float amount)
        {
            if (target == null || !(target is Pawn))
            {
                return;
            }

            EnqueueValue(PendingForcedCriticalFloatAmounts, target.thingIDNumber, amount);
        }

        // 读取统一战斗层暴击状态，负责按伤害进入 PreApplyDamage 的顺序消费队列。
        public static bool TryConsumeManualCritState(Thing target, out bool isCrit)
        {
            isCrit = false;
            if (target == null)
            {
                return false;
            }

            return TryDequeueValue(ManualCritStates, target.thingIDNumber, out isCrit);
        }

        // 读取待显示暴击飘字状态，负责在真实伤害回调后消费状态。
        public static bool TryConsumeCriticalFloatText(Thing target, out bool isCrit)
        {
            isCrit = false;
            if (target == null)
            {
                return false;
            }

            return TryDequeueValue(PendingCriticalFloatTexts, target.thingIDNumber, out isCrit);
        }

        // 读取强制暴击文字显示的固定数值，负责在实际伤害回调后优先使用公式伤害弹字。
        public static bool TryConsumeForcedCriticalFloatAmount(Thing target, out float amount)
        {
            amount = 0f;
            if (target == null)
            {
                return false;
            }

            return TryDequeueValue(PendingForcedCriticalFloatAmounts, target.thingIDNumber, out amount);
        }

        // 丢弃一次未落地伤害的显示状态，负责处理闪避、护盾或其他 PreApplyDamage 阶段取消伤害的情况。
        public static void DiscardPendingDamageDisplay(Thing target)
        {
            if (target == null)
            {
                return;
            }

            int targetId = target.thingIDNumber;
            TryDequeueValue(ManualCritStates, targetId, out bool _);
            TryDequeueValue(PendingCriticalFloatTexts, targetId, out bool _);
            TryDequeueValue(PendingForcedCriticalFloatAmounts, targetId, out float _);
        }

        // 入队指定目标的显示状态，负责保留同一目标连续多次伤害的先后顺序。
        private static void EnqueueValue<T>(Dictionary<int, Queue<T>> states, int targetId, T value)
        {
            if (!states.TryGetValue(targetId, out Queue<T> queue))
            {
                queue = new Queue<T>();
                states[targetId] = queue;
            }

            queue.Enqueue(value);
        }

        // 出队指定目标的一条显示状态，负责在队列清空后移除目标缓存。
        private static bool TryDequeueValue<T>(Dictionary<int, Queue<T>> states, int targetId, out T value)
        {
            value = default(T);
            if (!states.TryGetValue(targetId, out Queue<T> queue) || queue.Count == 0)
            {
                return false;
            }

            value = queue.Dequeue();
            if (queue.Count == 0)
            {
                states.Remove(targetId);
            }

            return true;
        }
    }
}
