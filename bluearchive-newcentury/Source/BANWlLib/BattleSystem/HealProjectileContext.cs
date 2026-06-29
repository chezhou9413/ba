using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 子弹治疗施法者快照上下文，负责在子弹命中附加治疗 Hediff 时把施法者属性传递给 Hediff_Regeneration。
    public static class HealProjectileContext
    {
        // 按 Pawn thingIDNumber 缓存最近一次伤害的施法者快照，供附加 Hediff 时读取。
        private static readonly Dictionary<int, BattleCasterSnapshot> snapshotByTargetId = new Dictionary<int, BattleCasterSnapshot>();

        // 注册施法者快照，负责在子弹伤害命中目标前缓存施法者属性。
        public static void Register(Pawn target, BattleCasterSnapshot snapshot)
        {
            if (target == null || snapshot == null)
            {
                return;
            }

            snapshotByTargetId[target.thingIDNumber] = snapshot;
        }

        // 尝试取出并移除施法者快照，负责让附加的 Hediff_Regeneration 获得施法者属性后清理缓存。
        public static bool TryConsume(Pawn target, out BattleCasterSnapshot snapshot)
        {
            snapshot = null;
            if (target == null)
            {
                return false;
            }

            if (snapshotByTargetId.TryGetValue(target.thingIDNumber, out snapshot))
            {
                snapshotByTargetId.Remove(target.thingIDNumber);
                return true;
            }

            return false;
        }

        // 清理目标残留快照，负责避免 Hediff 未成功创建时把旧快照留到下一次治疗。
        public static void Clear(Pawn target)
        {
            if (target == null)
            {
                return;
            }

            snapshotByTargetId.Remove(target.thingIDNumber);
        }
    }
}
